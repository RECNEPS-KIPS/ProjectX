using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Michsky.UI.MTP
{
    public class StyleCreator : EditorWindow
    {
        static StyleCreator window;
        protected GUIStyle panelStyle;
        protected GUIStyle lipStyle;
        protected GUIStyle lipAltStyle;

        Vector2 scrollPosition = Vector2.zero;

        public Texture2D bannerTexture;
        public StyleCreatorList styleCreatorList;
        StyleVideoPreview tempWindow;

        [MenuItem("Tools/UI特效/创建样式", false, 999)]
        public static void ShowWindow()
        {
            window = GetWindow<StyleCreator>("MTP - Style Creator");
            window.minSize = new Vector2(560, 534);
            window.maxSize = new Vector2(560, 534);
        }

        void OnEnable()
        {
            try
            {
                if (bannerTexture == null)
                    bannerTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/" + EditorPrefs.GetString("MTP.StyleCreator.RootFolder") + "Banner.png", typeof(Texture2D));

                if (styleCreatorList == null)
                    styleCreatorList = (StyleCreatorList)AssetDatabase.LoadAssetAtPath("Assets/" + EditorPrefs.GetString("MTP.StyleCreator.RootFolder") + "StyleData.asset", typeof(StyleCreatorList));
            }

            catch
            {
                if (EditorUtility.DisplayDialog("Motion Titles Pack", "Cannot open the creator due to missing/incorrect root folder. " +
                 "You can change the root folder by clicking 'Fix' button and enabling 'Change Root Folder'.", "Fix", "Cancel"))
                    ShowRootManager();
            }
        }

        void OnDisable()
        {
            if (tempWindow != null)
                tempWindow.Close();
        }

        void OnGUI()
        {
            // Custom skin
            GUISkin customSkin;
            Color defaultColor = GUI.color;
            Color lipBGColor;
            Color lipAltBGColor;

            if (EditorGUIUtility.isProSkin == true)
            {
                customSkin = (GUISkin)Resources.Load("Editor\\Skin\\MTP Skin Dark");
                // lipBGColor = new Color32(30, 35, 40, 255);
                // lipAltBGColor = new Color32(25, 30, 35, 255);
                lipBGColor = new Color32(47, 47, 47, 255);
                lipAltBGColor = new Color32(42, 42, 42, 255);
            }

            else
            {
                customSkin = (GUISkin)Resources.Load("Editor\\Skin\\MTP Skin Light");
                lipBGColor = new Color32(47, 47, 47, 255);
                lipAltBGColor = new Color32(42, 42, 42, 255);
            }

            // Custom panel
            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.textColor = GUI.skin.label.normal.textColor;
            panelStyle.margin = new RectOffset(15, 15, 0, 15);
            panelStyle.padding = new RectOffset(0, 0, 0, 0);

            // List item panel
            lipStyle = customSkin.FindStyle("Style Creator LIP");

            // Banner
            GUILayout.BeginHorizontal();
            GUILayout.Label(bannerTexture, GUILayout.Width(1000), GUILayout.Height(90));
            GUILayout.EndHorizontal();

            // Check for style list
            if (styleCreatorList == null)
                return;

            // Scroll panel
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(432));
            GUILayout.BeginVertical(panelStyle);

            bool drawAltBG = false;
            bool reverseOrder = false;
            int lineIndex = 0;

            for (int i = 0; i < styleCreatorList.styles.Count; i++)
            {
                lineIndex++;

                if (reverseOrder == true && drawAltBG == true) { drawAltBG = false; }
                else if (reverseOrder == true && drawAltBG == false) { drawAltBG = true; }

                if (drawAltBG == true) { GUI.backgroundColor = lipBGColor; drawAltBG = false; }
                else { GUI.backgroundColor = lipAltBGColor; drawAltBG = true; }

                reverseOrder = false;
                if (lineIndex == 2) { reverseOrder = true; }

                GUILayout.BeginHorizontal(lipStyle);
                GUI.backgroundColor = defaultColor;
                GUILayout.BeginVertical();

                GUILayout.Box(styleCreatorList.styles[i].stylePreview, customSkin.FindStyle("Style Creator Preview"));
                EditorGUILayout.LabelField(styleCreatorList.styles[i].styleTitle, customSkin.FindStyle("Style Creator Title"));

                GUILayout.BeginHorizontal();
                GUILayout.Space(85);

                if (styleCreatorList.styles[i].customContent == true)
                    GUILayout.Box(styleCreatorList.ccEnabled, customSkin.FindStyle("Style Creator Indicator"));
                else
                    GUILayout.Box(styleCreatorList.ccDisabled, customSkin.FindStyle("Style Creator Indicator"));

                if (styleCreatorList.styles[i].customizableWidth == true)
                    GUILayout.Box(styleCreatorList.cwEnabled, customSkin.FindStyle("Style Creator Indicator"));
                else
                    GUILayout.Box(styleCreatorList.cwDisabled, customSkin.FindStyle("Style Creator Indicator"));

                if (styleCreatorList.styles[i].customizableHeight == true)
                    GUILayout.Box(styleCreatorList.chEnabled, customSkin.FindStyle("Style Creator Indicator"));
                else
                    GUILayout.Box(styleCreatorList.chDisabled, customSkin.FindStyle("Style Creator Indicator"));

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("", customSkin.FindStyle("Style Creator Play"))) { PlayPreviewVideo(i); }
                if (GUILayout.Button("", customSkin.FindStyle("Style Creator Create"))) { CreateObject(i); }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                if (lineIndex == 2)
                {
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    lineIndex = 0;
                }
            }

            // Scroll Panel End
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            if (GUI.enabled == true)
                Repaint();
        }

        public void PlayPreviewVideo(int styleIndex)
        {
            StyleVideoPreview videoWindow = (StyleVideoPreview)EditorWindow.GetWindow(typeof(StyleVideoPreview));
            GUIContent titleContent = new GUIContent("MTP Preview: " + styleCreatorList.styles[styleIndex].styleTitle);
            videoWindow.titleContent = titleContent;
            videoWindow.videoClip = styleCreatorList.styles[styleIndex].styleVideoPreview;
            videoWindow.UpdateVideo();
            tempWindow = videoWindow;
        }

        public void CreateObject(int styleIndex)
        {
            try
            {
                GameObject clone = Instantiate(styleCreatorList.styles[styleIndex].stylePrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                clone.name = clone.name.Replace("(Clone)", "").Trim();
                Selection.activeObject = clone;

                // Move to canvas if available
                try
                {
                    var canvas = (Canvas)GameObject.FindObjectsOfType(typeof(Canvas))[0];
                    if (canvas != null) { clone.transform.SetParent(canvas.transform, false); }
                }

                catch { Debug.Log("<b>[MTP]</b> Canvas not found, creating the object outside of a canvas."); }

                Undo.RegisterCreatedObjectUndo(clone, clone.name);
                if (Application.isPlaying == false) { EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); }
            }

            catch { Debug.LogError("<b>[MTP Creator]</b> Something went wrong while creating the object. There might be missing MTP resources."); }
        }

        public void ShowRootManager()
        {
            Selection.activeObject = Resources.Load("Root Manager");

            if (Selection.activeObject == null)
                Debug.Log("<b>[Motion Titles Pack]</b> Cannot find the manager. Make sure you have 'Root Manager' asset in Resources folder.");
        }
    }
}