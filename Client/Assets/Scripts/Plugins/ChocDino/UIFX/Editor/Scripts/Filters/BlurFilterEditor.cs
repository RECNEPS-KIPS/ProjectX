//--------------------------------------------------------------------------//
// Copyright 2023-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using UnityEngine;
using UnityEditor;

namespace ChocDino.UIFX.Editor
{
	[CustomEditor(typeof(BlurFilter), true)]
	[CanEditMultipleObjects]
	internal class BlurFilterEditor : FilterBaseEditor
	{
		private static readonly GUIContent Content_Axes = new GUIContent("Axes");
		private static readonly GUIContent Content_Blur = new GUIContent("Blur");
		private static readonly GUIContent Content_Global = new GUIContent("Global");

		private SerializedProperty _propBlurAlgorithm;
		private SerializedProperty _propBlurAxes2D;
		private SerializedProperty _propDownsample;
		protected SerializedProperty _propBlur;
		private SerializedProperty _propApplyAlphaCurve;
		private SerializedProperty _propAlphaCurve;
		protected SerializedProperty _propStrength;
		//protected SerializedProperty _propIncludeChildren;

		private static readonly AboutInfo s_aboutInfo = 
				new AboutInfo(s_aboutHelp, "UIFX - Blur Filter\n© Chocolate Dinosaur Ltd", "uifx-logo-blur-filter")
				{
					sections = new AboutSection[]
					{
						new AboutSection("Asset Guides")
						{
							buttons = new AboutButton[]
							{
								new AboutButton("User Guide", "https://www.chocdino.com/products/uifx/blur-filter/about/"),
								new AboutButton("Scripting Guide", "https://www.chocdino.com/products/uifx/blur-filter/scripting/"),
								new AboutButton("Components Reference", "https://www.chocdino.com/products/uifx/blur-filter/components/blur-filter/"),
								new AboutButton("API Reference", "https://www.chocdino.com/products/uifx/blur-filter/API/ChocDino.UIFX/"),
							}
						},
						new AboutSection("Unity Asset Store Review\r\n<color=#ffd700>★★★★☆</color>")
						{
							buttons = new AboutButton[]
							{
								new AboutButton("Review <b>UIFX - Blur Filter</b>", "https://assetstore.unity.com/packages/slug/268262?aid=1100lSvNe#reviews"),
								new AboutButton("Review <b>UIFX Bundle</b>", AssetStoreBundleReviewUrl),
							}
						},
						new AboutSection("UIFX Support")
						{
							buttons = new AboutButton[]
							{
								new AboutButton("Discord Community", DiscordUrl),
								new AboutButton("Post to Unity Discussions", "https://discussions.unity.com/t/released-uifx-blur-filter/936189"),
								new AboutButton("Post Issues to GitHub", GithubUrl),
								new AboutButton("Email Us", SupportEmailUrl),
							}
						}
					}
				};

		private static readonly AboutToolbar s_aboutToolbar = new AboutToolbar(new AboutInfo[] { s_upgradeToBundle, s_aboutInfo } );

		protected virtual void OnEnable()
		{
			_propBlurAlgorithm = VerifyFindProperty("_algorithm");
			_propBlurAxes2D = VerifyFindProperty("_blurAxes2D");
			_propDownsample = VerifyFindProperty("_downSample");
			_propBlur = VerifyFindProperty("_blur");
			_propApplyAlphaCurve = VerifyFindProperty("_applyAlphaCurve");
			_propAlphaCurve = VerifyFindProperty("_alphaCurve");
			_propStrength = VerifyFindProperty("_strength");
			//_propIncludeChildren = VerifyFindProperty("_includeChildren");
		}

		public override void OnInspectorGUI()
		{
			s_aboutToolbar.OnGUI();

			serializedObject.Update();

			var filter = this.target as FilterBase;

			if (OnInspectorGUI_Check(filter))
			{
				return;
			}

			GUILayout.Label(Content_Blur, EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			EnumAsToolbar(_propBlurAlgorithm);
			EnumAsToolbar(_propDownsample);
			EnumAsToolbar(_propBlurAxes2D, Content_Axes);
			EditorGUILayout.PropertyField(_propBlur);
			//GUI.enabled = false;
			//EditorGUILayout.PropertyField(_propIncludeChildren);
			//GUI.enabled = true;
			EditorGUI.indentLevel--;

			GUILayout.Label(Content_Apply, EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(_propApplyAlphaCurve);
			if (_propApplyAlphaCurve.boolValue)
			{
				EditorGUILayout.PropertyField(_propAlphaCurve);
			}
			DrawStrengthProperty(_propStrength);
			EditorGUI.indentLevel--;

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			GUILayout.Label(Content_Global, EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			if (DrawStrengthProperty(ref BlurFilter.GlobalStrength))
			{
				#if UNITY_2022_2_OR_NEWER
				BlurFilter[] blurs = Object.FindObjectsByType<BlurFilter>(FindObjectsSortMode.None);
				#else
				BlurFilter[] blurs = Object.FindObjectsOfType<BlurFilter>();
				#endif
				foreach (var blur in blurs)
				{
					EditorUtility.SetDirty(blur);
				}
			}
			EditorGUI.indentLevel--;

			if (OnInspectorGUI_Baking(filter))
			{
				return;
			}

			FilterBaseEditor.OnInspectorGUI_Debug(filter);

			serializedObject.ApplyModifiedProperties();
		}
	}
}