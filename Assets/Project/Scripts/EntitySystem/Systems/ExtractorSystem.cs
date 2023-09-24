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

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            timeSinceLastTick = 0;
            Rate = 1;
            state.RequireForUpdate<PrefapsDataComponent>();
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

            NativeArray<Entity> entities = separatorQuery.ToEntityArray(Allocator.Temp);

            foreach (Entity entity in entities)
            {
                var aspect = SystemAPI.GetAspect<ExtractorAspect>(entity);

                var aspectOutput = aspect.Output;
                if (aspectOutput.IsOccupied) continue;

                aspectOutput.SlotContent = ItemAspect.CreateItemEntity(aspect.Item, ecb, aspect.Output.Position, prefaps);
                aspect.Output = aspectOutput;
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            separatorQuery.Dispose();
            entities.Dispose();
        }
    }
}
