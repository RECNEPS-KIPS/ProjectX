using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHideCursor : MonoBehaviour
{
    private void Start()
    {
        UpdateCorcur();
    }

    private void UpdateCorcur()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}