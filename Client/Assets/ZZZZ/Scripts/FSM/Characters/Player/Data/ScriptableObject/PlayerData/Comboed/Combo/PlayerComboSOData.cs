using UnityEngine;

namespace ZZZ
{
    [System.Serializable]
    public class PlayerComboSOData
    {
        [field: SerializeField, Header("lightCombo")]
        public ComboContainerData lightCombo { get; private set; }

        [field: SerializeField, Header("heavyCombo")]
        public ComboContainerData heavyCombo { get; private set; }

        [field: SerializeField, Header("executeCombo")]
        public ComboContainerData executeCombo { get; private set; }

        [field: SerializeField, Header("executeCombo")]
        public ComboData skillCombo { get; private set; }

        [field: SerializeField, Header("finishSkillCombo")]
        public ComboData finishSkillCombo { get; private set; }

        [field: SerializeField, Header("finishSkillCombo")]
        public ComboData switchSkill { get; private set; }
    }
}