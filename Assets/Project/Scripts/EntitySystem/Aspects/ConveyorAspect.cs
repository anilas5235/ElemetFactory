using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ConveyorAspect : IAspect
    {
        private readonly RefRW<ConveyorDataComponent> conveyorData;
    }
}
