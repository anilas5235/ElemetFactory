using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.Grid;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Systems
{
    public partial class ExtractorSystem : SystemBase
    {
        private static float timeForNextTick = 0;
        public static float Rate = 1;
        protected override void OnUpdate()
        {
            if(timeForNextTick > Time.ElapsedTime) return;
            timeForNextTick += 1f/Rate;
            
            Entities.ForEach(( ref DynamicBuffer<OutputDataComponent> outputs, in ExtractorTickDataComponent extractorTick) =>
            {
                OutputDataComponent output = outputs[0];
                if (!output.IsOccupied)
                {
                    
                    output.SlotContent =  BuildingGridEntityUtilities.CreateItemEntity(output.Position,extractorTick.itemID,EntityManager);
                }
            }).Schedule();
        }
    }
}
