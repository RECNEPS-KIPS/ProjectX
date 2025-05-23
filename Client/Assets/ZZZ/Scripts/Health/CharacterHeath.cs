
using UnityEngine;
using ZZZ;
public class CharacterHeath : CharacterHealthBase
{
    /// <summary>
    /// 敌人的伤害处理
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="hitName"></param>
    /// <param name="parryName"></param>
    protected override void CharacterHitAction(float damage, string hitName, string parryName)
    {
        base.CharacterHitAction(damage, hitName, parryName);
        if (healthInfo.hasStrength.Value)//格挡
        {
            healthInfo.TakeStrength(damage);
            animator.CrossFadeInFixedTime(parryName, 0.1f, 0);
           // SFX_PoolManager.MainInstance.TryGetSoundPool("PARRY",transform.position,Quaternion.identity);
            
        }
        else//挨打
        {
         
            healthInfo.TakeDamage(damage);
            animator.CrossFadeInFixedTime(hitName, 0.1f, 0);
            //SFX_PoolManager.MainInstance.TryGetSoundPool("HIT", transform.position, Quaternion.identity);
        }
        healthInfo.TakeDefenseValue(damage);
    }
    /// <summary>
    /// 处理敌人的破防逻辑   
    /// </summary>
    /// <param name="value"></param>
    protected override void OnUpdateDefenseValue(float value)
    {
        base.OnUpdateDefenseValue(value);
        if (currentEnemy == null) { return; }
        if (value <= 0)
        {
            GameEventsManager.MainInstance.CallEvent("达到QTE条件", currentEnemy);
            healthInfo.ReDefenseValue();
            return;
        }
    }
    //处理受击音效
    protected override void SetHitSFX(CharacterNameList characterNameList)
    {

        SFX_PoolManager.MainInstance.TryGetSoundPool(SoundStyle.HIT, characterNameList.ToString(), transform.position);
    }
}
