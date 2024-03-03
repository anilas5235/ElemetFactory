using System;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Item;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Project.Scripts.ItemSystem;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ItemEntityAspect : IAspect
    {
        public readonly Entity entity;

        public readonly RefRW<LocalTransform> transform;

        public readonly RefRW<ItemEntityStateDataComponent> dataComponent;

        public static Entity CreateItemEntity(Item item, EntityCommandBuffer ecb,float3 location,PrefabsDataComponent prefabs)
        {
            var itemEntity = ecb.Instantiate(prefabs.ItemSolid); //TODO: new system for item prefabs
            
            ecb.SetComponent(itemEntity, new ItemEntityStateDataComponent()
            {
                Arrived = true,
                DestinationPos = location,
                PreviousPos = location,
                    
            });
            ecb.SetComponent(itemEntity, new LocalTransform()
            {
                Position = location,
                Scale = 10,
            });
            return itemEntity;
        }
    }
}
