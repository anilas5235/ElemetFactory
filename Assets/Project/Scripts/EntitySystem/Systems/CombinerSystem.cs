using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Systems
{
    public partial class CombinerSystem : SystemBase
    {
        private static float timeForNextTick = 0;
        private static EntityManager _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    
        protected override void OnUpdate()
        {
            if(timeForNextTick > Time.ElapsedTime) return;
            timeForNextTick += 1f/CombinerTickDataComponent.Rate;
        
            Entities.ForEach((ref DynamicBuffer<InputDataComponent> inputs, ref DynamicBuffer<OutputDataComponent> outputs, in CombinerTickDataComponent comTick) =>
            {
                InputDataComponent input1 = inputs[0], input2 = inputs[1];
                OutputDataComponent output = outputs[0];
                if (input1.IsOccupied || input2.IsOccupied || !output.IsOccupied)
                {
                    Item itemA = ItemMemory.ItemDataBank[_entityManager.GetComponentData<ItemDataComponent>(input1.SlotContent).ItemID];
                    Item itemB = ItemMemory.ItemDataBank[_entityManager.GetComponentData<ItemDataComponent>(input2.SlotContent).ItemID];
                    
                    int[] combIDs = new int[itemA.ResourceIDs.Length + itemB.ResourceIDs.Length];
                    itemA.ResourceIDs.CopyTo(combIDs, 0);
                    input1.SlotContent = default;
                    itemB.ResourceIDs.CopyTo(combIDs, itemA.ResourceIDs.Length);
                    input2.SlotContent = default;

                    output.SlotContent = BuildingGridEntityUtilities.CreateItemEntity();
                }
            }).Schedule();
        }
    }
}
