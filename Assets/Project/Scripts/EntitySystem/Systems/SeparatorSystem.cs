using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Systems
{
    public partial class SeparatorSystem : SystemBase
    {
        private static float timeForNextTick = 0;
        public static float Rate = .25f;

        protected override void OnUpdate()
        {
            if (timeForNextTick > Time.ElapsedTime) return;
            timeForNextTick += 1f / Rate;

            Entities.ForEach((ref DynamicBuffer<InputDataComponent> inputs,
                ref DynamicBuffer<OutputDataComponent> outputs, in CombinerTickDataComponent comTick) =>
            {
                InputDataComponent input = inputs[0];
                OutputDataComponent output1 = outputs[0], output2 = outputs[1];
                if (!input.IsOccupied && output1.IsOccupied && output2.IsOccupied) return;
                
                Item itemA =
                    ItemMemory.ItemDataBank[
                        EntityManager.GetComponentData<ItemDataComponent>(input.SlotContent).ItemID];
                int itemLength = itemA.ResourceIDs.Length;
                int item1Length = Mathf.CeilToInt(itemLength / 2f), item2Length = itemLength - item1Length;

                int[] contentItem1 = new int[item1Length], contentItem2 = new int[item2Length];
                for (int i = 0; i < itemLength; i++)
                {
                    if (i < item1Length) contentItem1[i] = itemA.ResourceIDs[i];
                    else contentItem2[i - item1Length] = itemA.ResourceIDs[i];
                }

                input.SlotContent = default;

                output1.SlotContent = BuildingGridEntityUtilities.CreateItemEntity(output1.Position,
                    ResourcesUtility.CreateItemData(contentItem1),EntityManager);
                output2.SlotContent = BuildingGridEntityUtilities.CreateItemEntity(output2.Position,
                    ResourcesUtility.CreateItemData(contentItem2),EntityManager);
            }).Schedule();
        }
    }
}
