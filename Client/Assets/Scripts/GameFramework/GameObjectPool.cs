using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    public static GameObjectPool Instance;
    
    private void Awake()
    {
        Instance = this;
    }
    
    
}
