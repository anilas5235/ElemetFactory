using Project.Scripts.EntitySystem.Components.MaterialModify;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Systems
{
    public partial class MoveSin : SystemBase
    {
        protected override void OnUpdate()
        {
            float time = (float)Time.ElapsedTime;
            float deltaTime = Time.DeltaTime;

            Entities.ForEach((ref Translation translation,in ItemColorChange itemColorChange) => {
                translation.Value += new float3(0,math.sin(time),0) * deltaTime;
            }).ScheduleParallel();
        }
    }
}
