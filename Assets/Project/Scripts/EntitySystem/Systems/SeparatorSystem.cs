using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Systems
{
    [DisableAutoCreation]
    public partial struct SeparatorSystem : ISystem
    {
        private static float timeSinceLastTick;
        public static float Rate;

        [BurstCompatible]
        public void OnCreate(ref SystemState state)
        {
            timeSinceLastTick = 0;
            Rate = 1;
        }

        [BurstCompatible]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompatible]
        public void OnUpdate(ref SystemState state)
        {
            timeSinceLastTick += Time.deltaTime;
            if (timeSinceLastTick < 1f / Rate) return;
            timeSinceLastTick = 0;

            EntityManager entityManager = state.EntityManager;
            state.Entities.ForEach((ref DynamicBuffer<InputDataComponent> inputs,
                ref DynamicBuffer<OutputDataComponent> outputs, in CombinerTickDataComponent comTick) =>
            {
                InputDataComponent input = inputs[0];
                OutputDataComponent output1 = outputs[0], output2 = outputs[1];
                if (!input.IsOccupied && output1.IsOccupied && output2.IsOccupied) return;
                
                if(!ItemMemory.GetItem(entityManager.GetComponentData<ItemDataComponent>(input.SlotContent).ItemID,out Item itemA)) return;
                
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
                    ResourcesUtility.CreateItemData(contentItem1),entityManager,out output1.SlotContent);
                BuildingGridEntityUtilities.CreateItemEntity(output2.Position,
                    ResourcesUtility.CreateItemData(contentItem2),entityManager,out output2.SlotContent);

                contentItem1.Dispose();
                contentItem2.Dispose();
            }).Schedule();
        }
    }
}
