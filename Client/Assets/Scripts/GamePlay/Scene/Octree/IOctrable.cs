// author:KIPKIPS
// date:2024.10.25 21:07
// describe:
using System;
using UnityEngine;

namespace GamePlay.Scene
{
    public interface IOctrable
    {
        Transform ColliderTrs { get; set; }
        Transform SelfTrs{get;}
    }
}