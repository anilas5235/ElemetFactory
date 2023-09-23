using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Systems
{
    [DisableAutoCreation]
    public partial class TrashCanSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(( ref TrashCanDataComponent trash) => {
                
            }).Schedule();
        }
    }
}
