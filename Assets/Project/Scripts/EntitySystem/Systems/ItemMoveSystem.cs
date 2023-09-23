using Project.Scripts.EntitySystem.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Systems
{
    [DisableAutoCreation]
    public partial class ItemMoveSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            
            var separatorQuery = SystemAPI.QueryBuilder().WithAll<ItemDataComponent>().Build();
            if (separatorQuery.IsEmpty) return;

            Entities.ForEach((ref ItemDataComponent itemDataComponent, ref LocalTransform transform) => {
                
                if(itemDataComponent.Arrived) return;
                itemDataComponent.Progress += deltaTime;
                if (itemDataComponent.Progress >= 1)
                {
                    itemDataComponent.Arrived = true;
                    itemDataComponent.Progress = 0;
                    transform.Position = itemDataComponent.DestinationPos;
                    return;
                }
                
                transform.Position = itemDataComponent.PreviousPos + itemDataComponent.Progress *
                    (itemDataComponent.DestinationPos - itemDataComponent.PreviousPos);
                
            }).ScheduleParallel();
        }
    }
}
