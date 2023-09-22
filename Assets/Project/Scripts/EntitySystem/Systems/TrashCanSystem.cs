using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Transmission;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Systems
{
    [DisableAutoCreation]
    public partial class TrashCanSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref DynamicBuffer<InputDataComponent> inputs, in TrashCanTickDataComponent trashTick) => {
                
            }).Schedule();
        }
    }
}
