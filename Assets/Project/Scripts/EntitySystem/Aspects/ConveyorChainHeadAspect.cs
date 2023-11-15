using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ConveyorChainHeadAspect : IAspect
    {
        public readonly Entity entity;
        public readonly RefRW<ChainPushStartPoint> chainPushStart;
    }
}