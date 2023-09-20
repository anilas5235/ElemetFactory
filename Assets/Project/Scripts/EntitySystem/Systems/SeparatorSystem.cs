using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Systems
{
    public partial class SeparatorSystem : SystemBase
    {
        private static float timeForNextTick = 0;
        private static EntityManager _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        protected override void OnUpdate()
        {
            // Assign values to local variables captured in your job here, so that it has
            // everything it needs to do its work when it runs later.
            // For example,
            //     float deltaTime = Time.DeltaTime;

            // This declares a new kind of job, which is a unit of work to do.
            // The job is declared as an Entities.ForEach with the target components as parameters,
            // meaning it will process all entities in the world that have both
            // Translation and Rotation components. Change it to process the component
            // types you want.
        
            if(timeForNextTick > Time.ElapsedTime) return;
            timeForNextTick += 1f/SeparatorTickDataComponent.Rate;
            
            Entities.ForEach((ref DynamicBuffer<InputDataComponent> inputs, ref DynamicBuffer<OutputDataComponent> outputs, in CombinerTickDataComponent comTick) =>
            {
                InputDataComponent input = inputs[0];
                OutputDataComponent output1 = outputs[0], output2 = outputs[1];
                if (!input.IsOccupied && output1.IsOccupied && output2.IsOccupied) return;
                
                Item itemA = ItemMemory.ItemDataBank[_entityManager.GetComponentData<ItemDataComponent>(input.SlotContent).ItemID];
                int itemLength = itemA.ResourceIDs.Length;
                int item1Length = Mathf.CeilToInt(itemLength / 2f), item2Length = itemLength - item1Length;
                
                int[] contentItem1= new int[item1Length], contentItem2=new int[item2Length];
                for (int i = 0; i < itemLength; i++)
                {
                    if (i < item1Length) contentItem1[i] = itemA.ResourceIDs[i];
                    else contentItem2[i-item1Length] = itemA.ResourceIDs[i];
                }
                input.SlotContent = default;
                
                output1.SlotContent = BuildingGridEntityUtilities.CreateItemEntity(output1.Position,ResourcesUtility.CreateItemData(contentItem1));
                output2.SlotContent = BuildingGridEntityUtilities.CreateItemEntity(output2.Position,ResourcesUtility.CreateItemData(contentItem2));
            }).Schedule();
        }
    }
}
