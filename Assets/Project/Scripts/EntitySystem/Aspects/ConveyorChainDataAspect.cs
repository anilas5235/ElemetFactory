using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ConveyorChainDataAspect : IAspect
    {
        public readonly Entity entity;
        public readonly DynamicBuffer<ConveyorChainDataPoint> ConveyorChainData;
    }
}