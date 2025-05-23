using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZZZ
{
    [System.Serializable]
    public class PlayerDashData
    {
        [field: SerializeField] public string frontDushAnimationName { get; private set; } = "Dodge_Front";
        [field: SerializeField] public string backDushAnimationName { get; private set; } = "Dodge_Back";
        [field: SerializeField] public float fadeTime { get; private set; } = 0.1555f;
        [field: SerializeField] public float rotationTime { get; private set; } = 0.09f;
        [field: SerializeField] public bool dodgeBackApplyRotation{ get; private set; } = false;    
        [field: SerializeField] public float coldTime { get; private set; } = 0.5f;
    }
}
