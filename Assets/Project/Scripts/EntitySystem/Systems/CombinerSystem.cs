using Unity.Burst;
using Unity.Entities;
using UnityEngine;


namespace Project.Scripts.EntitySystem.Systems
{
    [DisableAutoCreation]
    [BurstCompile]
    public partial struct CombinerSystem : ISystem
    {
        private static float timeSinceLastTick;
        public static float Rate;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            timeSinceLastTick = 0;
            Rate= .25f;
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            timeSinceLastTick += Time.deltaTime;
            if (timeSinceLastTick < 1f / Rate) return;
            timeSinceLastTick = 0;
            
           
            /*
            state.Entities.WithAll<CombinerDataComponent>()
                .ForEach(
                (ref DynamicBuffer<InputDataComponent> inputs, ref DynamicBuffer<OutputDataComponent> outputs) =>
                {
                    InputDataComponent input1 = inputs[0],input2 = inputs[1];
                    OutputDataComponent output = outputs[0];
                    
                    if (!input1.IsOccupied && !input2.IsOccupied && output.IsOccupied) return;

                    if(!ItemMemory.GetItem(entityManager.GetComponentData<ItemDataComponent>(input1.SlotContent).ItemID,out Item itemA)) return;
                    if(!ItemMemory.GetItem(entityManager.GetComponentData<ItemDataComponent>(input1.SlotContent).ItemID,out Item itemB)) return;

                    NativeArray<uint> combIDs = new NativeArray<uint>(itemA.ResourceIDs.Length + itemB.ResourceIDs.Length, Allocator.TempJob);
                    for (int i = 0; i < itemA.ResourceIDs.Length; i++)combIDs[i] = itemA.ResourceIDs[i];
                    input1.SlotContent = default;
                    for (int i = 0; i < itemB.ResourceIDs.Length; i++)combIDs[i+itemA.ResourceIDs.Length] = itemA.ResourceIDs[i];
                    input2.SlotContent = default;

                    BuildingGridEntityUtilities.CreateItemEntity(output.Position, ResourcesUtility.CreateItemData(combIDs), entityManager, out output.SlotContent );
                    
                    combIDs.Dispose();

                }).Schedule();*/
        }
    }
}
