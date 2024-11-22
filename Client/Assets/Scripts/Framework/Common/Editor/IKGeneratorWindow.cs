using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RootMotion = Plugins.RootMotion;

namespace Framework.Common
{
    public class FootBones
    {
        public GameObject Bone1;
        public GameObject Bone2;
        public GameObject Bone3;

        public void Clear()
        {
            Bone1 = null;
            Bone2 = null;
            Bone3 = null;
        }
    }

    public class IKGeneratorWindow : EditorWindow
    {
        static EditorWindow mWindow;

        static GameObject mCurrentSelectGameObject;

        string[] mIKNames = { "IK Leg Left", "IK Leg Right", "IK Leg Front", "IK Leg Back" };
        bool isFourFootRobot = true;
        bool isTwoFootRobot = false;
        int footCount = 4;
        bool[] mSelectIKs = { false, false, false, false };
        static bool mSelectParentObject = false;
        static int mCurrentSelectIndex = -1;
        public static List<FootBones> mFootBones = new List<FootBones>();

        public bool IsFourFootRobot
        {
            get
            {
                return isFourFootRobot;
            }
            set
            {
                isFourFootRobot = value;
                if (value)
                {
                    footCount = 4;
                    if (mFootBones.Count != footCount)
                    {
                        ClearAndReInitFootBones(footCount);
                    }
                }
            }
        }
        public bool IsTwoFootRobot
        {
            get
            {
                return isTwoFootRobot;
            }
            set
            {
                isTwoFootRobot = value;
                if (value)
                {
                    footCount = 2;
                    if (mFootBones.Count != footCount)
                    {
                        ClearAndReInitFootBones(footCount);
                    }
                }
            }
        }

        void ClearAndReInitFootBones(int count)
        {
            mFootBones.Clear();
            for (int i = 0; i < count; i++)
            {
                mFootBones.Add(new FootBones());
            }
        }

        [UnityEditor.MenuItem("Tools/IK生成")]
        private static void Open()
        {
            mWindow = EditorWindow.GetWindow(typeof(IKGeneratorWindow), true, "IK生成器", true);
            mWindow.Show();
            mWindow.Focus();
            mWindow.position = new Rect(300, 400, 1390, 420);
            Selection.selectionChanged += OnSelectionChange;
        }

        void OnDestroy()
        {
            Selection.selectionChanged -= OnSelectionChange;
        }

        private static void OnSelectionChange()
        {
            if (mSelectParentObject)
                mCurrentSelectGameObject = Selection.activeGameObject;
            if (mCurrentSelectIndex >= 0)
            {
                if (mFootBones[mCurrentSelectIndex].Bone1 == null)
                    mFootBones[mCurrentSelectIndex].Bone1 = Selection.activeGameObject;
                else if (mFootBones[mCurrentSelectIndex].Bone2 == null)


                    mFootBones[mCurrentSelectIndex].Bone2 = Selection.activeGameObject;
                else if (mFootBones[mCurrentSelectIndex].Bone3 == null)
                    mFootBones[mCurrentSelectIndex].Bone3 = Selection.activeGameObject;
            }
            mWindow?.Repaint();
        }
        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if (mSelectParentObject = GUILayout.Toggle(mSelectParentObject, "开始选择Robot"))
            {

            }
            if (GUILayout.Button(new GUIContent("清空")))
            {
                mCurrentSelectGameObject = null;
            }
            GUILayout.Label(new GUIContent("你当前选择的是:" + mCurrentSelectGameObject?.name));

            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(new GUIContent("机器人类型选择一种:"));
            if (IsFourFootRobot = GUILayout.Toggle(IsFourFootRobot, new GUIContent("四足机器人")))
            {
                if (IsFourFootRobot)
                    IsTwoFootRobot = false;
                else
                    IsTwoFootRobot = true;
            }
            if (IsTwoFootRobot = GUILayout.Toggle(IsTwoFootRobot, new GUIContent("双足机器人")))
            {
                if (IsTwoFootRobot)
                    IsFourFootRobot = false;
                else
                    IsFourFootRobot = true;
            }
            GUILayout.Space(20);

            GUILayout.Label(new GUIContent("编辑Robot的IK:"));
            for (int i = 0; i < footCount; i++)
            {
                GUILayout.BeginHorizontal();
                if (mSelectIKs[i] = GUILayout.Toggle(mSelectIKs[i], "选中编辑"))
                {
                    if (mSelectIKs[i])
                        mCurrentSelectIndex = i;
                    else
                        mCurrentSelectIndex = -1;

                    Debug.Log("当前中选的是第" + mCurrentSelectIndex + "行");
                }
                if (GUILayout.Button("清空"))
                {
                    mFootBones[i].Clear();
                }
                GUILayout.Label(new GUIContent("Bone1:" + mFootBones[i]?.Bone1?.name));
                GUILayout.Label(new GUIContent("Bone2:" + mFootBones[i]?.Bone2?.name));
                GUILayout.Label(new GUIContent("Bone3:" + mFootBones[i]?.Bone3?.name));
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            GUILayout.Space(20);
            if (GUILayout.Button("添加IK"))
            {
                AddIK(mCurrentSelectGameObject);
            }

            GUILayout.Space(10);
            GUILayout.Label(new GUIContent("操作说明:\n1.将Robot拖放到Hierarchy中，2.选择Robot，勾选上Toogle，然后开始点击左侧的Robot节点，确认选择之后取消Toggle勾选\n2.选择双足还是四足机器人\n3.编辑Robot的IK，勾选上哪一行编辑在左侧点击选择GameObject就编辑哪一行\n4.点击添加IK\n说明:清空按钮是清空编辑的数据"));
        }

        void AddIK(GameObject gameObj)
        {
            if (gameObj == null)
            {
                Debug.LogError("当前没有选择的物体");
                return;
            }

            //var grounderIK = gameObj.transform.Find("Grounder IK");
            //if (grounderIK != null)
            //    Editor.DestroyImmediate(grounderIK.gameObject);

            var obj = new GameObject("Grounder IK");
            obj.transform.parent = gameObj.transform;
            var ikComponent = obj.AddComponent<RootMotion.FinalIK.GrounderIK>();
            ikComponent.characterRoot = gameObj.transform;
		    ikComponent.solver.layers = LayerMask.GetMask("Terrain", "Default"); //设置射线检测的层
            if (IsFourFootRobot)
                footCount = 4;
            else
                footCount = 2;
            ikComponent.legs = new RootMotion.FinalIK.IK[footCount];
            for (int i = 0; i < footCount; i++)
            {
                var ikleg = new GameObject(mIKNames[i]);
                ikleg.transform.parent = obj.transform;
                var limbIK = ikleg.AddComponent<RootMotion.FinalIK.LimbIK>();

                limbIK.solver.goal = i % 2 == 0 ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;

                var bone1 = new RootMotion.FinalIK.IKSolverTrigonometric.TrigonometricBone();
                bone1.transform = mFootBones[i].Bone1?.transform;
                limbIK.solver.bone1 = bone1;

                var bone2 = new RootMotion.FinalIK.IKSolverTrigonometric.TrigonometricBone();
                bone2.transform = mFootBones[i].Bone2?.transform;
                limbIK.solver.bone2 = bone2;

                var bone3 = new RootMotion.FinalIK.IKSolverTrigonometric.TrigonometricBone();
                bone3.transform = mFootBones[i].Bone3?.transform;
                limbIK.solver.bone3 = bone3;

                ikComponent.legs[i] = limbIK;
            }

        }
    }
    
}

