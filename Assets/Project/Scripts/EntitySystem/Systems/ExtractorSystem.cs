using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Systems
{
    public partial struct ExtractorSystem : ISystem
    {
        private static float timeSinceLastTick;
        public static float Rate;

        [GenerateTestsForBurstCompatibility]
        public void OnCreate(ref SystemState state)
        {
            timeSinceLastTick = 0;
            Rate = 1;
        }

        [GenerateTestsForBurstCompatibility]
        public void OnDestroy(ref SystemState state)
        {
        }

        [GenerateTestsForBurstCompatibility]
        public void OnUpdate(ref SystemState state)
        {
            timeSinceLastTick += Time.deltaTime;
            if (timeSinceLastTick < 1f / Rate) return;
            timeSinceLastTick = 0;
            
            /*var separatorQuery = SystemAPI.QueryBuilder().WithAll<ExtractorDataComponent>().Build();
            if (separatorQuery.IsEmpty) return;

            foreach (Entity entity in separatorQuery.ToEntityArray(Allocator.Temp))
            {
                 OutputDataComponent output = outputs[0];
                                if (!output.IsOccupied)
                                {
                                    BuildingGridEntityUtilities.CreateItemEntity(output.Position, extractorTick.itemID, out output.SlotContent); }
            }*/
        }
    }
}
