using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Systems
{
    [DisableAutoCreation]
    public partial class ConveyorSystem : SystemBase
    {
        private  float timeForNextTick = 0;
        public float Rate = 2;
        protected override void OnUpdate()
        {
            timeForNextTick += SystemAPI.Time.DeltaTime;
            if(timeForNextTick < 1f/Rate) return;
            timeForNextTick = 0;
        
            Entities.ForEach((ref ConveyorDataComponent conveyorTick) => {
                
            }).Schedule();
        }
    }
}
