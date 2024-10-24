//--------------------------------------------------------------------------//
// Copyright 2023-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using UnityEngine;
using UnityEditor;

namespace ChocDino.UIFX.Editor
{
	public class BaseEditor : UnityEditor.Editor
	{
		protected const string s_aboutHelp = "About & Help";

		protected static readonly string DiscordUrl = "https://discord.gg/wKRzKAHVUE";
		protected static readonly string ForumBundleUrl = "https://discussions.unity.com/t/released-uifx-bundle-advanced-effects-for-unity-ui/940575";
		protected static readonly string GithubUrl = "https://github.com/Chocolate-Dinosaur/UIFX/issues";
		protected static readonly string SupportEmailUrl = "mailto:support@chocdino.com";
		protected static readonly string UIFXBundleWebsiteUrl = "https://www.chocdino.com/products/uifx/bundle/about/";
		protected static readonly string AssetStoreBundleUrl = "https://assetstore.unity.com/packages/slug/266945?aid=1100lSvNe";
		protected static readonly string AssetStoreBundleReviewUrl = "https://assetstore.unity.com/packages/slug/266945?aid=1100lSvNe#reviews";

		internal const string PrefKey_BakedImageSubfolder = "UIFX.BakedImageSubfolder";
		internal const string DefaultBakedImageAssetsSubfolder = "Baked-Images";
		
		internal static readonly AboutInfo s_upgradeToBundle = 
				new AboutInfo("Upgrade ★", "This asset is part of the <b>UIFX Bundle</b> asset.\r\n\r\nAs an existing customer you are entitled to a discounted upgrade!", "uifx-logo-bundle", BaseEditor.ShowUpgradeBundleButton)
				{
					sections = new AboutSection[]
					{
						new AboutSection("Upgrade")
						{
							buttons = new AboutButton[]
							{
								new AboutButton("<color=#ffd700>★ </color>Upgrade to UIFX Bundle<color=#ffd700> ★</color>", AssetStoreBundleUrl),
							}
						},
						new AboutSection("Read More")
						{
							buttons = new AboutButton[]
							{
								new AboutButton("About UIFX Bundle", UIFXBundleWebsiteUrl),
							}
						},
					}
				};

		internal static bool ShowUpgradeBundleButton(bool dummy)
		{
			return !DetectUIFXBundle();
		}

		internal static bool DetectUIFXBundle()
		{
			const string frameFilterGUID = "867f71eed15164b469927b3eeb563a24";
			return !string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(frameFilterGUID));
		}

		// <summary>
		// Creates a button that toggles between two texts but maintains the same size by using the size of the largest.
		// This is useful so that the button doesn't change size resulting in the mouse cursor no longer being over the button.
		// </summary>
		protected bool ToggleButton(bool value, GUIContent labelTrue, GUIContent labelFalse)
		{
			float maxWidth = Mathf.Max(GUI.skin.button.CalcSize(labelTrue).x, GUI.skin.button.CalcSize(labelFalse).x);
			GUIContent content = value ? labelTrue : labelFalse;
			return GUILayout.Button(content, GUILayout.Width(maxWidth));
		}

		protected void EnumAsToolbar(SerializedProperty prop, GUIContent displayName = null)
		{
			if (displayName == null)
			{
				displayName = new GUIContent(prop.displayName);
			}
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(displayName);
			Rect rect = EditorGUILayout.GetControlRect();
			EditorGUILayout.EndHorizontal();

			displayName = EditorGUI.BeginProperty(rect, displayName, prop);

			EditorGUI.BeginChangeCheck();

			int newIndex = GUI.Toolbar(rect, prop.enumValueIndex, prop.enumDisplayNames);

			// Only assign the value back if it was actually changed by the user.
			// Otherwise a single value will be assigned to all objects when multi-object editing,
			// even when the user didn't touch the control.
			if (EditorGUI.EndChangeCheck())
			{
				prop.enumValueIndex = newIndex;
			}
			EditorGUI.EndProperty();
		}

		protected void TextureScaleOffset(SerializedProperty propTexture, SerializedProperty propScale, SerializedProperty propOffset, GUIContent displayName)
		{
			EditorGUILayout.PropertyField(propTexture);
			//EditorGUILayout.PropertyField(_propTextureOffset, Content_Offset);
			//EditorGUILayout.PropertyField(_propTextureScale, Content_Scale);

			Rect rect = EditorGUILayout.GetControlRect(true, 2 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), EditorStyles.layerMaskField);
			EditorGUI.BeginChangeCheck();
			Vector4 scaleOffset = new Vector4(propScale.vector2Value.x, propScale.vector2Value.y, propOffset.vector2Value.x, propOffset.vector2Value.y);
			EditorGUI.indentLevel++;
			scaleOffset = MaterialEditor.TextureScaleOffsetProperty(rect, scaleOffset, false);
			EditorGUI.indentLevel--;
			if (EditorGUI.EndChangeCheck())
			{
				propScale.vector2Value = new Vector2(scaleOffset.x, scaleOffset.y);
				propOffset.vector2Value = new Vector2(scaleOffset.z, scaleOffset.w);
			}
		}
		
		protected SerializedProperty VerifyFindProperty(string propertyPath)
		{
			SerializedProperty result = serializedObject.FindProperty(propertyPath);
			Debug.Assert(result != null);
			if (result == null)
			{
				Debug.LogError("Failed to find property '" + propertyPath + "' in object '" + serializedObject.ToString()+ "'");
			}
			return result;
		}

		internal static SerializedProperty VerifyFindPropertyRelative(SerializedProperty property, string relativePropertyPath)
		{
			SerializedProperty result = null;
			Debug.Assert(property != null);
			if (property == null)
			{
				Debug.LogError("Property is null while finding relative property '"+ relativePropertyPath + "'");
			}
			else
			{
				result = property.FindPropertyRelative(relativePropertyPath);
				Debug.Assert(result != null);
				if (result == null)
				{
					Debug.LogError("Failed to find relative property '" + relativePropertyPath + "' in property '" + property.name + "'");
				}
			}
			return result;
		}
	}
}
