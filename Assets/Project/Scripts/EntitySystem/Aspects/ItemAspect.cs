using System;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Project.Scripts.ItemSystem;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ItemAspect : IAspect
    {
        public readonly Entity entity;

        public readonly RefRW<LocalTransform> transform;

        public readonly RefRW<ItemDataComponent> dataComponent;

        public readonly RefRW<ItemColor> color;

        public Item Item => dataComponent.ValueRO.item;

        public static Entity CreateItemEntity(Item item, EntityCommandBuffer ecb,float3 location,PrefapsDataComponent prefaps)
        {
            Entity itemEntity = item.ItemForm switch
            {
                ItemForm.Gas => ecb.Instantiate(prefaps.ItemGas),
                ItemForm.Fluid => ecb.Instantiate(prefaps.ItemLiquid),
                ItemForm.Solid => ecb.Instantiate(prefaps.ItemSolid),
                _ => throw new ArgumentOutOfRangeException()
            };
                    
            ecb.SetComponent(itemEntity,new ItemColor(){Value = item.Color});
            ecb.SetComponent(itemEntity, new ItemDataComponent()
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