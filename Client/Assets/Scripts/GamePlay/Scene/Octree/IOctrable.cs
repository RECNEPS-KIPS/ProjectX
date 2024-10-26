// author:KIPKIPS
// date:2024.10.25 21:07
// describe:
using UnityEngine;

namespace GamePlay.Scene
{
    public interface IOctrable
    {
        Transform ColliderTrs { get; }
        Transform SelfTrs{get;}
        
        Collider Collider { get; }
    }
}