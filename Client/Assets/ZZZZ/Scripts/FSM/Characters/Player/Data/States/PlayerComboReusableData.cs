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
        public BindableProperty<int> currentIndex { get; set; } = new BindableProperty<int>();//防止因为index更新导致ATK转递index出现不对应的数值

        public bool canInput { get; set; }//输入的允许输入时间，相当于是否开启 预输入

        public bool canATK { get; set; }//攻击动画最小时间播放的开关，相当于连招 冷却时间

        public bool hasATKCommand { get; set; }//在可以攻击的条件下按下攻击键触发 攻击指令

        public bool canLink { get; set; }//可以衔接连招

        public bool canMoveInterrupt { get; set; }//可以通过移动打断
        public int executeIndex { get; set; }

        public bool canQTE { get; set; }//触发切人技能特写的条件
    }
}
