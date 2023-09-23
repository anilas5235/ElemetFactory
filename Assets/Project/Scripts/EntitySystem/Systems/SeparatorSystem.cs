using System.Runtime.InteropServices;
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
            timeSinceLastTick = 0;
            Rate = 1;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            timeSinceLastTick += Time.deltaTime;
            if (timeSinceLastTick < 1f / Rate) return;
            timeSinceLastTick = 0;
            
            var separatorQuery = SystemAPI.QueryBuilder().WithAll<SeparatorDataComponent>().Build();
            if (separatorQuery.IsEmpty) return;

            foreach (Entity entity in separatorQuery.ToEntityArray(Allocator.Temp))
            {
                SeparatorDataComponent separatorDataComponent = SystemAPI.GetComponent<SeparatorDataComponent>(entity);
                InputSlot input = separatorDataComponent.input;
                OutputSlot output1 = separatorDataComponent.output1, output2 = separatorDataComponent.output2;
                if (!input.IsOccupied && output1.IsOccupied && output2.IsOccupied) return;
            
                if(!ItemMemory.GetItem(SystemAPI.GetComponent<ItemDataComponent>(input.SlotContent).ItemID,out Item itemA)) return;
                
                int itemLength = itemA.ResourceIDs.Length;
                int item1Length = Mathf.CeilToInt(itemLength / 2f), item2Length = itemLength - item1Length;

                NativeArray<uint> contentItem1 = new (item1Length,Allocator.TempJob), contentItem2 = new (item2Length,Allocator.TempJob);
                for (int i = 0; i < itemLength; i++)
                {
                    if (i < item1Length) contentItem1[i] = itemA.ResourceIDs[i];
                    else contentItem2[i - item1Length] = itemA.ResourceIDs[i];
                }

                input.SlotContent = default;

                BuildingGridEntityUtilities.CreateItemEntity(output1.Position,
                    ResourcesUtility.CreateItemData(contentItem1),out output1.SlotContent);
                BuildingGridEntityUtilities.CreateItemEntity(output2.Position,
                    ResourcesUtility.CreateItemData(contentItem2),out output2.SlotContent);

                contentItem1.Dispose();
                contentItem2.Dispose();
            }
        }
    }
}
