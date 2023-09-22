using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.Grid;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Systems
{
    public partial struct ExtractorSystem : ISystem
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

            EntityManager  entityManager = state.EntityManager;

            state.Entities.ForEach((ref DynamicBuffer<OutputDataComponent> outputs,
                in ExtractorTickDataComponent extractorTick) =>
            {
                OutputDataComponent output = outputs[0];
                if (!output.IsOccupied)
                {
                    BuildingGridEntityUtilities.CreateItemEntity(output.Position, extractorTick.itemID, entityManager,
                        out output.SlotContent);
                }

            }).Run();
        }
    }
}
