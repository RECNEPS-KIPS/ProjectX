using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterHealthData",menuName = "Create/Asset/CharacterHealthData")]
public class CharacterHealthData : ScriptableObject
{
    [field: SerializeField] public HealthData healthData { get;private  set; }
    //field必须在 { get;  set; }访问器的作用下才能对属性序列化
}

