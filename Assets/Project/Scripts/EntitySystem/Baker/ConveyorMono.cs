using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class ConveyorMono : MonoBehaviour
    {
        public class ConveyorDataBaker : Baker<ConveyorMono>
        {
            public override void Bake(ConveyorMono authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ConveyorDataComponent());
            }
        }
    }
}
