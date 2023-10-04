using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace Project.Scripts.EntitySystem.Components.MaterialModify
{
    [MaterialProperty("_BaseColor")]
    public struct TileColor : IComponentData
    {
        public float4 Value;
    }
}