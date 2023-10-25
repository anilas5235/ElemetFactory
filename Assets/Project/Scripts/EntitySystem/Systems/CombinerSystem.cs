using Project.Scripts.EntitySystem.Components;
using Unity.Burst;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Systems
{
    [BurstCompile]
    public partial struct CombinerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.AddComponentData(state.SystemHandle, new TimeRateDataComponent()
            {
                timeSinceLastTick = 0,
                Rate = .25f
            });
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var rateHandel = SystemAPI.GetComponent<TimeRateDataComponent>(state.SystemHandle);
            rateHandel.timeSinceLastTick += SystemAPI.Time.DeltaTime;
            if (rateHandel.timeSinceLastTick >= 1f / rateHandel.Rate)
            {
                
                rateHandel.timeSinceLastTick = 0;
            }
            
            SystemAPI.SetComponent(state.SystemHandle,rateHandel);
        }
    }
}
