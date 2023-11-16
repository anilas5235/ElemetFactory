using Project.Scripts.EntitySystem.Aspects;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Buffer
{
    public struct ConveyorChainDataPoint : IBufferElementData
    {
        public ConveyorAspect ConveyorAspect;
    }
}