using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodPos : MonoBehaviour
{
    private Camera cam;  
    private void Awake()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        syncBloodUI();
    }

    private void syncBloodUI()
    {
        ZZZZUIManager.MainInstance.stateBarUI.ShowAt(this.transform.position);
    }
}
