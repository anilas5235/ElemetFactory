using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Transmission;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Systems
{
    public partial class ConveyorSystem : SystemBase
    {
        private static float timeForNextTick = 0;
        private static EntityManager _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        protected override void OnUpdate()
        {
            if(timeForNextTick > Time.ElapsedTime) return;
            timeForNextTick += 1f/ConveyorTickDataComponent.Rate;
        
            Entities.ForEach((ref DynamicBuffer<InputDataComponent> inputs, ref DynamicBuffer<OutputDataComponent> outputs, in ConveyorTickDataComponent conveyorTick) => {
                
            }).Schedule();
        }
    }
}
