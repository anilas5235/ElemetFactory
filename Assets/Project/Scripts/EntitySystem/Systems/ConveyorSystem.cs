using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Transmission;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Systems
{
    [DisableAutoCreation]
    public partial class ConveyorSystem : SystemBase
    {
        private static float timeForNextTick = 0;
        public static float Rate = 2;
        protected override void OnUpdate()
        {
            timeForNextTick += Time.DeltaTime;
            if(timeForNextTick < 1f/Rate) return;
            timeForNextTick = 0;
        
            Entities.ForEach((ref DynamicBuffer<InputDataComponent> inputs, ref DynamicBuffer<OutputDataComponent> outputs, in ConveyorTickDataComponent conveyorTick) => {
                
            }).Schedule();
        }
    }
}
