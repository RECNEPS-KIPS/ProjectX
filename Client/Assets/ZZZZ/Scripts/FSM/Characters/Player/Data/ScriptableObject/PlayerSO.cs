
using UnityEngine;
namespace ZZZ
{
    [CreateAssetMenu(fileName ="Player",menuName ="Create/Character/Player")]
    
    public class PlayerSO:ScriptableObject
    {
        //field会字段显示在编辑面板上，哪怕是一个私有字段
        [field: SerializeField] public PlayerMovementData movementData{ get; private set; }

        [field: SerializeField] public PlayerComboData ComboData { get; private set; }
    }
}
