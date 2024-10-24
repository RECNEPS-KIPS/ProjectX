//--------------------------------------------------------------------------//
// Copyright 2023-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

#if UIFX_TMPRO

#if UNITY_2022_3_OR_NEWER
	#define UIFX_SUPPORTS_VERTEXCOLORALWAYSGAMMASPACE
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityInternal = UnityEngine.Internal;
using TMPro;
using ChocDino.UIFX;

namespace ChocDino.UIFX
{
	/// <summary>
	/// Allows multiple image filters derived from FilterBase to be applied to TextMeshPro
	/// Tested with TextMeshPro v2.1.6 (Unity 2019), v3.0.8 (Unity 2020), 3.2.0-pre.9 (Unity 2022)
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(TextMeshProUGUI))]
	[HelpURL("https://www.chocdino.com/products/unity-assets/")]
	[AddComponentMenu("UI/Chocolate Dinosaur UIFX/Filters/UIFX - Filter Stack (TextMeshPro)", 200)]
	public class FilterStackTextMeshPro : UIBehaviour
	{
		private Graphic _graphic;
		private Graphic GraphicComponent { get { if (_graphic == null) _graphic = GetComponent<Graphic>(); return _graphic; } }

		private TextMeshProUGUI _textMeshPro;

		private List<TMP_SubMeshUI> _subMeshes = new List<TMP_SubMeshUI>(8);
		private static List<TMP_SubMeshUI> _subMeshTemp = new List<TMP_SubMeshUI>(8);

		protected static class ShaderProp
		{
			public readonly static int SourceTex = Shader.PropertyToID("_SourceTex");
			public readonly static int ResultTex = Shader.PropertyToID("_ResultTex");
		}

		private const string BlendShaderPath = "Hidden/ChocDino/UIFX/Blend";

		private ScreenRectFromMeshes _screenRect = new ScreenRectFromMeshes();
		private Compositor _composite = new Compositor();
		private RenderTexture _rt;
		private RenderTexture _rt2;
		private Material _displayMaterial;
		private VertexHelper _quadVertices;
		private List<Color> _vertexColors;
		private Mesh _quadMesh;
		private int _lastRenderFrame = -1;
		private bool _needsRendering = true;

		[SerializeField] bool _applyToSprites = true;
		[SerializeField] bool _updateOnTransform = true;
		[SerializeField] bool _relativeToTransformScale = false;
		[SerializeField, Delayed] float _relativeFontSize = 0f;
		[SerializeField] FilterBase[] _filters = null;

		public bool ApplyToSprites { get { return _applyToSprites; } set { ChangeProperty(ref _applyToSprites, value); } }
		public bool UpdateOnTransform { get { return _updateOnTransform; } set { ChangeProperty(ref _updateOnTransform, value); } }

		private bool CanApplyFilter()
		{
			if (!this.isActiveAndEnabled) return false;
			if (!GraphicComponent.enabled) return false;
			bool result = false;
			if (_filters != null)
			{
				// See if any filters are rendering
				foreach (var filter in _filters)
				{
					if (filter && filter.IsFiltered())
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		[UnityInternal.ExcludeFromDocs]
		protected override void Awake()
		{
			_textMeshPro = GetComponent<TextMeshProUGUI>();
			base.Awake();
		}

		/// <summary>
		/// NOTE: OnDidApplyAnimationProperties() is called when the Animator is used to keyframe properties
		/// </summary>
		protected override void OnDidApplyAnimationProperties()
		{
			GraphicComponent.SetAllDirty();
			base.OnDidApplyAnimationProperties();
		}

		#if UNITY_EDITOR
		protected override void Reset()
		{
			GraphicComponent.SetAllDirty();
			base.Reset();
		}
		protected override void OnValidate()
		{
			GraphicComponent.SetAllDirty();
		}
		#endif

		protected void ChangeProperty<T>(ref T backing, T value) where T : struct
		{
			if (ObjectHelper.ChangeProperty(ref backing, value))
			{
				backing = value;
				GraphicComponent.SetAllDirty();
			}
		}

		[UnityInternal.ExcludeFromDocs]
		protected override void OnEnable()
		{
			_needsRendering = true;
			var shader = Shader.Find(BlendShaderPath);
			if (shader)
			{
				_displayMaterial = new Material(shader);
			}
			_quadVertices = new VertexHelper();

			GraphicComponent.RegisterDirtyMaterialCallback(OnGraphicMaterialDirtied);
			GraphicComponent.RegisterDirtyVerticesCallback(OnGraphicVerticesDirtied);

			TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextGeomeryRebuilt);
			//CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
			//_textMeshPro.OnPreRenderText += OnTextVerticesChanged;
			Canvas.willRenderCanvases += WillRenderCanvases;

			// This forces TMP to re-render
			_textMeshPro.SetAllDirty();
			
			base.OnEnable();
		}

		[UnityInternal.ExcludeFromDocs]
		protected override void OnDisable()
		{
			ObjectHelper.Destroy(ref _quadMesh);
			RenderTextureHelper.ReleaseTemporary(ref _rt2);
			RenderTextureHelper.ReleaseTemporary(ref _rt);
			_composite.FreeResources();

			GraphicComponent.UnregisterDirtyMaterialCallback(OnGraphicMaterialDirtied);
			GraphicComponent.UnregisterDirtyVerticesCallback(OnGraphicVerticesDirtied);

			Canvas.willRenderCanvases -= WillRenderCanvases;
			//_textMeshPro.OnPreRenderText -= OnTextVerticesChanged;
			//CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
			TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextGeomeryRebuilt);

			ObjectHelper.Dispose(ref _quadVertices);
			ObjectHelper.Destroy(ref _displayMaterial);

			// This forces TMP to re-render
			_textMeshPro.SetAllDirty();

			base.OnDisable();
		}

		protected void OnGraphicMaterialDirtied()
		{
			_needsRendering = true;
		}
		protected void OnGraphicVerticesDirtied()
		{
			_needsRendering = true;
		}

		//void OnTextVerticesChanged(TMP_TextInfo textInfo) {}
		
		void OnTextGeomeryRebuilt(Object obj)
		{
			if (obj == _textMeshPro)
			{	
				_needsRendering = true;

				// For some reason we need to set things dirty here so that future changes to the filters will cause rerendering
				GraphicComponent.SetAllDirty();
			}
		}

		void GatherSubMeshes()
		{
			GetComponentsInChildren<TMP_SubMeshUI>(false, _subMeshTemp);

			// SubMesh GameObject ordering doesn't always match the meshInfo order, so we have to reorder it
			_subMeshes.Clear();
			_subMeshes.Add(null);
			for (int i = 1; i < _textMeshPro.textInfo.materialCount; i++)
			{
				var meshInfo = _textMeshPro.textInfo.meshInfo[i];
				for (int j = 0; j < _subMeshTemp.Count; j++)
				{
					if (_subMeshTemp[j].mesh == meshInfo.mesh)
					{
						_subMeshes.Add(_subMeshTemp[j]);
						break;
					}
				}
			}
			Debug.Assert(_subMeshes.Count == _textMeshPro.textInfo.materialCount);
		}

		void CalculateScreenRect()
		{
			_screenRect.Start(GetRenderCamera());

			// Grow rectangle from geometry
			if (_textMeshPro)
			{
				int materialCount = _textMeshPro.textInfo.materialCount;

				if (materialCount > 1)
				{
					GatherSubMeshes();
				}

				for (int i = 0; i < materialCount; i++)
				{
					var meshInfo = _textMeshPro.textInfo.meshInfo[i];
					if (meshInfo.vertexCount > 0)
					{
						// Check if we need to skip the sprite
						if (i > 0 && !_applyToSprites)
						{
							var subMesh = _subMeshes[i];
							bool isSprite = (subMesh.spriteAsset != null);
							if (isSprite) { continue; }
						}
						_screenRect.AddVertexBounds(this.transform, meshInfo.mesh.vertices, meshInfo.mesh.vertexCount);
					}
				}
			}
			
			_screenRect.End();

			RectAdjustOptions rectAdjustOptions = new RectAdjustOptions();

			// Grow rectangle for filters (if any)
			if (_filters != null)
			{
				foreach (var filter in _filters)
				{
					if (filter && filter.IsFiltered())
					{
						// Apply relative scale based on font size and transform scale
						{
							float userScale = 1f;
							if (_relativeToTransformScale)
							{
								float canvasScale = 1f;
								if (_textMeshPro.canvas)
								{
									canvasScale = _textMeshPro.canvas.transform.localScale.x;
								}
								userScale *= filter.transform.lossyScale.x / canvasScale;
							}
							if (_relativeFontSize > 0f)
							{
								userScale *= (_textMeshPro.fontSize / _relativeFontSize);
							}
							filter.UserScale = userScale;
						}

						filter.AdjustRect(_screenRect);

						rectAdjustOptions.padding = Mathf.Max(rectAdjustOptions.padding, filter.RectAdjustOptions.padding);
						rectAdjustOptions.roundToNextMultiple = Mathf.Max(rectAdjustOptions.roundToNextMultiple, filter.RectAdjustOptions.roundToNextMultiple);
					}
				}
			}

			_screenRect.OptimiseRects(rectAdjustOptions);

			if (_filters != null)
			{
				foreach (var filter in _filters)
				{
					if (filter && filter.IsFiltered())
					{
						filter.SetFinalRect(_screenRect);
					}
				}
			}
		}

		private void SetupMaterialTMPro(Material material, Camera camera, RectInt textureRect)
		{
			float sw = textureRect.width;
			float sh = textureRect.height;
			if (camera == null)
			{
				// For some reason in Overlay mode we have to multiply these by 2 to get correct rendering otherwise
				// the result is fuzzy..Need to figure out why though.
				sw *= 2f;
				sh *= 2f;
			}
			Shader.SetGlobalVector(UnityShaderProp.ScreenParams, new Vector4(sw, sh, 1f + (1f / sw), 1f + (1f / sh)));

			//float a = (!camera || camera.orthographic) ? 0f : 0.875f;
			//material.SetFloat(ShaderUtilities.ID_PerspectiveFilter, a);

			material.SetFloat(ShaderUtilities.ID_PerspectiveFilter, 0f);
		}

		private void HandleVertexColors(Mesh mesh, Material material, bool vertexColorAlwaysGammaSpace, bool isSprite)
		{
			// We only have to adjust vertex color processing in Linear color-space
			if (QualitySettings.activeColorSpace == ColorSpace.Linear)
			{
				if (isSprite && !_textMeshPro.tintAllSprites)
				{
					// If the sprite isn't tinted then it'll have white vertex colors which don't need adjusting
					return;
				}

				// If Canvas.vertexColorAlwaysGammaSpace is supported then we just need to tell the shader to do the conversion to linear
				#if UIFX_SUPPORTS_VERTEXCOLORALWAYSGAMMASPACE
				{
					material.SetInt(UnityShaderProp.UIVertexColorAlwaysGammaSpace, vertexColorAlwaysGammaSpace ? 1 : 0);
				}
				#endif

				if (!vertexColorAlwaysGammaSpace)
				{
					// TMPro doesn't convert vertex color to linear, so we have to do this manually
					// (or perhaps this happens automatically when it's rendered by the UI system)
					ColorUtils.ConvertMeshVertexColorsToLinear(mesh, ref _vertexColors);
				}
			}
		}

		void RenderToTexture()
		{
			// Composite the TMP to a RenderTexture
			Camera canvasCamera = GetRenderCamera();
			
			if (_composite.Start(canvasCamera, _screenRect.GetTextureRect()))
			{
				bool vertexColorAlwaysGammaSpace = false;
				#if UIFX_SUPPORTS_VERTEXCOLORALWAYSGAMMASPACE
					vertexColorAlwaysGammaSpace = _textMeshPro.canvas.vertexColorAlwaysGammaSpace;
				#endif

				for (int i = 0; i < _textMeshPro.textInfo.materialCount; i++)
				{
					TMP_MeshInfo meshInfo = _textMeshPro.textInfo.meshInfo[i];
					if (meshInfo.vertexCount > 0)
					{
						if (i == 0)
						{
							// First mesh is always text
							HandleVertexColors(meshInfo.mesh, _textMeshPro.fontSharedMaterial, vertexColorAlwaysGammaSpace, false);
							SetupMaterialTMPro(_textMeshPro.fontSharedMaterial, canvasCamera, _screenRect.GetTextureRect());
							_composite.AddMesh(this.transform, meshInfo.mesh, _textMeshPro.fontSharedMaterial, true);
						}
						else
						{
							// SubMesh can be text or sprites
							var subMesh = _subMeshes[i];
							bool isSprite = (subMesh.spriteAsset != null);

							// Check if we need to skip the sprite
							if (isSprite && !_applyToSprites) { continue; }
							
							{
								HandleVertexColors(meshInfo.mesh, meshInfo.material, vertexColorAlwaysGammaSpace, isSprite);
								if (!isSprite)
								{
									SetupMaterialTMPro(meshInfo.material, canvasCamera, _screenRect.GetTextureRect());
								}
								_composite.AddMesh(this.transform, meshInfo.mesh, meshInfo.material, false);

								// Hide the submesh
								subMesh.canvasRenderer.SetMesh(null);
							}
						}
					}
				}
				_composite.End();

				var sourceTexture = _composite.GetTexture();

				// Render the filters (if any)
				if (_filters != null && sourceTexture)
				{
					if (_rt && (_rt.width != sourceTexture.width || _rt.height != sourceTexture.height))
					{
						RenderTextureHelper.ReleaseTemporary(ref _rt);
					}
					if (_rt2 && (_rt2.width != sourceTexture.width || _rt2.height != sourceTexture.height))
					{
						RenderTextureHelper.ReleaseTemporary(ref _rt2);
					}

					int activeFilterCount = 0;
					foreach (var filter in _filters)
					{
						if (filter && filter.IsFiltered())
						{
							activeFilterCount++;
						}
					}

					if (activeFilterCount > 0)
					{

						if (!_rt)
						{
							_rt = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, sourceTexture.format, RenderTextureReadWrite.Linear);
							#if UNITY_EDITOR
							_rt.name = "FilterStack-Output1";
							#endif
						}
						if (!_rt2 && activeFilterCount > 1)
						{
							_rt2 = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, sourceTexture.format, RenderTextureReadWrite.Linear);
							#if UNITY_EDITOR
							_rt2.name = "FilterStack-Output2";
							#endif
						}

						RenderTexture destTexture = _rt;
						foreach (var filter in _filters)
						{
							if (filter && filter.IsFiltered())
							{
								if (filter.RenderToTexture(sourceTexture, destTexture))
								{
									sourceTexture = destTexture;
									destTexture = (sourceTexture == _rt)?_rt2:_rt;
								}
							}
						}
					}
				}

				// Release unused temporary textures
				if (sourceTexture == _rt)
				{
					RenderTextureHelper.ReleaseTemporary(ref _rt2);
				}
				else if (sourceTexture == _rt2)
				{
					RenderTextureHelper.ReleaseTemporary(ref _rt);
				}
				else
				{
					RenderTextureHelper.ReleaseTemporary(ref _rt2);
					RenderTextureHelper.ReleaseTemporary(ref _rt);
				}

				// Update the display material
				if (sourceTexture)
				{
					if (_displayMaterial)
					{
						_displayMaterial.mainTexture = sourceTexture;
						_displayMaterial.SetTexture(ShaderProp.SourceTex, sourceTexture);
						_displayMaterial.SetTexture(ShaderProp.ResultTex, sourceTexture);
					}
				}
			}
		}

		private Matrix4x4 _previousLocalToWorldMatrix;
		private Matrix4x4 _previousCameraMatrix;

		protected virtual void Update()
		{
			bool forceUpdate = false;

			if (_updateOnTransform)
			{
				// Detect a change to the matrix (this also detects changes to the camera and viewport)
				if (MathUtils.HasMatrixChanged(_previousLocalToWorldMatrix, this.transform.localToWorldMatrix, false))
				{
					_previousLocalToWorldMatrix = this.transform.localToWorldMatrix;
					forceUpdate = true;
				}
				if (_textMeshPro.canvas && _textMeshPro.canvas.renderMode == RenderMode.WorldSpace)
				{
					Camera camera = GetRenderCamera();
					if (camera)
					{
						if (MathUtils.HasMatrixChanged(_previousCameraMatrix, camera.transform.localToWorldMatrix, ignoreTranslation:false))
						{
							_previousCameraMatrix = camera.transform.localToWorldMatrix;
							forceUpdate = true;
						}
					}
				}
			}

			#if UIFX_FILTERS_FORCE_UPDATE_PLAYMODE
			if (Application.isPlaying)
			{
				forceUpdate = true;
			}
			#endif
			#if UIFX_FILTERS_FORCE_UPDATE_EDITMODE
			if (!Application.isPlaying)
			{
				forceUpdate = true;
			}
			#endif

			if (forceUpdate)
			{
				GraphicComponent.SetVerticesDirty();
				GraphicComponent.SetMaterialDirty();
			}
		}

		void WillRenderCanvases()
		{
			if (!CanApplyFilter())
			{
				return;
			}

			if (_lastRenderFrame != Time.frameCount)
			{
				if (HasActiveFilters())
				{
					// Prevent re-rendering unnecessarily
					_lastRenderFrame = Time.frameCount;

					// Do the rendering
					if (_needsRendering)
					{
						CalculateScreenRect();
						RenderToTexture();
						ApplyOutputMeshAndMaterial();
						_needsRendering = false;
					}
					else
					{
						ApplyPreviousOutput();
					}
				}
			}
		}

		private void ApplyOutputMeshAndMaterial()
		{
			_screenRect.BuildScreenQuad(GetRenderCamera(), this.transform, 1f, _quadVertices);

			if (_quadMesh == null)
			{
				_quadMesh = new Mesh();
			}
			_quadVertices.FillMesh(_quadMesh);
			
			if (_displayMaterial)
			{
				// Copy the stencil masking properties
				if (_textMeshPro.maskable)
				{
					UnityShaderProp.CopyStencilProperties(_textMeshPro.GetModifiedMaterial(_textMeshPro.fontSharedMaterial), _displayMaterial);
				}

				var cr = _textMeshPro.canvasRenderer;
				cr.SetMesh(_quadMesh);
				cr.materialCount = 1;
				cr.SetMaterial(_displayMaterial, 0);
			}
		}

		private void ApplyPreviousOutput()
		{		
			if (_displayMaterial)
			{
				var cr = _textMeshPro.canvasRenderer;
				cr.SetMesh(_quadMesh);
				cr.materialCount = 1;
				cr.SetMaterial(_displayMaterial, 0);
			}
		}


		private Camera GetRenderCamera()
		{
			Camera camera = null;
			Canvas canvas = _textMeshPro.canvas;
			if (canvas)
			{
				camera = canvas.worldCamera;
				if (camera == null && canvas.renderMode == RenderMode.WorldSpace)
				{
					camera = Camera.main;

					#if UNITY_EDITOR
					// NOTE: if we're in the "in-context" prefab editing mode, it uses a World Space canvas with no camera set.
					// when the original scene camera for the canvas was in Overlay mode, it would cause the filter to not render,
					// because the Camera.main camera would be used, and it wouldn't be looking at the UI component.  So instead
					// we detect this case and just use null camera as if it were in overlay mode.  Not sure how robust this is...
					if (EditorHelper.IsInContextPrefabMode())
					{
						camera = null;
					}
					#endif
				}
				else if (camera == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					camera = null;
				}
			}
			return camera;
		}

		private bool HasActiveFilters()
		{
			bool result = false;
			if (_filters != null)
			{
				for (int i = 0; i < _filters.Length; i++)
				{
					if (_filters[i] != null)
					{
						if (_filters[i].enabled)
						{
							result = true;
							break;
						}
					}
				}
			}
			return result;
		}

		protected void LOG(string message, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "")
		{
			ChocDino.UIFX.Log.LOG(message, this, LogType.Log, callerName);
		}

		protected void LOGFUNC(string message = null, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "")
		{
			if (string.IsNullOrEmpty(message))
			{
				ChocDino.UIFX.Log.LOG(callerName, this, LogType.Log, callerName);
			}
			else
			{
				ChocDino.UIFX.Log.LOG(callerName + " " + message, this, LogType.Log, callerName);
			}
		}		
	}
}
#endif