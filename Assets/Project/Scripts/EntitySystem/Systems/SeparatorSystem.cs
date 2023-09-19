using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.ItemSystem;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Systems
{
    public partial class SeparatorSystem : SystemBase
    {
        private static float timeForNextTick = 0;
        private static EntityManager _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        protected override void OnUpdate()
        {
            // Assign values to local variables captured in your job here, so that it has
            // everything it needs to do its work when it runs later.
            // For example,
            //     float deltaTime = Time.DeltaTime;

            // This declares a new kind of job, which is a unit of work to do.
            // The job is declared as an Entities.ForEach with the target components as parameters,
            // meaning it will process all entities in the world that have both
            // Translation and Rotation components. Change it to process the component
            // types you want.
        
            if(timeForNextTick > Time.ElapsedTime) return;
            timeForNextTick += 1f/SeparatorTickDataComponent.Rate;
        
            Entities.ForEach((ref Translation translation, in Rotation rotation) => {
                // Implement the work to perform for each entity here.
                // You should only access data that is local or that is a
                // field on this job. Note that the 'rotation' parameter is
                // marked as 'in', which means it cannot be modified,
                // but allows this job to run in parallel with other jobs
                // that want to read Rotation component data.
                // For example,
                //     translation.Value += math.mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
                int itemLength = inputs[0].SlotContent.Item.ResourceIDs.Length;
                if(itemLength < 1) break;
                int item1Length = math.CeilToInt(itemLength / 2f), item2Length = itemLength - item1Length;
                ItemContainer item = inputs[0].EmptySlot();
                int[] contentItem1= new int[item1Length], contentItem2=new int[item2Length];
                for (int i = 0; i < itemLength; i++)
                {
                    if (i < item1Length) contentItem1[i] = item.Item.ResourceIDs[i];
                    else contentItem2[i-item1Length] = item.Item.ResourceIDs[i];
                }
                item.Destroy();

                outputs[0].FillSlot(ItemUtility.GetItemContainerWith(new Item(contentItem1),outputs[0]));
                outputs[1].FillSlot(ItemUtility.GetItemContainerWith(new Item(contentItem2),outputs[1]));
            }).Schedule();
        }
    }
}
