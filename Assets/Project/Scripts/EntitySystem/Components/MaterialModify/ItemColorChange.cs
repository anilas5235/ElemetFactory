using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Components.MaterialModify
{    
    [MaterialProperty("_ContentColor")]
    public struct ItemColor : IComponentData
    {
        public float4 Value;
    }
}
