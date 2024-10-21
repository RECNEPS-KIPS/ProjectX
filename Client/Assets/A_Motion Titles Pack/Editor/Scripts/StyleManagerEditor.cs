using UnityEngine;
using UnityEditor;

namespace Michsky.UI.MTP
{
    [CustomEditor(typeof(StyleManager))]
    public class StyleManagerEditor : Editor
    {
        private StyleManager sTarget;
        private AnimationClip tempAnim;
        private int currentTab;
        private int currentAnim;
        bool playAnim;

        GUISkin customSkin;

        public string[] animOptions = new string[] { "In", "Out" };

        private void OnEnable()
        {
            sTarget = (StyleManager)target;
            sTarget.inspectAnim = false;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Skin\\MTP Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Skin\\MTP Skin Light"); }
        }

        private void OnDisable()
        {
            sTarget.tempAnimTime = 0;

            if (AnimationMode.InAnimationMode() == true)
            {
                AnimationMode.EndSampling();
                AnimationMode.StopAnimationMode();
            }
        }

        public override void OnInspectorGUI()
        {
            MTPEditorHandler.DrawComponentHeader(customSkin, "SM Top Header");

            GUIContent[] toolbarTabs = new GUIContent[4];
            toolbarTabs[0] = new GUIContent("Content");
            toolbarTabs[1] = new GUIContent("Animation");
            toolbarTabs[2] = new GUIContent("Resources");
            toolbarTabs[3] = new GUIContent("Settings");

            currentTab = MTPEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Content", "Content"), customSkin.FindStyle("Tab Content")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Animation", "Animation"), customSkin.FindStyle("Tab Animation")))
                currentTab = 1;
            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 2;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 3;

            GUILayout.EndHorizontal();

            var editMode = serializedObject.FindProperty("editMode");
            var textItems = serializedObject.FindProperty("textItems");
            var imageItems = serializedObject.FindProperty("imageItems");
            var styleAnimator = serializedObject.FindProperty("styleAnimator");
            var inAnim = serializedObject.FindProperty("inAnim");
            var outAnim = serializedObject.FindProperty("outAnim");
            var forceUpdate = serializedObject.FindProperty("forceUpdate");
            var scale = serializedObject.FindProperty("scale");
            var customScale = serializedObject.FindProperty("customScale");
            var playOnEnable = serializedObject.FindProperty("playOnEnable");
            var animationSpeed = serializedObject.FindProperty("animationSpeed");
            var showFor = serializedObject.FindProperty("showFor");
            var customContent = serializedObject.FindProperty("customContent");
            var customizableWidth = serializedObject.FindProperty("customizableWidth");
            var customizableHeight = serializedObject.FindProperty("customizableHeight");
            var onEnable = serializedObject.FindProperty("onEnable");
            var onDisable = serializedObject.FindProperty("onDisable");
            var disableOnOut = serializedObject.FindProperty("disableOnOut");
            var playOutAnimation = serializedObject.FindProperty("playOutAnimation");
            var loopAnimations = serializedObject.FindProperty("loopAnimations");

            switch (currentTab)
            {
                case 0:
                    GUILayout.Space(6);

                    if (customScale.boolValue == false)
                    {
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);

                        EditorGUILayout.LabelField(new GUIContent("Scale"), customSkin.FindStyle("Text"), GUILayout.Width(40));
                        EditorGUILayout.PropertyField(scale, new GUIContent(""));
                        sTarget.transform.localScale = new Vector3(scale.floatValue, scale.floatValue, scale.floatValue);

                        GUILayout.EndHorizontal();
                        GUILayout.Space(6);
                    }

                    if (sTarget.textItems.Count != 0 && sTarget.textItems[0] != null)
                    {
                        GUILayout.Box(new GUIContent(""), customSkin.FindStyle("Texts Top Header"));
                        DrawTextItem(0);
                    }

                    if (1 < sTarget.textItems.Count && sTarget.textItems[1] != null)
                    {
                        GUILayout.Space(3);
                        DrawTextItem(1);
                    }

                    if (2 < sTarget.textItems.Count && sTarget.textItems[2] != null)
                    {
                        GUILayout.Space(3);
                        DrawTextItem(2);
                    }

                    if (3 < sTarget.textItems.Count && sTarget.textItems[3] != null)
                    {
                        GUILayout.Space(3);
                        DrawTextItem(3);
                    }

                    ///////////////////////////////////////////////////////////////////

                    if (sTarget.imageItems.Count != 0 && sTarget.imageItems[0] != null)
                    {
                        if (sTarget.textItems.Count != 0)
                            GUILayout.Space(14);

                        GUILayout.Box(new GUIContent(""), customSkin.FindStyle("Images Top Header"));
                        DrawImageItem(0);
                    }

                    if (1 < sTarget.imageItems.Count && sTarget.imageItems[1] != null)
                    {
                        GUILayout.Space(3);
                        DrawImageItem(1);
                    }

                    if (2 < sTarget.imageItems.Count && sTarget.imageItems[2] != null)
                    {
                        GUILayout.Space(3);
                        DrawImageItem(2);
                    }

                    if (3 < sTarget.imageItems.Count && sTarget.imageItems[3] != null)
                    {
                        GUILayout.Space(3);
                        DrawImageItem(3);
                    }

                    MTPEditorHandler.DrawHeader(customSkin, "Info Top Header", 10);

                    if (customContent.boolValue == true)
                    {
                        EditorGUILayout.HelpBox("This style supports custom content. " +
                            "You can create your own objects under 'Mask Content'.", MessageType.Info);
                    }

                    else
                    {
                        EditorGUILayout.HelpBox("This style does not support custom content. " +
                           "You can't add custom objects.", MessageType.Warning);
                    }

                    if (customizableWidth.boolValue == true)
                    {
                        EditorGUILayout.HelpBox("This style supports dynamic width. " +
                            "You can change width using Rect Transform freely.", MessageType.Info);
                    }

                    else
                    {
                        EditorGUILayout.HelpBox("This style does not support dynamic width. " +
                           "Only use 'Scale' parameter to change the size.", MessageType.Warning);
                    }

                    if (customizableHeight.boolValue == true)
                    {
                        EditorGUILayout.HelpBox("This style supports dynamic height. " +
                            "You can change height using Rect Transform freely.", MessageType.Info);
                    }

                    else
                    {
                        EditorGUILayout.HelpBox("This style does not support dynamic height. " +
                           "Only use 'Scale' parameter to change the size.", MessageType.Warning);
                    }

                    break;

                case 1:
                    if (sTarget.inAnim == null || sTarget.outAnim == null)
                    {
                        EditorGUILayout.HelpBox("Animation variables are missing. Switch to Resources tab and assign the correct variables.", MessageType.Error);
                        return;
                    }

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(-3);
                    sTarget.inspectAnim = MTPEditorHandler.DrawTogglePlain(sTarget.inspectAnim, customSkin, "Inspect Animations");
                    GUILayout.Space(3);

                    if (currentAnim == 0)
                        tempAnim = sTarget.inAnim;
                    else if (currentAnim == 1)
                        tempAnim = sTarget.outAnim;

                    UpdateAnimation();

                    if (sTarget.inspectAnim == false && AnimationMode.InAnimationMode())
                    {
                        AnimationMode.EndSampling();
                        AnimationMode.StopAnimationMode();
                    }

                    if (sTarget.inspectAnim == true)
                    {
                        if (tempAnim == null && sTarget.styleAnimator == null)
                        {
                            EditorGUILayout.HelpBox("Animation cannot be found.", MessageType.Error);
                            return;
                        }

                        if (!AnimationMode.InAnimationMode())
                        {
                            sTarget.tempAnimTime = 0;
                            AnimationMode.InAnimationMode();
                            AnimationMode.StartAnimationMode();
                        }
                    }

                    float startTime = 0.0f;
                    float stopTime = tempAnim.length;

                    EditorGUI.BeginDisabledGroup(sTarget.inspectAnim == false);
                    GUILayout.Space(2);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(new GUIContent("Selected Animation:"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    currentAnim = EditorGUILayout.Popup(currentAnim, animOptions);

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(33);

                    EditorGUILayout.LabelField(new GUIContent(startTime.ToString("F2") + "s"), customSkin.FindStyle("Text"), GUILayout.Width(35));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(new GUIContent(stopTime.ToString("F2") + "s"), customSkin.FindStyle("Text"), GUILayout.Width(35));

                    GUILayout.EndHorizontal();
                    GUILayout.Space(-26);
                    GUILayout.Box(new GUIContent(""), customSkin.FindStyle("Anim Icon"));
                    GUILayout.Space(-25);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(35);

                    sTarget.tempAnimTime = EditorGUILayout.Slider(sTarget.tempAnimTime, startTime, stopTime);

                    GUILayout.EndHorizontal();
                    GUILayout.Space(3);
                    GUILayout.BeginHorizontal();
                    EditorGUI.EndDisabledGroup();

                    if (playAnim == false && GUILayout.Button("▶ Play"))
                    {
                        sTarget.inspectAnim = true;
                        sTarget.tempAnimTime = 0;
                        playAnim = true;
                    }

                    if (playAnim == true && GUILayout.Button("■ Stop"))
                        playAnim = false;

                    if (GUILayout.Button("Inspect via animation window"))
                    {
                        sTarget.inspectAnim = false;
                        sTarget.tempAnimTime = 0;
                        EditorApplication.ExecuteMenuItem("Window/Animation/Animation");
                    }

                    GUILayout.EndHorizontal();

                    if (sTarget.inspectAnim == true)
                    {
                        GUILayout.Space(3);
                        EditorGUILayout.HelpBox("Animation is locked to 30 FPS while inpecting.\n" +
                        "To see the animation at 60 FPS, use the animation window.", MessageType.Info);
                    }

                    GUILayout.EndVertical();
                    GUILayout.EndVertical();

                    if (playAnim == true)
                    {
                        sTarget.tempAnimTime += Time.deltaTime / 4f;
                        this.Repaint();

                        if (sTarget.tempAnimTime >= tempAnim.length)
                            playAnim = false;
                    }

                    break;

                case 2:
                    MTPEditorHandler.DrawProperty(styleAnimator, customSkin, "Style Animator");
                    GUILayout.Space(2);
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    GUILayout.Space(2);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(-3);
                    editMode.boolValue = MTPEditorHandler.DrawTogglePlain(editMode.boolValue, customSkin, "Dev Mode");
                    GUILayout.Space(3);

                    if (editMode.boolValue == true)
                    {
                        GUILayout.Space(3);
                        EditorGUILayout.HelpBox("Please don't change anything if you don't know what you're doing. " +
                            "Changing these variables might break the object!", MessageType.Warning);
                        GUILayout.Space(3);
                        EditorGUI.indentLevel = 1;
                        EditorGUILayout.PropertyField(textItems, new GUIContent("Text Items"));
                        EditorGUILayout.PropertyField(imageItems, new GUIContent("Image Items"));
                        EditorGUI.indentLevel = 0;
                        EditorGUILayout.PropertyField(inAnim, new GUIContent("In Anim"));
                        EditorGUILayout.PropertyField(outAnim, new GUIContent("Out Anim"));
                        EditorGUILayout.PropertyField(customContent, new GUIContent("Custom Content"));
                        EditorGUILayout.PropertyField(customizableWidth, new GUIContent("Customizable Width"));
                        EditorGUILayout.PropertyField(customizableHeight, new GUIContent("Customizable Height"));
                    }

                    GUILayout.EndVertical();
                    break;

                case 3:
                    MTPEditorHandler.DrawHeader(customSkin, "Style Top Header", 6);
                    forceUpdate.boolValue = MTPEditorHandler.DrawToggle(forceUpdate.boolValue, customSkin, "Force To Update At Start");
                    customScale.boolValue = MTPEditorHandler.DrawToggle(customScale.boolValue, customSkin, "Use Custom Scale");
                    playOnEnable.boolValue = MTPEditorHandler.DrawToggle(playOnEnable.boolValue, customSkin, "Play On Enable");
                    playOutAnimation.boolValue = MTPEditorHandler.DrawToggle(playOutAnimation.boolValue, customSkin, "Play Out Animation");
                    disableOnOut.boolValue = MTPEditorHandler.DrawToggle(disableOnOut.boolValue, customSkin, "Disable On Out");
                    loopAnimations.boolValue = MTPEditorHandler.DrawToggle(loopAnimations.boolValue, customSkin, "Loop Animations");
                    MTPEditorHandler.DrawProperty(animationSpeed, customSkin, "Animation Speed");
                    MTPEditorHandler.DrawProperty(showFor, customSkin, "Show For (s)");

                    MTPEditorHandler.DrawHeader(customSkin, "Events Top Header", 10);
                    EditorGUILayout.PropertyField(onEnable);
                    EditorGUILayout.PropertyField(onDisable);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        void UpdateAnimation()
        {
            if (sTarget == null && sTarget.styleAnimator.runtimeAnimatorController == null && tempAnim == null)
                return;

            if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode())
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(sTarget.gameObject, tempAnim, sTarget.tempAnimTime);
                AnimationMode.EndSampling();
            }
        }

        void DrawTextItem(int index)
        {
            SerializedObject tempSerializedObj = new SerializedObject(sTarget.textItems[index]);
            var _text = tempSerializedObj.FindProperty("text");
            var _selectedFont = tempSerializedObj.FindProperty("selectedFont");
            var _fontSize = tempSerializedObj.FindProperty("fontSize");
            var _textColor = tempSerializedObj.FindProperty("textColor");

            GUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent(sTarget.textItems[index].itemID), customSkin.FindStyle("Text"));
            EditorGUILayout.PropertyField(_text, new GUIContent(""));

            GUILayout.Space(3);
            GUILayout.BeginHorizontal();

            if (sTarget.textItems[index].textObject.enableAutoSizing == false)
            {
                EditorGUILayout.PropertyField(_fontSize, new GUIContent(""), GUILayout.Width(40));
                GUILayout.Space(3);
            }

            EditorGUILayout.PropertyField(_selectedFont, new GUIContent(""));
            GUILayout.Space(3);
            EditorGUILayout.PropertyField(_textColor, new GUIContent(""));

            GUILayout.EndHorizontal();
            GUILayout.Space(3);

            if (GUILayout.Button("Select Object"))
                Selection.activeObject = sTarget.textItems[index].textObject;

            GUILayout.EndVertical();

            tempSerializedObj.ApplyModifiedProperties();
            sTarget.textItems[index].UpdateAll();
        }

        void DrawImageItem(int index)
        {
            SerializedObject tempSerializedObj = new SerializedObject(sTarget.imageItems[index]);
            var _imageColor = tempSerializedObj.FindProperty("imageColor");
            var _preferGradient = tempSerializedObj.FindProperty("preferGradient");
            var _imageGradient = tempSerializedObj.FindProperty("imageGradient");
            var _thickness = tempSerializedObj.FindProperty("thickness");

            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(new GUIContent(sTarget.imageItems[index].itemID), customSkin.FindStyle("Text"));

            if (sTarget.imageItems[index].enableThickness == true)
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(new GUIContent("Thickness"), customSkin.FindStyle("Text"), GUILayout.Width(100));
                EditorGUILayout.PropertyField(_thickness, new GUIContent(""));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            _preferGradient.boolValue = GUILayout.Toggle(_preferGradient.boolValue, new GUIContent("Use Gradient"), customSkin.FindStyle("Toggle"), GUILayout.Width(125));
            _preferGradient.boolValue = GUILayout.Toggle(_preferGradient.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

            if (_preferGradient.boolValue == false) { EditorGUILayout.PropertyField(_imageColor, new GUIContent("")); }
            else { EditorGUILayout.PropertyField(_imageGradient, new GUIContent("")); }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Select Object"))
                Selection.activeObject = sTarget.imageItems[index].imageObject;

            GUILayout.EndVertical();

            tempSerializedObj.ApplyModifiedProperties();
            sTarget.imageItems[index].UpdateAll();
        }
    }
}