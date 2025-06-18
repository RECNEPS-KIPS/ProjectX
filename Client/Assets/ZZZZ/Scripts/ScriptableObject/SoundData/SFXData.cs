using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="SFXData",menuName ="Asset/SFX/SFXData")]
public class SFXData : ScriptableObject
{
    [SerializeField] public List<AudioClip>  SFXList = new List<AudioClip>();
}
