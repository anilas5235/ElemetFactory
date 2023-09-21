using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEditorInternal.Profiling.Memory.Experimental;

namespace Project.Scripts.EntitySystem.Systems
{
    [AlwaysUpdateSystem]
    public partial class CombinerSystem : SystemBase
    {
        private static float timeSinceLastTick;
        public static float Rate;

        protected override void OnCreate()
        {
            timeSinceLastTick = 0;
            Rate= .25f;
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            timeSinceLastTick += Time.DeltaTime;
            if (timeSinceLastTick < 1f / Rate) return;
            timeSinceLastTick = 0;

            Entities.WithAll<CombinerTickDataComponent>().ForEach(
                (ref DynamicBuffer<InputDataComponent> inputs, ref DynamicBuffer<OutputDataComponent> outputs) =>
                {
                    InputDataComponent input1 = inputs[0],input2 = inputs[1];
                    OutputDataComponent output = outputs[0];
                    
                    if (!input1.IsOccupied && !input2.IsOccupied && output.IsOccupied) return;
                    
                    Item itemA = ItemMemory.ItemDataBank[
                        EntityManager.GetComponentData<ItemDataComponent>(input1.SlotContent).ItemID];
                    Item itemB = ItemMemory.ItemDataBank[
                        EntityManager.GetComponentData<ItemDataComponent>(input2.SlotContent).ItemID];

                    NativeArray<int> combIDs = new NativeArray<int>(itemA.ResourceIDs.Length + itemB.ResourceIDs.Length, Allocator.TempJob);
                    for (int i = 0; i < itemA.ResourceIDs.Length; i++)combIDs[i] = itemA.ResourceIDs[i];
                    input1.SlotContent = default;
                    for (int i = 0; i < itemB.ResourceIDs.Length; i++)combIDs[i+itemA.ResourceIDs.Length] = itemA.ResourceIDs[i];
                    input2.SlotContent = default;

                    output.SlotContent = BuildingGridEntityUtilities.CreateItemEntity(output.Position,
                        ResourcesUtility.CreateItemData(combIDs.ToArray()), EntityManager);
                    
                    combIDs.Dispose();

                }).Schedule();
        }
    }
}
