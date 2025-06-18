using GGG.Tool;
using UnityEngine;
using ZZZ;
// using static UnityEngine.Rendering.DebugUI;

public class CharacterHealthBase : MonoBehaviour
{
    [SerializeField] private float currentHP;
    [SerializeField] private float currentStrength;
    [SerializeField] private float currentDefenseValue;
    protected Transform currentEnemy;
    [SerializeField]protected CharacterHealthInfo characterHealthInfo;
    protected CharacterHealthInfo healthInfo;
    protected Animator animator;
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        healthInfo=Instantiate(characterHealthInfo);

        healthInfo.currentHP.OnValueChanged += OnUpdatePH;
        healthInfo.currentStrength.OnValueChanged += OnUpdateStrength;
        healthInfo.currentDefenseValue.OnValueChanged += OnUpdateDefenseValue;
    }

 
    private void Start()
    {
        healthInfo.InitHealthData();
    }
    protected virtual void Update()
    {
        LookAtAttacker();
    }
    protected virtual void OnEnable()
    {
        GameEventsManager.MainInstance.AddEventListening<float,string,string,Transform,Transform, ZZZ.CharacterComboBase>("�����˺�", OnCharacterHitEventHandler);

        GameEventsManager.MainInstance.AddEventListening<float>("�����˺�", OnCharacterDamageAction);

    }

 

    protected virtual void OnDisable()
    {
        GameEventsManager.MainInstance.ReMoveEvent<float, string, string, Transform, Transform, ZZZ.CharacterComboBase>("�����˺�", OnCharacterHitEventHandler);

        GameEventsManager.MainInstance.ReMoveEvent<float>("�����˺�", OnCharacterDamageAction);

        healthInfo.currentHP.OnValueChanged -= OnUpdatePH;

        healthInfo.currentStrength.OnValueChanged -= OnUpdateStrength;

        healthInfo.currentDefenseValue.OnValueChanged -= OnUpdateDefenseValue;

    }

  
    //�¼�ע��
    private void OnCharacterHitEventHandler(float Damage, string HitName, string ParryName, Transform Attacker, Transform Bearer, ZZZ.CharacterComboBase characterCombo)
    {
      
        if (Bearer != this.transform) { return; }
        SetEnemy(Attacker);
        CharacterHitAction(Damage, HitName, ParryName);
        OnCharacterDamageAction(Damage);
        SetHitFVX(Attacker,Bearer);
        SetHitSFX(characterCombo.player.characterName);
    }

 
    protected void OnCharacterDamageAction(float damage)
    {
        healthInfo.TakeDamage(damage);
    }
    protected void CharacterStrengthAction(float damage)
    {
        healthInfo.TakeStrength(damage);
    }

    protected virtual void CharacterHitAction(float damage, string hitName, string parryName)
    {
        
    }

    protected virtual void SetEnemy(Transform attacker)
    {
        if (currentEnemy != attacker || currentEnemy == null)
        { 
            currentEnemy = attacker;    
        }
    }
    private void LookAtAttacker()
    {
        if (currentEnemy == null) { return; }
        if (animator.AnimationAtTag("Hit")&&animator.GetCurrentAnimatorStateInfo(0).normalizedTime<0.3f)
        {
            transform.Look(currentEnemy.position, 50);
        }
        
    
    }
    protected void SetHitFVX(Transform attacker,Transform hitter)
    { 
        Vector3 hitDir = (attacker.position- hitter.position).normalized;
        Vector3 targetPos = hitter.position + hitDir*0.8f+Vector3.up*1f;
        VFX_PoolManager.MainInstance.GetVFX(ZZZ.CharacterNameList.Enemy,"Hit", targetPos);
    }
    protected virtual void SetHitSFX(CharacterNameList characterNameList)
    {
       
    }

    private void OnUpdatePH(float value)
    {
        Debug.Log("���˵�Ѫ��Ϊ"+value);
        currentHP = value;
        if (value > 0)
        {
            healthInfo.onDead.Value = false;
            ZZZZUIManager.MainInstance.stateBarUI.UpdateBlood(currentHP / healthInfo.maxHP);
            return;
        }
        healthInfo.onDead.Value = true;


    }
    private void OnUpdateStrength(float value)
    {
        currentStrength = value;
        if (value > 0)
        {
            healthInfo.hasStrength.Value = true;
            return;
        }
        healthInfo.hasStrength.Value = false;
    }
    protected virtual void OnUpdateDefenseValue(float value)
    {
        currentDefenseValue = value;
    }

}
