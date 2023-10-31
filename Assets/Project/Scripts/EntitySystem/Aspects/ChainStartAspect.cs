using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ChainStartAspect : IAspect
    {
        public readonly Entity entity;
        public readonly RefRO<ChainPullStartPointTag> tag;
        public readonly BuildingAspect buildingAspect;
    }
}