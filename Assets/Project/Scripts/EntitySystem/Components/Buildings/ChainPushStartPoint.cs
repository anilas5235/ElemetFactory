using Project.Scripts.EntitySystem.Aspects;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Buildings
{
    public struct ChainPushStartPoint : IComponentData
    {
        [NativeDisableContainerSafetyRestriction] public NativeList<BuildingAspect> Chain;
    }
}