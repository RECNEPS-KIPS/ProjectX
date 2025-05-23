
using UnityEngine;

namespace ZZZ
{
    [System.Serializable]
    public class PlayerComboSOData
    {
        [field: SerializeField, Header("轻攻击连招")] public ComboContainerData lightCombo { get; private set; }
        [field: SerializeField, Header("重攻击连招")] public ComboContainerData heavyCombo { get; private set; }
        [field: SerializeField, Header("处决")] public ComboContainerData executeCombo { get; private set; }
        [field: SerializeField, Header("大招")] public ComboData skillCombo { get; private set; }
        [field: SerializeField, Header("终极大招")] public ComboData finishSkillCombo { get; private set; }
        [field:SerializeField,Header("切人大招")] public ComboData switchSkill { get; private set; }
    }
}
