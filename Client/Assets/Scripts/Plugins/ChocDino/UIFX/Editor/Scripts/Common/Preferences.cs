//--------------------------------------------------------------------------//
// Copyright 2023-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ChocDino.UIFX.Editor
{
	internal static class Preferences
	{
		internal static readonly string SettingsPath = "Project/Chocolate Dinosaur/UIFX";

		[SettingsProvider]
		static SettingsProvider CreateSettingsProvider()
		{
			return new UIFXSettingsProvider(SettingsPath, SettingsScope.Project);
		}

		private class UIFXSettingsProvider : SettingsProvider
		{
			public UIFXSettingsProvider(string path, SettingsScope scope) : base(path, scope)
			{
				this.keywords = new HashSet<string>(new[] { "UIFX", "Chocolate", "Dinosaur", "ChocDino", "UI", "GUI", "UGUI" });
			}

			private static readonly GUIContent Content_BakedImagesFolder = new GUIContent("Baked Images Folder", "Folder path for baked images. Path is relative to Assets folder");

			private string _defines;
			private string _oldDefines;
			private bool _unappliedChanges;

			public override void OnActivate(string searchContext, VisualElement rootElement)
			{
				CacheDefines();
			}

			public override void OnDeactivate()
			{
			}

			private void CacheDefines()
			{
				var target = EditorUserBuildSettings.activeBuildTarget;
				var group = BuildPipeline.GetBuildTargetGroup(target);
				#if UNITY_2023_1_OR_NEWER
				var buildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(group);
				_oldDefines = _defines = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
				#else
				_oldDefines = _defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
				#endif
			}

			private void ApplyDefines()
			{
				var target = EditorUserBuildSettings.activeBuildTarget;
				var group = BuildPipeline.GetBuildTargetGroup(target);
				#if UNITY_2023_1_OR_NEWER
				var buildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(group);
				PlayerSettings.SetScriptingDefineSymbols(buildTarget, _defines);
				#else
				PlayerSettings.SetScriptingDefineSymbolsForGroup(group, _defines);
				#endif
				CacheDefines();
			}

			private bool HasDefine(string define)
			{
				return (_defines.IndexOf(define) >= 0);
			}

			private void AddDefine(string define)
			{
				_defines = (_defines + ";" + define + ";").Replace(";;", ";");
			}

			private void RemoveDefine(string define)
			{
				_defines = _defines.Replace(define, "").Replace(";;", ";");
			}

			private bool HasDefineChanged(string define)
			{
				bool a = HasDefine(define);
				bool b = (_oldDefines.IndexOf(define) >= 0);
				return (a != b);
			}

			public override void OnTitleBarGUI()
			{
				if (_unappliedChanges)
				{
					GUI.color = Color.green;
					if (GUILayout.Button("Apply Changes"))
					{
						ApplyDefines();
					}
					GUI.color = Color.white;
				}
			}

			public override void OnFooterBarGUI()
			{
			}

			public override void OnGUI(string searchContext)
			{
				const string UIFX_TMPRO = "UIFX_TMPRO";
				const string UIFX_BETA = "UIFX_BETA";
				const string UIFX_UNRELEASED = "UIFX_UNRELEASED";
				const string UIFX_FILTER_HIDE_INSPECTOR_PREVIEW = "UIFX_FILTER_HIDE_INSPECTOR_PREVIEW";
				const string UIFX_LOG = "UIFX_LOG";
				const string UIFX_FILTER_DEBUG = "UIFX_FILTER_DEBUG";
				const string UIFX_FILTERS_FORCE_UPDATE_PLAYMODE = "UIFX_FILTERS_FORCE_UPDATE_PLAYMODE";
				const string UIFX_FILTERS_FORCE_UPDATE_EDITMODE = "UIFX_FILTERS_FORCE_UPDATE_EDITMODE";

				EditorGUILayout.Space();

				bool changes= false;
				changes |= ShowDefineToggle("Text Mesh Pro Support (Requires com.unity.textmeshpro package)", UIFX_TMPRO);
				changes |= ShowDefineToggle("Hide Inspector Filter Preview", UIFX_FILTER_HIDE_INSPECTOR_PREVIEW);
				changes |= ShowDefineToggle("Beta Features", UIFX_BETA);
				ShowEditorPref(Content_BakedImagesFolder, BaseEditor.PrefKey_BakedImageSubfolder, BaseEditor.DefaultBakedImageAssetsSubfolder);
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

				if (SystemInfo.deviceName == "DESKTOP-ULTRA")
				{
					GUILayout.Label("Developer Mode:", EditorStyles.largeLabel);
					changes |= ShowDefineToggle("Debug Logging", UIFX_LOG);
					changes |= ShowDefineToggle("Filter Debugging", UIFX_FILTER_DEBUG);
					changes |= ShowDefineToggle("Filter Force Update in Play Mode", UIFX_FILTERS_FORCE_UPDATE_PLAYMODE);
					changes |= ShowDefineToggle("Filter Force Update in Edit Mode", UIFX_FILTERS_FORCE_UPDATE_EDITMODE);
					changes |= ShowDefineToggle("Unreleased Features", UIFX_UNRELEASED);

					EditorGUILayout.Space();
					EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
				}

				_unappliedChanges = changes;

				Links();
			}

			private void Links()
			{
				const string DiscordCommunityUrl = "https://discord.gg/wKRzKAHVUE";
				const string DocumentationUrl = "https://www.chocdino.com/products/category/components/";
				const string AssetStoreUrl = "https://assetstore.unity.com/publishers/80225?aid=1100lSvNe";

				GUILayout.Label("Chocolate Dinosaur Links:", EditorStyles.largeLabel);

				if (GUILayout.Button("UIFX Documentation", EditorStyles.miniButton))
				{
					Application.OpenURL(DocumentationUrl);
				}
				if (GUILayout.Button("Discord Community", EditorStyles.miniButton))
				{
					Application.OpenURL(DiscordCommunityUrl);
				}
				if (GUILayout.Button("Our Assets", EditorStyles.miniButton))
				{
					Application.OpenURL(AssetStoreUrl);
				}
			}

			private bool ShowDefineToggle(string label, string define)
			{
				bool enabled = HasDefine(define);
				bool changed = HasDefineChanged(define);
				if (changed)
				{
					label += " *";
				}

				bool newState = GUILayout.Toggle(enabled, label);
				if (newState != enabled)
				{
					if (newState)
					{
						AddDefine(define);
					}
					else
					{
						RemoveDefine(define);
					}
				}

				return HasDefineChanged(define);
			}

			private void ShowEditorPref(GUIContent label, string prefKey, string defaultValue)
			{
				var oldValue = EditorPrefs.GetString(prefKey, defaultValue);
				var newValue = EditorGUILayout.TextField(label, oldValue, GUILayout.MaxWidth(512f));
				if (newValue != oldValue)
				{
					EditorPrefs.SetString(prefKey, newValue);
				}
			}
		}
	}
}