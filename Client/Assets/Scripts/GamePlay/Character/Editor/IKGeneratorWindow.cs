using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RootMotion = Plugins.RootMotion;

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
    static EditorWindow window;

    static GameObject currentSelectGameObject;

    private readonly string[] ikNames = { "IK Leg Left", "IK Leg Right", "IK Leg Front", "IK Leg Back" };
    private bool isFourFootRobot = true;
    private bool isTwoFootRobot;
    private int footCount = 4;
    private readonly bool[] selectIKs = { false, false, false, false };
    private static bool selectParentObject;
    private static int currentSelectIndex = -1;
    public static readonly List<FootBones> footBones = new();

    public bool IsFourFootRobot
    {
        get => isFourFootRobot;
        set
        {
            isFourFootRobot = value;
            if (!value) return;
            footCount = 4;
            if (footBones.Count != footCount)
            {
                ClearAndReInitFootBones(footCount);
            }
        }
    }

    public bool IsTwoFootRobot
    {
        get => isTwoFootRobot;
        set
        {
            isTwoFootRobot = value;
            if (!value) return;
            footCount = 2;
            if (footBones.Count != footCount)
            {
                ClearAndReInitFootBones(footCount);
            }
        }
    }

    private void ClearAndReInitFootBones(int count)
    {
        footBones.Clear();
        for (var i = 0; i < count; i++)
        {
            footBones.Add(new FootBones());
        }
    }

    [MenuItem("Tools/IKGeneratorWindow")]
    private static void Open()
    {
        window = GetWindow(typeof(IKGeneratorWindow), true, "IK生成器", true);
        window.Show();
        window.Focus();
        window.position = new Rect(300, 400, 1390, 420);
        Selection.selectionChanged += OnSelectionChange;
    }

    void OnDestroy()
    {
        Selection.selectionChanged -= OnSelectionChange;
    }

    private static void OnSelectionChange()
    {
        if (selectParentObject)
        {
            currentSelectGameObject = Selection.activeGameObject;
        }

        if (currentSelectIndex >= 0)
        {
            if (footBones[currentSelectIndex].Bone1 == null)
            {
                footBones[currentSelectIndex].Bone1 = Selection.activeGameObject;
            }
            else if (footBones[currentSelectIndex].Bone2 == null)


            {
                footBones[currentSelectIndex].Bone2 = Selection.activeGameObject;
            }
            else if (footBones[currentSelectIndex].Bone3 == null)
            {
                footBones[currentSelectIndex].Bone3 = Selection.activeGameObject;
            }
        }

        window?.Repaint();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (selectParentObject == GUILayout.Toggle(selectParentObject, "开始选择Robot"))
        {
        }

        if (GUILayout.Button(new GUIContent("清空")))
        {
            currentSelectGameObject = null;
        }

        GUILayout.Label(new GUIContent("你当前选择的是:" + (currentSelectGameObject != null ? currentSelectGameObject.name : "")));

        GUILayout.EndHorizontal();
        GUILayout.Space(20);
        GUILayout.Label(new GUIContent("机器人类型选择一种:"));
        if (IsFourFootRobot == GUILayout.Toggle(IsFourFootRobot, new GUIContent("四足机器人")))
        {
            if (IsFourFootRobot)
            {
                IsTwoFootRobot = false;
            }
            else
            {
                IsTwoFootRobot = true;
            }
        }

        if (IsTwoFootRobot == GUILayout.Toggle(IsTwoFootRobot, new GUIContent("双足机器人")))
        {
            if (IsTwoFootRobot)
            {
                IsFourFootRobot = false;
            }
            else
            {
                IsFourFootRobot = true;
            }
        }

        GUILayout.Space(20);

        GUILayout.Label(new GUIContent("编辑Robot的IK:"));
        for (int i = 0; i < footCount; i++)
        {
            GUILayout.BeginHorizontal();
            if (selectIKs[i] == GUILayout.Toggle(selectIKs[i], "选中编辑"))
            {
                if (selectIKs[i])
                {
                    currentSelectIndex = i;
                }
                else
                {
                    currentSelectIndex = -1;
                }

                LogManager.Log("IKGeneratorWindow", "当前中选的是第" + currentSelectIndex + "行");
            }

            if (GUILayout.Button("清空"))
            {
                footBones[i].Clear();
            }

            var bone1 = string.Empty;
            if (footBones[i] != null && footBones[i].Bone1 != null)
            {
                bone1 = footBones[i].Bone1.name;
            }

            var bone2 = string.Empty;
            if (footBones[i] != null && footBones[i].Bone2 != null)
            {
                bone2 = footBones[i].Bone1.name;
            }

            var bone3 = string.Empty;
            if (footBones[i] != null && footBones[i].Bone3 != null)
            {
                bone3 = footBones[i].Bone1.name;
            }

            GUILayout.Label(new GUIContent($"Bone1:{bone1}"));
            GUILayout.Label(new GUIContent($"Bone2:{bone2}"));
            GUILayout.Label(new GUIContent($"Bone3:{bone3}"));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("添加IK"))
        {
            AddIK(currentSelectGameObject);
        }

        GUILayout.Space(10);
        GUILayout.Label(new GUIContent(
            "操作说明:\n1.将Robot拖放到Hierarchy中，2.选择Robot，勾选上Toogle，然后开始点击左侧的Robot节点，确认选择之后取消Toggle勾选\n2.选择双足还是四足机器人\n3.编辑Robot的IK，勾选上哪一行编辑在左侧点击选择GameObject就编辑哪一行\n4.点击添加IK\n说明:清空按钮是清空编辑的数据"));
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

        var obj = new GameObject("Grounder IK")
        {
            transform =
            {
                parent = gameObj.transform
            }
        };
        var ikComponent = obj.AddComponent<RootMotion.FinalIK.GrounderIK>();
        ikComponent.characterRoot = gameObj.transform;
        ikComponent.solver.layers = LayerMask.GetMask("Default");//.GetMask("Terrain", "Default"); //设置射线检测的层
        footCount = IsFourFootRobot ? 4 : 2;

        ikComponent.legs = new RootMotion.FinalIK.IK[footCount];
        for (var i = 0; i < footCount; i++)
        {
            var ikleg = new GameObject(ikNames[i])
            {
                transform =
                {
                    parent = obj.transform
                }
            };
            var limbIK = ikleg.AddComponent<RootMotion.FinalIK.LimbIK>();

            limbIK.solver.goal = i % 2 == 0 ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;

            var bone1 = new RootMotion.FinalIK.IKSolverTrigonometric.TrigonometricBone();
            if (footBones[i].Bone1 != null)
            {
                bone1.transform = footBones[i].Bone1.transform;
            }

            limbIK.solver.bone1 = bone1;

            var bone2 = new RootMotion.FinalIK.IKSolverTrigonometric.TrigonometricBone();
            if (footBones[i].Bone2 != null)
            {
                bone2.transform = footBones[i].Bone2.transform;
            }

            limbIK.solver.bone2 = bone2;

            var bone3 = new RootMotion.FinalIK.IKSolverTrigonometric.TrigonometricBone();
            if (footBones[i].Bone3 != null)
            {
                bone3.transform = footBones[i].Bone3.transform;
            }

            limbIK.solver.bone3 = bone3;

            ikComponent.legs[i] = limbIK;
        }
    }
}