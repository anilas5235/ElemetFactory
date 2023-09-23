using System;
using System.Runtime.InteropServices;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Systems
{
    [StructLayout(LayoutKind.Auto)]
    [BurstCompile]
    public partial struct ExtractorSystem : ISystem
    {
        private float timeSinceLastTick;
        public float Rate;

        public Item[] existingItems;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            timeSinceLastTick = 0;
            Rate = 1;
            state.RequireForUpdate<PrefapsDataComponent>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            timeSinceLastTick += Time.deltaTime;
            if (timeSinceLastTick < 1f / Rate) return;
            timeSinceLastTick = 0;
            
            EntityQuery separatorQuery = SystemAPI.QueryBuilder().WithAll<ExtractorAspect>().Build();
            if (separatorQuery.IsEmpty) return;

            PrefapsDataComponent prefaps = SystemAPI.GetSingleton<PrefapsDataComponent>();
            
            EntityCommandBuffer ecb = new EntityCommandBuffer( Allocator.Temp);

            NativeArray<Item> existingItemsTemp = new NativeArray<Item>(existingItems,Allocator.Temp);

            NativeArray<Entity> entities = separatorQuery.ToEntityArray(Allocator.Temp);

            foreach (Entity entity in entities)
            {
                var aspect = SystemAPI.GetAspect<ExtractorAspect>(entity);

                if (aspect.Output.IsOccupied) continue;
                
                Item item = existingItemsTemp[(int)aspect.ItemID];

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
                    DestinationPos = aspect.Location,
                    PreviousPos = aspect.Location,
                    ItemID = aspect.ItemID,
                });
                ecb.SetComponent(itemEntity, new LocalTransform()
                {
                    Position = aspect.Location,
                    Scale = 10,
                });
            }
            
            ecb.Playback(state.EntityManager);
        }
    }
}
