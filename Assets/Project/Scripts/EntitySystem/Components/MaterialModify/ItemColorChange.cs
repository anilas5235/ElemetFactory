using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Components.MaterialModify
{
    [GenerateAuthoringComponent]
    [MaterialProperty("_ContentColor",MaterialPropertyFormat.Float4)]
    public struct ItemColorChange : IComponentData
    {
        public float4 Value;
    }
}
