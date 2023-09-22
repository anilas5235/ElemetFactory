using Project.Scripts.EntitySystem.Components;
using Unity.Entities;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Systems
{
    [DisableAutoCreation]
    public partial class ItemMoveSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;
        
            Entities.ForEach((ref ItemDataComponent itemDataComponent, ref Translation translation) => {
                if(itemDataComponent.Arrived) return;
                itemDataComponent.Progress += deltaTime;
                if (itemDataComponent.Progress >= 1)
                {
                    itemDataComponent.Arrived = true;
                    itemDataComponent.Progress = 0;
                    translation.Value = itemDataComponent.DestinationPos;
                    return;
                }
                translation.Value = itemDataComponent.PreviousPos + itemDataComponent.Progress *
                    (itemDataComponent.DestinationPos - itemDataComponent.PreviousPos);
            }).ScheduleParallel();
        }
    }
}
