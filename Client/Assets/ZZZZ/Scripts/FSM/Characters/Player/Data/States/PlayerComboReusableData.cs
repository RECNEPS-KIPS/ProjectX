using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZZZ
{
    public class PlayerComboReusableData
    {
        public Transform cameraTransform { get; set; }

        public Vector3 detectionDir { get; set; }

        public Vector3 detectionOrigin { get; set; }
        public ComboContainerData currentCombo { get; set; }
        public ComboData currentSkill { get; set; }

        public int ATKIndex { get; set; }
        public int comboIndex { get; set; }
        public BindableProperty<int> currentIndex { get; set; } = new BindableProperty<int>();

        public bool canInput { get; set; }

        public bool canATK { get; set; }

        public bool hasATKCommand { get; set; }

        public bool canLink { get; set; }

        public bool canMoveInterrupt { get; set; }
        public int executeIndex { get; set; }

        public bool canQTE { get; set; }
    }
}