using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Item
{
    public struct ItemEntityStateDataComponent : IComponentData
    {
        public float3 DestinationPos;
        public float3 PreviousPos;
        
        public bool Arrived;
        public float Progress;
    }
}
