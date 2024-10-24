//--------------------------------------------------------------------------//
// Copyright 2023-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

//#define UIFX_FILTER_SUPPORT_CHILDREN
//#define UIFX_FILTER_TMP
#define UIFX_OLD178_RESOLUTION_SCALING
#if UNITY_EDITOR
	#define UIFX_FILTER_DEBUG
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using UnityInternal = UnityEngine.Internal;
#if UIFX_FILTER_TMP
using TMPro;
#endif

namespace ChocDino.UIFX
{
	/// <summary>
	/// Base class for all derived filter classes
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(CanvasRenderer))]
	[HelpURL("https://www.chocdino.com/products/unity-assets/")]
	public abstract partial class FilterBase : UIBehaviour, IMaterialModifier, IMeshModifier
	{
		[Tooltip("How strongly the effect is applied.")]
		[Range(0f, 1f)]
		[SerializeField] protected float _strength = 1f;

		/// <summary>How much of the maximum effect to apply.  Range [0..1] Default is 1.0</summary>
		public float Strength { get { return _strength; } set { ChangeProperty(ref _strength, Mathf.Clamp01(value)); } }

		/// <summary> Scale all resolution calculations by this ammount.  Useful for allowing font size / transform scale to keep consistent rendering of effects.</summary>
		public float UserScale { get { return _userScale; } set { ChangeProperty(ref _userScale, value); } }

		protected readonly static Vector4 Alpha8TextureAdd = new Vector4(1f, 1f, 1f, 0f);

		protected static class ShaderProp
		{
			public readonly static int SourceTex = Shader.PropertyToID("_SourceTex");
			public readonly static int ResultTex = Shader.PropertyToID("_ResultTex");
		}

		private bool _isGraphicText;
		private Graphic _graphic;
		internal Graphic GraphicComponent { get { if (!_graphic) { _graphic = GetComponent<Graphic>(); _isGraphicText = _graphic is Text; } return _graphic; } }

		private MaskableGraphic _maskableGraphic;
		private MaskableGraphic MaskableGraphicComponent { get { if (!_maskableGraphic) { _maskableGraphic = GraphicComponent as MaskableGraphic; } return _maskableGraphic; } }

		private CanvasRenderer _canvasRenderer;
		private CanvasRenderer CanvasRenderComponent { get { if (!_canvasRenderer) { if (GraphicComponent) { _canvasRenderer = _graphic.canvasRenderer; } else { _canvasRenderer = GetComponent<CanvasRenderer>(); } } return _canvasRenderer; } }

		private Material _baseMaterial;
		protected ScreenRectFromMeshes _screenRect = new ScreenRectFromMeshes();
		private Compositor _composite = new Compositor();
		protected Material _displayMaterial;
		private Mesh _mesh;
		private List<Color> _vertexColors;
		private Mesh _quadMesh;
		private Material _baseMaterialCopy;
		private Canvas _canvas;
		private bool _materialOutputPremultipliedAlpha = false;
		private bool _isFilterEnabled = false;
		protected float _userScale = 1f;
		protected bool _forceUpdate = true;
		protected RectAdjustOptions _rectAdjustOptions = new RectAdjustOptions();

		internal RectAdjustOptions RectAdjustOptions { get { return _rectAdjustOptions;} }
		internal float ResolutionScalingFactor { get; private set; }

		private const string DefaultBlendShaderPath = "Hidden/ChocDino/UIFX/Blend";
		private const string ResolveShaderPath = "Hidden/ChocDino/UIFX/Resolve";
		private RenderTexture _resolveTexture;
		private Material _resolveMaterial;
		private Texture2D _readableTexture;
		
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

		public bool IsFiltered()
		{
			return CanApplyFilter();
		}

		internal virtual bool CanApplyFilter()
		{
			// TODO: Check Graphic is enabled?
			if (!this.isActiveAndEnabled) return false;
			if (_canvas == null) return false;
			if (!_canvas.isActiveAndEnabled) return false;
			return DoParametersModifySource();
		}

		protected virtual bool DoParametersModifySource()
		{
			if (_strength <= 0f) return false;
			return true;
		}

		private const string TextMeshProShaderPrefix = "TextMeshPro";
		private Material _lastBaseMaterial;
		private bool _isLastMaterialTMPro;

		public virtual Material GetModifiedMaterial(Material baseMaterial)
		{
			Material resultMaterial = baseMaterial;
			_isResolveTextureDirty = true;

			if (GraphicComponent != null && CanApplyFilter())
			{
				if (baseMaterial && baseMaterial.shader)
				{
					// Ignore TextMeshPro shaders as they aren't supported directly and require a component that renders directly, not via GetModifiedMaterial()
					if ((baseMaterial != _lastBaseMaterial))
					{
						// NOTE: getting the shader name generates garbage, so we cache the material to minimise this
						_isLastMaterialTMPro = (baseMaterial.shader.name.StartsWith(TextMeshProShaderPrefix));
						_lastBaseMaterial = baseMaterial;
					}

					if (!_isLastMaterialTMPro)
					{
						if (_baseMaterialCopy == null || _baseMaterialCopy.shader != baseMaterial.shader)
						{
							ObjectHelper.Destroy(ref _baseMaterialCopy);
							_baseMaterial = baseMaterial;
							_baseMaterialCopy = new Material(baseMaterial);
							_materialOutputPremultipliedAlpha = MaterialHelper.MaterialOutputsPremultipliedAlpha(baseMaterial);
						}
						if (_baseMaterialCopy)
						{
							SetupMaterialForRendering(baseMaterial);

							// If alpha <= 0 there is no need to render the filters
							if (_graphicMaterialDirtied || _lastRenderAlpha > 0f )
							{
								if (RenderFilter(true))
								{
									resultMaterial = _displayMaterial;
								}
							}

							_graphicMaterialDirtied = false;
						}
					}
				}
			}
			return resultMaterial;
		}

		protected virtual string GetDisplayShaderPath()
		{
			return DefaultBlendShaderPath;
		}

		private bool _graphicMaterialDirtied = false;

		protected void OnGraphicMaterialDirtied()
		{
			_graphicMaterialDirtied = true;
		}
		
		protected override void OnEnable()
		{
			_canvas = GetCanvas();

			if (GraphicComponent)
			{
				GraphicComponent.RegisterDirtyMaterialCallback(OnGraphicMaterialDirtied);
			}

			string shaderPath = GetDisplayShaderPath();
			if (!string.IsNullOrEmpty(shaderPath))
			{
				var shader = Shader.Find(shaderPath);
				if (shader)
				{
					_displayMaterial = new Material(shader);
					#if UNITY_EDITOR
					_displayMaterial.name = "Filter-DisplayMaterial";
					#endif
					Debug.Assert(_displayMaterial != null);
				}
			}

			ForceUpdate();
			#if UIFX_FILTER_SUPPORT_CHILDREN
			FindChildren();
			#endif

			//Canvas.willRenderCanvases += WillRenderCanvases;

			base.OnEnable();
		}

		protected override void OnDisable()
		{
			//Canvas.willRenderCanvases -= WillRenderCanvases;

			if (GraphicComponent)
			{
				GraphicComponent.UnregisterDirtyMaterialCallback(OnGraphicMaterialDirtied);
			}

			ObjectHelper.Destroy(ref _blitMesh);
			if (_blitCommands != null)
			{
				_blitCommands.Release();
				_blitCommands = null;
			}

			RenderTextureHelper.ReleaseTemporary(ref _resolveTexture);
			ObjectHelper.Destroy(ref _resolveMaterial);
			ObjectHelper.Destroy(ref _readableTexture);
			ObjectHelper.Destroy(ref _displayMaterial);
			ObjectHelper.Destroy(ref _baseMaterialCopy);
			ObjectHelper.Destroy(ref _quadMesh);
			ObjectHelper.Destroy(ref _mesh);
			_composite.FreeResources();
			_baseMaterial = null;

			#if UIFX_FILTER_TMP
			// TODO: For TMP we have to restore the CanvasRenderer mesh+material
			CanvasRenderComponent.SetMesh(null);
			CanvasRenderComponent.materialCount = 1;
			CanvasRenderComponent.SetMaterial(null, 0);
			#endif

			#if UIFX_FILTER_SUPPORT_CHILDREN
			ClearChildren();
			#endif

			// NOTE: We have to force the Graphic to update when OnDisable() is called due to this component being destroyed due to the check this.isActiveAndEnabled
			// which would prevent it from running. this.isActiveAndEnabled is there to prevent forcing update when modifying values on a disabled component
			ForceUpdate(force:true);

			base.OnDisable();
		}

		/// <summary>
		/// NOTE: OnRectTransformDimensionsChange() is called whenever any of the elements in RectTransform (or parents) change
		/// This doesn't get called when pixel-perfect option is disabled and the translation/rotation/scale etc changes..
		/// </summary>
		protected override void OnRectTransformDimensionsChange()
		{
			// NOTE: Ideally we wouldn't want to force a complete update when TRANSLATION changes as this wouldn't require rerendering, however
			// this is the simplest solution for now.
			//ForceUpdate();
			//OnPropertyChange();
			base.OnRectTransformDimensionsChange();
		}

		/// <summary>
		/// NOTE: OnDidApplyAnimationProperties() is called when the Animator is used to keyframe properties
		/// </summary>
		protected override void OnDidApplyAnimationProperties()
		{
			base.OnDidApplyAnimationProperties();
			OnPropertyChange();
		}

		/// <summary>
		/// OnCanvasHierarchyChanged() is called when the Canvas is enabled/disabled
		/// </summary>
		protected override void OnCanvasHierarchyChanged()
		{
			_canvas = GetCanvas();
			ForceUpdate();
			base.OnCanvasHierarchyChanged();
		}

		/// <summary>
		/// OnTransformParentChanged() is called when a parent is changed, in which case we may need to get a new Canvas
		/// </summary>
		protected override void OnTransformParentChanged()
		{
			var oldCanvas = _canvas;
			_canvas = GetCanvas();
			if (oldCanvas != _canvas)
			{
				ForceUpdate();
			}
			base.OnTransformParentChanged();
		}

		/// <summary>
		/// Forces the filter to update.  Usually this happens automatically, but in some cases you may want to force an update.
		/// </summary>
		public void ForceUpdate(bool force = false)
		{
			_forceUpdate = true;

			if (force || this.isActiveAndEnabled)
			{
				var graphic = GraphicComponent;
				// There is no point setting the graphic dirty if it is not active/enabled (because SetMaterialDirty() will just return causing _forceUpdate to cleared prematurely)
				if (graphic != null && graphic.isActiveAndEnabled)
				{
					// We have to force the parent graphic to update so that the GetModifiedMaterial() and ModifyMesh() are called
					// TOOD: This wasteful, so ideally find a way to prevent this
					graphic.SetMaterialDirty();
					graphic.SetVerticesDirty();
					_forceUpdate = false;
				}
			}
			_lastModifyMeshFrame = -1;
		}

		#if UNITY_EDITOR
		protected override void Reset()
		{
			base.Reset();
			OnPropertyChange();

			// NOTE: Have to ForceUpdate() otherwise mesh doesn't update due to ModifyMesh being called multiple times a frame in this path and _lastModifyMeshFrame preventing update
			ForceUpdate();
		}
		
		protected override void OnValidate()
		{
			OnPropertyChange();
			base.OnValidate();

			// NOTE: Have to ForceUpdate() otherwise the Game View sometimes doesn't update the rendering, even though the Scene View does...
			ForceUpdate();
		}
		#endif

		private Matrix4x4 _previousLocalToWorldMatrix;
		private float _lastRenderAlpha = -1f;
		internal Vector2Int _lastRenderAdjustLeftDown;
		internal Vector2Int _lastRenderAdjustRightUp;

		protected void OnPropertyChange()
		{
			var graphic = GraphicComponent;
			if (!graphic) { return; }

			// If the property change caused the rectangle to change size, we need to SetVerticesDirty() to get the geometry regenerated
			Vector2Int leftDown = Vector2Int.zero;
			Vector2Int rightUp = Vector2Int.zero;
			GetFilterAdjustSize(ref leftDown, ref rightUp);
			if (leftDown != _lastRenderAdjustLeftDown || rightUp != _lastRenderAdjustRightUp)
			{
				graphic.SetVerticesDirty();

				// In cases where the parameters are adjusted which means the filter goes between being active and not, need to also update the material
				bool needsMaterialUpdate = _isFilterEnabled != CanApplyFilter();
				if (needsMaterialUpdate)
				{
					graphic.SetMaterialDirty();
				}
			}
			else
			{
				// Otherwise the property change only needs the material updated
				if (_isFilterEnabled || CanApplyFilter())
				{
					graphic.SetMaterialDirty();
				}

				// In cases where the parameters are adjusted which means the filter goes between being active and not,
				// or the vertex alpha value changes, need to also update the vertices
				bool needsVerticesUpdate = (_isFilterEnabled != CanApplyFilter()) || (_lastRenderAlpha != GetAlpha());
				if (needsVerticesUpdate)
				{
					graphic.SetVerticesDirty();
				}
			}
			_isResolveTextureDirty = true;
		}

		protected virtual void Update()
		{
			if (!_isLastMaterialTMPro)
			{
				bool forceUpdate = _forceUpdate;

				// Detect a change to the matrix (this also detects changes to the camera and viewport)
				if (MathUtils.HasMatrixChanged(_previousLocalToWorldMatrix, this.transform.localToWorldMatrix, false))
				{
					forceUpdate = true;
					_previousLocalToWorldMatrix = this.transform.localToWorldMatrix;
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
					ForceUpdate();
				}
			}
		}

		#if false
		void LateUpdate()
		{
			if (CanApplyFilter() != _isFilterEnabled)
			{
				#if UIFX_FILTER_SUPPORT_CHILDREN
				ForceChildrenUpdate();
				#endif
			}

			#if UIFX_FILTER_TMP
			if (GraphicComponent is TextMeshProUGUI)
			{
				var tmp = ((TextMeshProUGUI)GraphicComponent);
				//if (tmp.havePropertiesChanged)
				{
					Mesh sourceMesh = null;
					if (CanApplyFilter())
					{
						sourceMesh = ((TextMeshProUGUI)GraphicComponent).mesh;
					}

					ApplyMesh(sourceMesh);
				}
			}
			#endif
		}

		void WillRenderCanvases()
		{
			#if UIFX_FILTER_SUPPORT_CHILDREN
			// This will run when there is no GraphicComponent (useful for cases where 
			// only the children are having the effect applied to them)
			if (GraphicComponent == null && CanApplyFilter())
			{
				if (_quadMesh == null)
				{
					_quadMesh = new Mesh();
				}
				_quadMesh.Clear();
				VertexHelper v = new VertexHelper(_quadMesh);
				ModifyMesh(v);
				v.FillMesh(_quadMesh);
				v.Dispose();
				var cr = CanvasRenderComponent;
				cr.SetMesh(_quadMesh);
				if (_displayMaterial)
				{
					cr.materialCount = 1;
					cr.SetMaterial(_displayMaterial, 0);
				}
			}
			#endif
		}
		#endif

		private Mesh _blitMesh;
		private CommandBuffer _blitCommands;
		
		internal bool RenderToTexture(RenderTexture sourceTexture, RenderTexture destTexture)
		{
			Debug.Assert(sourceTexture != null);
			Debug.Assert(destTexture != null);
			
			RenderTexture prevRT = RenderTexture.active;

			GetResolutionScalingFactor();
			
			// Render filters and setup display material
			RenderTexture resultTexture = RenderFilters(sourceTexture);
			SetupDisplayMaterial(sourceTexture, resultTexture);
		
			// Final blit using display material (a clear is needed because we're blending into the texture).
			if (GetAlpha() >= 1f)
			{
				RenderTexture.active = destTexture;
				GL.Clear(false, true, Color.clear);
				Graphics.Blit(sourceTexture, destTexture, _displayMaterial);
			}
			else
			{
				// NOTE: No way to set alpha value in Graphics.Blit() so using CommandBuffer.DrawMesh()
				if (_blitMesh == null)
				{
					_blitMesh = new Mesh();
					_blitMesh.vertices = new Vector3[] { new Vector3(-1f, 1f, -1f), new Vector3(1f, 1f, -1f), new Vector3(1f, -1f, -1f), new Vector3(-1f, -1f, -1f) };
					_blitMesh.uv = new Vector2[] { new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 0f) };
					_blitMesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
				}
				Color32 alphaColor = new Color(1f, 1f, 1f, GetAlpha());
				_blitMesh.colors32 = new Color32[] { alphaColor, alphaColor, alphaColor, alphaColor } ;

				if (_blitCommands == null)
				{
					_blitCommands = new CommandBuffer();
				}
				_blitCommands.Clear();
				_blitCommands.SetRenderTarget(new RenderTargetIdentifier(destTexture));
				_blitCommands.ClearRenderTarget(false, true, Color.clear, 0f);
				_blitCommands.SetViewMatrix(Matrix4x4.identity);
				_blitCommands.SetProjectionMatrix(Matrix4x4.identity);
				_blitCommands.DrawMesh(_blitMesh, Matrix4x4.identity, _displayMaterial);
				Graphics.ExecuteCommandBuffer(_blitCommands);
			}
 
			RenderTexture.active = prevRT;
			return true;
		}

		private bool _isResolveTextureDirty = true;

		/// <summary>
		/// Resolves to a final sRGB straight-alpha texture suitable for display or saving to image file.
		/// </summary>
		public RenderTexture ResolveToTexture()
		{
			if (_isResolveTextureDirty)
			{
				RenderTexture prevRT = RenderTexture.active;
				if (RenderFilter(true))
				{
					if (_displayMaterial && _composite.GetTexture())
					{
						int outputWidth = _composite.GetTexture().width;
						int outputHeight = _composite.GetTexture().height;

						if (!_resolveMaterial)
						{
							_resolveMaterial = new Material(Shader.Find(ResolveShaderPath));
							_resolveMaterial.name = "Resolve";
						}
						if (_resolveTexture && (_resolveTexture.width != outputWidth || _resolveTexture.height != outputHeight))
						{
							RenderTextureHelper.ReleaseTemporary(ref _resolveTexture);
						}
						if (!_resolveTexture)
						{
							_resolveTexture = RenderTexture.GetTemporary(outputWidth, outputHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
							_resolveTexture.name = "Resolve";
						}

						RenderTexture displayTexture = RenderTexture.GetTemporary(outputWidth, outputHeight, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

						// Render as if displaying - this will output premultiplied alpha and possibly in linear space
						RenderTexture.active = displayTexture;
						GL.Clear(false, true, Color.clear);
						Graphics.Blit(_displayMaterial.mainTexture, displayTexture, _displayMaterial);

						// Resolve by removing premultiplied alpha and converting to sRGB
						Graphics.Blit(displayTexture, _resolveTexture, _resolveMaterial);

						RenderTextureHelper.ReleaseTemporary(ref displayTexture);

						_isResolveTextureDirty = false;
					}
				}
				else
				{
					RenderTextureHelper.ReleaseTemporary(ref _resolveTexture);
				}
				RenderTexture.active = prevRT;
			}
			return _resolveTexture;
		}

		/// <summary>
		/// Resolve the filter output to a sRGB texture and write it to a PNG file
		/// </summary>
		public bool SaveToPNG(string path)
		{
			if (CanApplyFilter())
			{
				RenderTexture texture = ResolveToTexture();
				if (texture)
				{
					// Create a new readable texture if necessary
					if (_readableTexture && (_readableTexture.width != texture.width || _readableTexture.height != texture.height))
					{
						ObjectHelper.Destroy(ref _readableTexture);
					}
					if (!_readableTexture)
					{
						_readableTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
					}
					
					return TextureUtils.WriteToPNG(texture, _readableTexture, path);
				}
			}
			return false;
		}
		
		public bool IsFilterEnabled()
		{
			return _isFilterEnabled;
		}

		protected virtual float GetAlpha()
		{
			return 1f;
		}

		private static List<Canvas> s_canvasList;

		private Canvas GetCanvas()
		{
			Canvas result = null;
			if (GraphicComponent)
			{
				result = GraphicComponent.canvas;
			}
			else
			{
				if (s_canvasList == null)
				{
					s_canvasList = new List<Canvas>(4);
				}
				var list = s_canvasList;
				this.gameObject.GetComponentsInParent(false, list);
				if (list.Count > 0)
				{
					// Find the first active and enabled canvas.
					for (int i = 0; i < list.Count; ++i)
					{
						if (list[i].isActiveAndEnabled)
						{
							result = list[i];
							break;
						}
					}
				}
			}
			return result;
		}

		private Camera GetRenderCamera()
		{
			Debug.Assert(this.isActiveAndEnabled);
			Camera camera = null;
			if (_canvas)
			{
				camera = _canvas.worldCamera;
				if (camera == null && _canvas.renderMode == RenderMode.WorldSpace)
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
				else if (camera == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					camera = null;
				}
			}
			
			return camera;
		}

		private static UIVertex s_vertex;
		private bool HasMeshChanged(VertexHelper verts)
		{
			if (_mesh == null)
			{
				return true;
			}

			if (verts.currentVertCount != _mesh.vertexCount || verts.currentIndexCount != _mesh.GetIndexCount(0))
			{
				return true;
			}

			int vertexCount = verts.currentVertCount;
			for (int i = 0; i < vertexCount; i++)
			{
				verts.PopulateUIVertex(ref s_vertex, i);

				if (s_vertex.position != _mesh.vertices[i])
				{
					return true;
				}

				// From Unity 2020.2 UIVertex.uv0 changed from Vector2 to Vector4
				#if UNITY_2020_2_OR_NEWER
				if (s_vertex.uv0.x != _mesh.uv[i].x)
				{
					return true;
				}
				if (s_vertex.uv0.y != _mesh.uv[i].y)
				{
					return true;
				}
				#else
				if (s_vertex.uv0 != _mesh.uv[i])
				{
					return true;
				}
				#endif
			}

			return false;
		}

		private void GrabMesh(VertexHelper verts)
		{
			if (_mesh == null)
			{
				_mesh = new Mesh();
			}
			verts.FillMesh(_mesh);
			if (QualitySettings.activeColorSpace == ColorSpace.Linear)
			{
				ColorUtils.ConvertMeshVertexColorsToLinear(_mesh, ref _vertexColors);
			}
		}

		private void GrabMesh(Mesh mesh)
		{
			if (_mesh == null)
			{
				_mesh = Object.Instantiate(mesh);
			}
			else
			{
				_mesh.Clear();
				_mesh.vertices = mesh.vertices;
				_mesh.colors = mesh.colors;
				_mesh.uv = mesh.uv;
				_mesh.uv2 = mesh.uv2;
				_mesh.normals = mesh.normals;
				_mesh.tangents = mesh.tangents;
				_mesh.triangles = mesh.triangles;
			}
			if (QualitySettings.activeColorSpace == ColorSpace.Linear)
			{
				ColorUtils.ConvertMeshVertexColorsToLinear(_mesh, ref _vertexColors);
			}
		}

		private void ApplyMesh(Mesh mesh)
		{
			CanvasRenderer cr = CanvasRenderComponent;

			_isFilterEnabled = false;
			_lastRenderAdjustLeftDown = _lastRenderAdjustRightUp = Vector2Int.zero;
			_isResolveTextureDirty = true;
			if (CanApplyFilter())
			{
				_isFilterEnabled = true;

				// NOTE: GraphicsComponent.canvas can become null when deleting the GameObject
				if (GraphicComponent != null && GraphicComponent.canvas == null)
				{
					return;
				}

				_lastRenderAlpha = GetAlpha();
				bool isFilterVisible = _lastRenderAlpha > 0f;
		
				if (isFilterVisible)
				{
					// Grab the current geometry
					GrabMesh(mesh);

					if (RenderFilter(true))
					{
						// Create quad to display the result
						if (_quadMesh == null)
						{
							_quadMesh = new Mesh();
						}
						_quadMesh.Clear();
						VertexHelper verts = new VertexHelper(_quadMesh);

						Camera renderCamera = GetRenderCamera();
						_screenRect.BuildScreenQuad(renderCamera, this.transform, _lastRenderAlpha, verts);
						verts.FillMesh(_quadMesh);

						verts.Dispose();

						cr.SetMesh(_quadMesh);
						if (_displayMaterial)
						{
							cr.materialCount = 1;
							cr.SetMaterial(_displayMaterial, 0);
						}
					}
					else
					{
						// There is nothing to render, probably because the area is zero
						cr.SetMesh(null);
					}
				}
				else
				{
					// There is nothing to render since the alpha is zero
					cr.SetMesh(null);
				}
			}
			else
			{
				// Filter is not applied
				cr.SetMesh(_mesh);
			}
		}

		private int _lastModifyMeshFrame = -1;

		/// <summary>
		/// Implements IMeshModifier.ModifyMesh() which is called by uGUI automatically when Graphic components generate geometry
		/// Note that this method is not called when using TextMeshPro as it bypasses the standard geometry generation and instead
		/// applies internally generated Mesh to the CanvasRenderer directly.
		/// Note that ModifyMesh() is called BEFORE GetModifiedMaterial()
		/// </summary>
		[UnityInternal.ExcludeFromDocs]
		public void ModifyMesh(VertexHelper verts)
		{
			_isFilterEnabled = false;
			_lastRenderAdjustLeftDown = _lastRenderAdjustRightUp = Vector2Int.zero;
			_isResolveTextureDirty = true;
			if (CanApplyFilter())
			{
				_isFilterEnabled = true;

				// NOTE: GraphicsComponent.canvas can become null when deleting the GameObject
				if (GraphicComponent != null && GraphicComponent.canvas == null)
				{
					return;
				}

				_lastRenderAlpha = GetAlpha();
				bool isFilterVisible = _lastRenderAlpha > 0f;
		
				if (isFilterVisible)
				{
					// In the editor when pixel-perfect is enabled and the transform is adjusted, it will call ModifyMesh twice in a frame
					// So we added this check to reduce unnecessary processing during the second call.
					// But in some cases (like when font regenerates texture layout, the mesh will change in the middle of the frame, which does require a rebuild)
					if (_lastModifyMeshFrame != Time.frameCount || HasMeshChanged(verts))
					{
						_lastModifyMeshFrame = Time.frameCount;

						// Grab the current geometry
						GrabMesh(verts);

						// Create quad to display the result
						if (GenerateScreenRect())
						{
							Camera renderCamera = GetRenderCamera();
							_screenRect.BuildScreenQuad(renderCamera, this.transform, _lastRenderAlpha, verts);

							if (!_graphicMaterialDirtied)
							{
								// Only render the filter if the material isn't also going to render the filter.
								RenderFilter(false);
							}
						}
						else
						{
							// There is nothing to render, probably because the area is zero
							verts.Clear();
						}
					}
					else
					{
						Camera renderCamera = GetRenderCamera();
						_screenRect.BuildScreenQuad(renderCamera, this.transform, _lastRenderAlpha, verts);
					}
				}
				else
				{
					// There is nothing to render since the alpha is zero
					verts.Clear();
				}
			}
			else
			{
				// Filter is not applied
			}
		}


		private static bool _issuedModifyMeshWarning = false;

		[UnityInternal.ExcludeFromDocs]
		[System.Obsolete("use IMeshModifier.ModifyMesh (VertexHelper verts) instead, or set useLegacyMeshGeneration to false", false)]
		public void ModifyMesh(Mesh mesh)
		{
			if (!_issuedModifyMeshWarning)
			{
				Debug.LogWarning("use IMeshModifier.ModifyMesh (VertexHelper verts) instead, or set useLegacyMeshGeneration to false");
				_issuedModifyMeshWarning = true;
			}

			VertexHelper vh = new VertexHelper(mesh);
			ModifyMesh(vh);
			vh.FillMesh(mesh);
			vh.Dispose();

			//	throw new System.NotImplementedException("use IMeshModifier.ModifyMesh (VertexHelper verts) instead, or set useLegacyMeshGeneration to false");
		}

		protected void GetResolutionScalingFactor()
		{
		#if UIFX_OLD178_RESOLUTION_SCALING
			Camera renderCamera = GetRenderCamera();
			ResolutionScalingFactor = Filters.GetScaling(renderCamera) * _userScale;
		#else
			float canvasScale = 1f;
			Debug.Assert(_canvas != null);
			if (_canvas != null)
			{
				canvasScale = _canvas.scaleFactor;

				#if UNITY_EDITOR
				// Handle cases where we're in "in-context" prefab mode, in this case Canvas.ScaleFacotr is not set, but
				// scale needs to be derived from the transform
				if (canvasScale == 1f && _canvas.worldCamera == null && _canvas.renderMode == RenderMode.WorldSpace)
				{
					if (EditorHelper.IsInContextPrefabMode())
					{
						canvasScale = _canvas.transform.localScale.x;
					}
				}
				#endif
			}
			
			ResolutionScalingFactor = /*Filters.GetScaling(renderCamera) */ canvasScale * _userScale;
		#endif
		}

		protected bool GenerateScreenRect()
		{
			bool result = false;

			GetResolutionScalingFactor();

			// Calculate screen area
			{
				// TODO: only recalculate when mesh/camera changes
				Camera renderCamera = GetRenderCamera();
				_screenRect.Start(renderCamera);
				if (GraphicComponent)
				{
					_screenRect.AddMeshBounds(this.transform, _mesh);
				}
				#if UIFX_BLUR_FILTER_SUPPORT_CHILDREN
				if (_children.Count > 0)
				{
					for (int i = 0; i < _children.Count; i++)
					{
						IFilterChild child = _children[i];
						if (child.IsRenderable())
						{
							_screenRect.AddMeshBounds(_children[i].GetTransform(), _children[i].GetMesh());
						}
					}
				}
				#endif
				_screenRect.End();

				// Expand to accomodate filter
				{
					_lastRenderAdjustLeftDown = Vector2Int.zero;
					_lastRenderAdjustRightUp = Vector2Int.zero;
					GetFilterAdjustSize(ref _lastRenderAdjustLeftDown, ref _lastRenderAdjustRightUp);
					_screenRect.Adjust(_lastRenderAdjustLeftDown, _lastRenderAdjustRightUp);
					_screenRect.OptimiseRects(_rectAdjustOptions);

					{
						Rect r = _screenRect.GetRect();
						RectInt textureRectDst = _screenRect.GetTextureRect();

						Debug.Assert(textureRectDst.xMin <= r.xMin);
						Debug.Assert(textureRectDst.yMin <= r.yMin);
						Debug.Assert(textureRectDst.xMax >= r.xMax);
						Debug.Assert(textureRectDst.yMax >= r.yMax);

						float xMin = Mathf.InverseLerp(textureRectDst.xMin, textureRectDst.xMax, r.xMin);
						float xMax = Mathf.InverseLerp(textureRectDst.xMin, textureRectDst.xMax, r.xMax);
						float yMin = Mathf.InverseLerp(textureRectDst.yMin, textureRectDst.yMax, r.yMin);
						float yMax = Mathf.InverseLerp(textureRectDst.yMin, textureRectDst.yMax, r.yMax);

						_rectRatio = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
						//_rectRatio = Rect.MinMaxRect(0f, 0f, 1f, 1f);
					}
				}
			}

			if (_screenRect.GetRect().width > 0 && _screenRect.GetRect().height > 0)
			{
				if (_screenRect.GetRect().width <= Filters.GetMaxiumumTextureSize() && _screenRect.GetRect().height <= Filters.GetMaxiumumTextureSize())
				{
					result = true;
				}
				else
				{
					// NOTE: If texture size limit has been reached, then the previously generates textures are used.
					// This is incorrect but better than a crash or undefined behaviour.
					result = true;
				}
			}
			else
			{
				// There is nothing to render since the rect area is zero
			}
			return result;
		}

		internal void AdjustRect(ScreenRectFromMeshes rect)
		{
			_screenRect.SetRect(rect.GetRect());
			
			GetResolutionScalingFactor();

			Vector2Int leftDown = Vector2Int.zero;
			Vector2Int rightUp = Vector2Int.zero;
			GetFilterAdjustSize(ref leftDown, ref rightUp);
			rect.Adjust(leftDown, rightUp);

			// Store the rect
			_screenRect.SetRect(rect.GetRect());
		}

		internal void SetFinalRect(ScreenRectFromMeshes finalRect)
		{
			Rect r = _screenRect.GetRect();
			RectInt textureRectSrc = new RectInt(Mathf.FloorToInt(r.xMin), Mathf.FloorToInt(r.yMin), Mathf.CeilToInt(r.xMax) - Mathf.FloorToInt(r.xMin), Mathf.CeilToInt(r.yMax) - Mathf.FloorToInt(r.yMin));
			RectInt textureRectDst = finalRect.GetTextureRect();

			Debug.Assert(textureRectDst.xMin <= textureRectSrc.xMin);
			Debug.Assert(textureRectDst.yMin <= textureRectSrc.yMin);
			Debug.Assert(textureRectDst.xMax >= textureRectSrc.xMax);
			Debug.Assert(textureRectDst.yMax >= textureRectSrc.yMax);

			// Get the ratio of the smaller internal rectangle for this filter, compared to the (potentially) larger texture size based on the accumulated size of all filters
			float xMin = Mathf.InverseLerp(textureRectDst.xMin, textureRectDst.xMax, textureRectSrc.xMin);
			float xMax = Mathf.InverseLerp(textureRectDst.xMin, textureRectDst.xMax, textureRectSrc.xMax);
			float yMin = Mathf.InverseLerp(textureRectDst.yMin, textureRectDst.yMax, textureRectSrc.yMin);
			float yMax = Mathf.InverseLerp(textureRectDst.yMin, textureRectDst.yMax, textureRectSrc.yMax);

			_rectRatio = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
		}

		protected Rect _rectRatio = new Rect(0f, 0f, 1f, 1f);

		private void SetupMaterialForRendering(Material baseMaterial)
		{
			Debug.Assert(_baseMaterialCopy != null);

			// Copy material properties (and enabled keywords) for rendering
			_baseMaterialCopy.CopyPropertiesFromMaterial(baseMaterial);

			var graphic = GraphicComponent;

			// NOTE: When we get a copy of the material via GetModifiedMaterial() the mainTexture usually 
			// hasn't been set yet, unless it's a material that the user has set.  So we must get this from 
			// the GraphicComponent and assign it ourselves.
			//bool isCustomUserMaterial = (graphic.material != graphic.defaultMaterial);
			//if (!isCustomUserMaterial)
			{
				// Only override the material mainTexture if one is specified in the Graphic
				var graphicTexture = graphic.mainTexture;
				if (graphicTexture)
				{
					_baseMaterialCopy.mainTexture = graphicTexture;
				}
				if (_baseMaterialCopy.mainTexture)
				{
					if (_isGraphicText)
					{
						// For Unity's Text as decoding vector needs to be set
						_baseMaterialCopy.SetVector(UnityShaderProp.TextureAddSample, Alpha8TextureAdd);
					}
					else
					{
						_baseMaterialCopy.SetVector(UnityShaderProp.TextureAddSample, Vector4.zero);
					}
				}
			}

			// Copy the stencil properties to the display material
			if (_displayMaterial && MaskableGraphicComponent)
			{
				if (MaskableGraphicComponent.maskable)
				{
					UnityShaderProp.CopyStencilProperties(_baseMaterialCopy, _displayMaterial);
				}
				else
				{
					UnityShaderProp.ResetStencilProperties(_displayMaterial);
				}
			}
		}

		protected virtual bool RenderFilter(bool generateScreenRect)
		{
			bool result = false;

			GetResolutionScalingFactor();
			Camera renderCamera = GetRenderCamera();

			{
				// Only call GenerateScreenRect() when needed
				if (generateScreenRect)
				{
					GenerateScreenRect();
				}

				// TODO: only recalculate when screenRect/mesh/camera changes
				if (_composite.Start(renderCamera, _screenRect.GetTextureRect()))
				{
					if (GraphicComponent)
					{
						if (!_graphicMaterialDirtied)
						{
							if (_baseMaterial && _baseMaterialCopy)
							{
								SetupMaterialForRendering(_baseMaterial);
							}
						}
						_composite.AddMesh(this.transform, _mesh, _baseMaterialCopy, _materialOutputPremultipliedAlpha);
					}
					#if UIFX_BLUR_FILTER_SUPPORT_CHILDREN
					if (_children.Count > 0)
					{
						for (int i = 0; i < _children.Count; i++)
						{
							IFilterChild child = _children[i];
							if (child.IsRenderable())
							{
								_composite.AddMesh(child.GetTransform(), child.GetMesh(), child.GetMaterial());
							}
						}
					}
					#endif

					_composite.End();

					// Optionally render the composite through filters
					{
						RenderTexture sourceTexture = _composite.GetTexture();
						RenderTexture resultTexture = RenderFilters(sourceTexture);
						SetupDisplayMaterial(sourceTexture, resultTexture);
					}
				}
				result = true;
			}
			return result;
		}

		protected virtual RenderTexture RenderFilters(RenderTexture source)
		{
			// Optionally apply some filters
			return source;
		}

		protected virtual void GetFilterAdjustSize(ref Vector2Int leftDown, ref Vector2Int rightUp)
		{
		}

		protected virtual void SetupDisplayMaterial(Texture source, Texture result)
		{
			if (_displayMaterial)
			{
				_displayMaterial.mainTexture = result;
				_displayMaterial.SetTexture(ShaderProp.SourceTex, source);
				_displayMaterial.SetTexture(ShaderProp.ResultTex, result);
			}
		}

		protected bool ChangeProperty<T>(ref T backing, T value) where T : struct
		{
			bool result = false;
			if (ObjectHelper.ChangeProperty(ref backing, value))
			{
				result = true;
				backing = value;
				OnPropertyChange();
			}
			return result;
		}

		protected bool ChangePropertyRef<T>(ref T backing, T value) where T : class
		{
			bool result = false;
			if (backing != value)
			{
				result = true;
				backing = value;
				OnPropertyChange();
			}
			return result;
		}

		internal Rect GetLocalRect()
		{
			return _screenRect.GetLocalRect();
		}

		internal virtual string GetDebugString()
		{
			string result = string.Empty;
			if (_screenRect != null)
			{
				result += "Rect: " + _screenRect.GetRect().ToString() + "\n";
			}
			{
				Vector2Int leftDown = Vector2Int.zero;
				Vector2Int rightUp = Vector2Int.zero;
				GetFilterAdjustSize(ref leftDown, ref rightUp);
				result += "Expand: [" + leftDown + " - " + rightUp + "]\n";
			}

			#if UIFX_FILTER_SUPPORT_CHILDREN
			result += "Children: " + _children.Count;
			#endif
			return result;
		}

		internal virtual Texture[] GetDebugTextures()
		{
			List<Texture> result = new List<Texture>();
			if (_composite != null)
			{
				result.Add(_composite.GetTexture());
			}
			if (_displayMaterial != null)
			{
				result.Add(_displayMaterial.mainTexture);
			}
			return result.ToArray();
		}
	}
}