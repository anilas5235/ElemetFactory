using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Transmission;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Systems
{
    public partial class ExtractorSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(( ref DynamicBuffer<OutputDataComponent> outputs, in ExtractorTickDataComponent extractorTick) => {
                
            }).Schedule();
        }
    }
}
