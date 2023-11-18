using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.DataObject;
using Project.Scripts.EntitySystem.Systems;
using Unity.Collections;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ConveyorChainHeadAspect : IAspect
    {
        public readonly Entity Entity;
        public readonly DynamicBuffer<ConveyorChainDataPoint> ChainBuffer;
        public readonly RefRW<ConveyorChainDataComponent> chainDataComponent;

        public void RemoveConveyor(Entity conveyorEntity)
        {
            using var firstChain = new NativeList<ConveyorChainDataPoint>(Allocator.Temp);
            using var secondChain = new NativeList<ConveyorChainDataPoint>(Allocator.Temp);
            bool forFirst = true;
            for (int i = 0; i < ChainBuffer.Length; i++)
            {
                var entity = ChainBuffer[i].ConveyorEntity;
                if (entity == conveyorEntity)
                {
                    forFirst = false;
                    continue;
                }
                if(forFirst) firstChain.Add(new ConveyorChainDataPoint()
                {
                    ConveyorEntity = entity
                });
                else secondChain.Add(new ConveyorChainDataPoint()
                {
                    ConveyorEntity = entity
                });
            }
            
            var ecb = PlacingSystem.beginSimulationEntityCommandBuffer.CreateCommandBuffer(
                World.DefaultGameObjectInjectionWorld.Unmanaged);

            if (firstChain.Length > 0)
            {
                var headEntity = ChunkDataAspect.CreateChainHead(ecb, out var buffer);
                buffer.AddRange(firstChain);
                foreach (ConveyorChainDataPoint conveyor in buffer)
                {
                    ecb.SetComponent(conveyor.ConveyorEntity, new ConveyorDataComponent()
                    {
                        head = headEntity,
                    });
                }
            }

            if (secondChain.Length > 0)
            {
                var headEntity = ChunkDataAspect.CreateChainHead(ecb, out var buffer);
                buffer.AddRange(secondChain);
                foreach (ConveyorChainDataPoint conveyor in buffer)
                {
                    ecb.SetComponent(conveyor.ConveyorEntity, new ConveyorDataComponent()
                    {
                        head = headEntity,
                    });
                }
            }
            ecb.DestroyEntity(Entity);
        }
    }
}