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
        public readonly DynamicBuffer<EntityRefBufferElement> ChainBuffer;
        public readonly RefRW<ConveyorChainDataComponent> chainDataComponent;

        public void RemoveConveyor(Entity conveyorEntity)
        {
            using var firstChain = new NativeList<EntityRefBufferElement>(Allocator.Temp);
            using var secondChain = new NativeList<EntityRefBufferElement>(Allocator.Temp);
            bool forFirst = true;
            for (int i = 0; i < ChainBuffer.Length; i++)
            {
                var entity = ChainBuffer[i].Entity;
                if (entity == conveyorEntity)
                {
                    forFirst = false;
                    continue;
                }
                if(forFirst) firstChain.Add(new EntityRefBufferElement()
                {
                    Entity = entity
                });
                else secondChain.Add(new EntityRefBufferElement()
                {
                    Entity = entity
                });
            }
            
            var ecb = PlacingSystem.beginSimulationEntityCommandBuffer.CreateCommandBuffer(
                World.DefaultGameObjectInjectionWorld.Unmanaged);

            if (firstChain.Length > 0)
            {
                var headEntity = ChunkDataAspect.CreateChainHead(ecb, out var buffer);
                buffer.AddRange(firstChain.AsArray());
                foreach (EntityRefBufferElement conveyor in buffer)
                {
                    ecb.SetComponent(conveyor.Entity, new ConveyorDataComponent()
                    {
                        head = headEntity,
                    });
                }
            }

            if (secondChain.Length > 0)
            {
                var headEntity = ChunkDataAspect.CreateChainHead(ecb, out var buffer);
                buffer.AddRange(secondChain.AsArray());
                foreach (EntityRefBufferElement conveyor in buffer)
                {
                    ecb.SetComponent(conveyor.Entity, new ConveyorDataComponent()
                    {
                        head = headEntity,
                    });
                }
            }
            ecb.DestroyEntity(Entity);
        }
    }
}