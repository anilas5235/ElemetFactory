using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Entities;
using UnityEditorInternal.Profiling.Memory.Experimental;

namespace Project.Scripts.EntitySystem.Systems
{
    public partial class CombinerSystem : SystemBase
    {
        private static float timeForNextTick = 0;
        public static float Rate = .25f;
    
        protected override void OnUpdate()
        {
            if(timeForNextTick > Time.ElapsedTime) return;
            timeForNextTick += 1f/Rate;
        
            Entities.ForEach((ref DynamicBuffer<InputDataComponent> inputs, ref DynamicBuffer<OutputDataComponent> outputs, in CombinerTickDataComponent comTick) =>
            {
                InputDataComponent input1 = inputs[0], input2 = inputs[1];
                OutputDataComponent output = outputs[0];
                if (!input1.IsOccupied && !input2.IsOccupied && output.IsOccupied) return;
                
                Item itemA = ItemMemory.ItemDataBank[EntityManager.GetComponentData<ItemDataComponent>(input1.SlotContent).ItemID];
                Item itemB = ItemMemory.ItemDataBank[EntityManager.GetComponentData<ItemDataComponent>(input2.SlotContent).ItemID];
                    
                int[] combIDs = new int[itemA.ResourceIDs.Length + itemB.ResourceIDs.Length];
                itemA.ResourceIDs.CopyTo(combIDs, 0);
                input1.SlotContent = default;
                itemB.ResourceIDs.CopyTo(combIDs, itemA.ResourceIDs.Length);
                input2.SlotContent = default;

                output.SlotContent = BuildingGridEntityUtilities.CreateItemEntity(output.Position,ResourcesUtility.CreateItemData(combIDs),EntityManager);
            }).Schedule();
        }
    }
}
