using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public abstract class GameBaseScene : MonoBehaviour
    {
        protected virtual void Awake()
        {
            GameRoot.Instance.RegisterModuleFromScene(this);
        }

        protected virtual void Start()
        {
            
        }
        
        public abstract List<GameBaseModule> RegisterGameModules();
    }
}