using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="CharacterHealthInfo",menuName ="Create/Asset/CharacterHealthInfo")]
public class CharacterHealthInfo : ScriptableObject
{
    [SerializeField]private CharacterHealthData healthData;
  
    public BindableProperty<float> currentHP = new BindableProperty<float>();
    public BindableProperty<float> currentStrength = new BindableProperty<float>();
    public BindableProperty<float> currentDefenseValue = new BindableProperty<float>();
    public BindableProperty<bool> onDead = new BindableProperty<bool>();
    public BindableProperty<bool> hasStrength =new BindableProperty<bool>();
    public float maxHP;
  
    //属性封装不会显示在unity面板上

    public void InitHealthData()
    {
        maxHP= healthData.healthData.maxHP;
        currentHP.Value = healthData.healthData.maxHP;
        currentStrength.Value=healthData.healthData.maxStrength;
        currentDefenseValue.Value = healthData.healthData.maxDefenseValue;
        Debug.Log("敌人初始化的血量为" + currentHP.Value);
    }
    public void TakeDamage(float Damage)
    {
        if (!hasStrength.Value)
        {

            currentHP.Value = TakeHealthValue(currentHP.Value, Damage, healthData.healthData.maxHP, false);
        }
     
    }
    public void TakeStrength(float Damage)
    {
        if (hasStrength.Value)
        {
            currentStrength.Value = TakeHealthValue(currentStrength.Value, Damage, healthData.healthData.maxHP, false);
        }
    }
    public void TakeDefenseValue(float Damage)
    {
        currentDefenseValue.Value = TakeHealthValue(currentDefenseValue.Value, Damage, healthData.healthData.maxDefenseValue, false);
    }
    public void RePH(float Damage)
    {
        currentHP.Value = TakeHealthValue(currentHP.Value, Damage, healthData.healthData.maxHP, true);
    }
    public void ReStrength(float Damage)
    {
        currentStrength.Value = TakeHealthValue(currentStrength.Value, Damage, healthData.healthData.maxStrength, true);
    }
    /// <summary>
    /// 回满防御值
    /// </summary>
    public void ReDefenseValue()
    { 
    currentDefenseValue.Value = TakeHealthValue(currentDefenseValue.Value, healthData.healthData.maxDefenseValue, healthData.healthData.maxDefenseValue, true);
    }
    
    private float TakeHealthValue(float currentValue,float offsetValue,float maxValue,bool canAdd)
    {
        return Mathf.Clamp(canAdd?currentValue+ offsetValue:currentValue-offsetValue,0, maxValue);
    
    }
}
