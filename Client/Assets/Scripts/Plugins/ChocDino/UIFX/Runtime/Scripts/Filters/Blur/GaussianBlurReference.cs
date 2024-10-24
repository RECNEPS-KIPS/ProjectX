//--------------------------------------------------------------------------//
// Copyright 2023-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using UnityEngine;

namespace ChocDino.UIFX
{
	public class GaussianBlurReference : ITextureBlur
	{
		internal class BlurShader
		{
			internal const string Id = "Hidden/ChocDino/UIFX/GaussianBlur-Reference";

			internal static class Prop
			{
				internal static readonly int KernelRadius = Shader.PropertyToID("_KernelRadius");
				internal static readonly int Weights = Shader.PropertyToID("_Weights");

			}
			internal static class Pass
			{
				internal const int Horizontal = 0;
				internal const int Vertical = 1;
			}
		}

		public BlurAxes2D BlurAxes2D { get { return _blurAxes2D; } set { _blurAxes2D = value; } }
		public Downsample Downsample { get { return _downSample; } set { if (_downSample != value) { _downSample = value; _kernelDirty = _materialDirty = true; } } }

		private Downsample _downSample = Downsample.Auto;
		private float _blurSize = 0.05f;
		private Material _material;
		private RenderTexture _sourceTexture;
		private RenderTexture _rtBlurH;
		private RenderTexture _rtBlurV;
		private BlurAxes2D _blurAxes2D = BlurAxes2D.Default;
		private const int MaxRadius = 512;
		private float[] _weights;

		private bool _kernelDirty = true;
		private bool _materialDirty = true;
		private FilterBase _parentFilter = null;

		private GaussianBlurReference() { }

		public GaussianBlurReference(FilterBase parentFilter)
		{
			Debug.Assert(parentFilter != null);
			_parentFilter = parentFilter;
		}

		public void ForceDirty()
		{
			_kernelDirty = _materialDirty = true;
		}

		public void SetBlurSize(float diagonalPercent)
		{
			if (diagonalPercent != _blurSize)
			{
				_blurSize = diagonalPercent;
				_kernelDirty = _materialDirty = true;
			}
		}

		public void AdjustBoundsSize(ref Vector2Int leftDown, ref Vector2Int rightUp)
		{
			// Get radius for box blur
			float radius = GetScaledRadius();

			// NOTE: This size is based off a solid white box which is the worst case, if
			// the contents of the image is dark then this radius could be significantly shrunk,
			// but there is no easy way to detect this. Also if the image is HDR then this expand
			// may be too small - perhaps expose option to the user.
			radius *= GetDownsampleFactor();

			int x = Mathf.CeilToInt(radius);
			if (x > 0)
			{
				Vector2Int result = new Vector2Int(x, x);
				if (_blurAxes2D == BlurAxes2D.Horizontal)
				{
					result.y = 0;
				}
				else if (_blurAxes2D == BlurAxes2D.Vertical)
				{
					result.x = 0;
				}
				leftDown += result;
				rightUp += result;
			}
		}

		public RenderTexture Process(RenderTexture sourceTexture)
		{
			Debug.Assert(sourceTexture != null);

			RenderTexture prevRT = RenderTexture.active;

			SetupResources(sourceTexture);

			if (_kernelDirty)
			{
				UpdateKernel();
			}
			if (_materialDirty)
			{
				UpdateMaterial();
			}

			RenderTexture src = _sourceTexture;
			if (GetScaledRadius() > 0f)
			{
				// Have to downsample first otherwise it will be biased in the first blur pass direction
				// leading to slightly stretched result
				if (GetDownsampleFactor() > 1)
				{
					Graphics.Blit(src, _rtBlurV);
					_rtBlurV.IncrementUpdateCount();
					src = _rtBlurV;
				}

				// Blur
				{
					if (_blurAxes2D == BlurAxes2D.Default)
					{
						Graphics.Blit(src, _rtBlurH, _material, BlurShader.Pass.Horizontal);
						_rtBlurH.IncrementUpdateCount();
						Graphics.Blit(_rtBlurH, _rtBlurV, _material, BlurShader.Pass.Vertical);
						_rtBlurV.IncrementUpdateCount();
						src = _rtBlurV;
					}
					else
					{
						int pass = (_blurAxes2D == BlurAxes2D.Horizontal) ? BlurShader.Pass.Horizontal : BlurShader.Pass.Vertical;
						var dst = _rtBlurH;

						Graphics.Blit(src, dst, _material, pass);
						dst.IncrementUpdateCount();
						src = dst;
					}
				}
			}
			else
			{
				FreeTextures();
			}

			RenderTexture.active = prevRT;

			return src;
		}

		public void FreeResources()
		{
			FreeShaders();
			FreeTextures();
		}

		private uint _currentTextureHash;

		private uint CreateTextureHash(int width, int height)
		{
			uint hash = 0;
			hash = (hash | (uint)width) << 13;
			hash = (hash | (uint)height) << 13;
			return hash;
		}

		void SetupResources(RenderTexture sourceTexture)
		{
			uint desiredTextureProps = 0;
			if (sourceTexture != null)
			{
				desiredTextureProps = CreateTextureHash(sourceTexture.width / GetDownsampleFactor(), sourceTexture.height / GetDownsampleFactor());
			}

			if (desiredTextureProps != _currentTextureHash)
			{
				FreeTextures();
				_materialDirty = true;
			}
			if (_sourceTexture == null && sourceTexture != null)
			{
				CreateTextures(sourceTexture);
				_currentTextureHash = desiredTextureProps;
			}
			
			if (_sourceTexture != sourceTexture)
			{
				_materialDirty = true;
				_sourceTexture = sourceTexture;
			}
			if (_material == null)
			{
				CreateShaders();
			}
		}

		private float GetScaledRadius()
		{
			float radius = _blurSize;
			radius *= _parentFilter.ResolutionScalingFactor;
			radius /= (float)GetDownsampleFactor();
			return radius;
		}

		void UpdateMaterial()
		{
			_material.SetInt(BlurShader.Prop.KernelRadius, _weights.Length);
			_material.SetFloatArray(BlurShader.Prop.Weights, _weights);
			_materialDirty = false;
		}

		static Material CreateMaterialFromShader(string shaderName)
		{
			Material result = null;
			Shader shader = Shader.Find(shaderName);
			if (shader != null)
			{
				result = new Material(shader);
			}
			return result;
		}

		void CreateShaders()
		{
			_material = CreateMaterialFromShader(BlurShader.Id);
			Debug.Assert(_material != null);
			_material.SetFloatArray(BlurShader.Prop.Weights, new float[MaxRadius]);
			_materialDirty = true;
		}

		void CreateTextures(RenderTexture sourceTexture)
		{
			Debug.Assert(sourceTexture.width > 0 && sourceTexture.height > 0);
			int w = Mathf.Max(1, sourceTexture.width / GetDownsampleFactor());
			int h = Mathf.Max(1, sourceTexture.height / GetDownsampleFactor());

			RenderTextureFormat format = sourceTexture.format;
			if ((Filters.PerfHint & PerformanceHint.UseMorePrecision) != 0)
			{
				// TODO: create based on the input texture format, but just with more precision
				format = RenderTextureFormat.ARGBHalf;
			}

			_rtBlurH = RenderTexture.GetTemporary(w, h, 0, format, RenderTextureReadWrite.Linear);
			_rtBlurV = RenderTexture.GetTemporary(w, h, 0, format, RenderTextureReadWrite.Linear);

			#if UNITY_EDITOR
			_rtBlurH.name = "BlurH";
			_rtBlurV.name = "BlurV";
			#endif
		}

		void FreeShaders()
		{
			ObjectHelper.Destroy(ref _material);
		}

		void FreeTextures()
		{
			RenderTextureHelper.ReleaseTemporary(ref _rtBlurV);
			RenderTextureHelper.ReleaseTemporary(ref _rtBlurH);
			_currentTextureHash = 0;
			_sourceTexture = null;
		}

		private int GetDownsampleFactor()
		{
			int result = 1;
			if (_downSample == Downsample.Auto)
			{
				if ((Filters.PerfHint & PerformanceHint.AllowDownsampling) != 0)
				{
					result = 2;
				}
			}
			else
			{
				result = (int)_downSample;
			}

			if (_blurSize > 120f)
			{
				result *= 4;
			}
			else if (_blurSize > 60f)
			{
				result *= 2;
			}
			
			return result;
		}

		// NOTE full kernel size is double this, plus one for the center coordinate
		static int GetHalfKernelSize(float sigma)
		{
			return Mathf.CeilToInt(3.0f * sigma);
		}

		static float GetSigmaFromKernelRadius(float radius)
		{
			return radius / 3f;
		}

		static float GetWeight(int x, float sigma)
		{
			return 1.0f / (Mathf.Sqrt(Mathf.PI * 2.0f) * sigma) * Mathf.Exp(-(x * x) / (2.0f * sigma * sigma));
		}

		void UpdateKernel()
		{
			float radius = GetScaledRadius();
			float sigma = GetSigmaFromKernelRadius(radius);
				
			// Generate weights
			int size = 1 + GetHalfKernelSize(sigma);
			Debug.Assert(size <= MaxRadius);
			_weights = new float[size];
			_weights[0] = GetWeight(0, sigma);
			float total = _weights[0];
			for (int i = 1; i < size; i++)
			{
				_weights[i] = GetWeight(i, sigma);
				total += _weights[i] * 2f;
			}

			// Normalise weights
			for (int i = 0; i < size; i++)
			{
				_weights[i] /= total;
			}
		
			_kernelDirty = false;
			_materialDirty = true;
		}
	}
}