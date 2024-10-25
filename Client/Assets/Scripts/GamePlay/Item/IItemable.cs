using System;
using Framework.Core.Manager.ResourcesLoad;
using Unity.VisualScripting;
using UnityEngine;

namespace GamePlay.Item
{
    public interface IItemable
    {
        int ID { get; set; }
        ItemConfig ItemConfig { get; }
    }
}