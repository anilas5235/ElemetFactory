using System.Runtime.InteropServices;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Systems
{
    [DisableAutoCreation]
    [StructLayout(LayoutKind.Auto)]
    public partial struct SeparatorSystem : ISystem
    {
        private float timeSinceLastTick;
        public float Rate;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrefabsDataComponent>();
            timeSinceLastTick = 0;
            Rate = 1;
        }

        /*
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            timeSinceLastTick += Time.deltaTime;
            if (timeSinceLastTick < 1f / Rate) return;
            timeSinceLastTick = 0;
            
            EntityQuery separatorQuery = SystemAPI.QueryBuilder().WithAll<SeparatorDataComponent>().Build();
            if (separatorQuery.IsEmpty)
            {
                separatorQuery.Dispose();
                return;
            }
            
            PrefabsDataComponent prefabs = SystemAPI.GetSingleton<PrefabsDataComponent>();
            
            EntityCommandBuffer ecb = new EntityCommandBuffer( Allocator.Temp);

            NativeArray<Entity> entities = separatorQuery.ToEntityArray(Allocator.Temp);

            foreach (Entity entity in entities)
            {
                SeparatorAspect separatorData = SystemAPI.GetAspect<SeparatorAspect>(entity);


                OutputSlot outputSlot0 = separatorData.OutputSlot0;
                OutputSlot outputSlot1 = separatorData.OutputSlot1;
                if (!separatorData.Input.IsOccupied || outputSlot0.IsOccupied || outputSlot1.IsOccupied) return;

                Item itemA = SystemAPI.GetAspect<ItemAspect>(separatorData.ItemEntityInInput).Item;
                
                int itemLength = itemA.ResourceIDs.Length;
                int item1Length = Mathf.CeilToInt(itemLength / 2f), item2Length = itemLength - item1Length;

                NativeArray<uint> contentItem1 = new (item1Length,Allocator.TempJob), contentItem2 = new (item2Length,Allocator.TempJob);
                for (int i = 0; i < itemLength; i++)
                {
                    if (i < item1Length) contentItem1[i] = itemA.ResourceIDs[i];
                    else contentItem2[i - item1Length] = itemA.ResourceIDs[i];
                }
                
                ecb.DestroyEntity(separatorData.ItemEntityInInput);
                separatorData.ItemEntityInInput = default;

                outputSlot0.SlotContent = ItemAspect.CreateItemEntity(ResourcesUtility.CreateItemData(contentItem1), ecb, outputSlot0.Position, prefabs);
                separatorData.OutputSlot0 = outputSlot0;

                
                outputSlot1.SlotContent = ItemAspect.CreateItemEntity(ResourcesUtility.CreateItemData(contentItem2), ecb, outputSlot1.Position, prefabs);
                separatorData.OutputSlot1 = outputSlot1;

                contentItem1.Dispose();
                contentItem2.Dispose();
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            entities.Dispose();
            separatorQuery.Dispose();
        }
        */
    }
}
