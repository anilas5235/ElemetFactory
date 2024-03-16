using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace Project.Scripts.EntitySystem.Components.MaterialModify
{
    [MaterialProperty("_Position")]
    public struct AtlasModifier : IComponentData
    {
        public float2 Value;
    }
}