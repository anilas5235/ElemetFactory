using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.ItemSystem;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class BuildingMono : MonoBehaviour
    {
        public class BuildingBaker : Baker<BuildingMono>
        {
            public override void Bake(BuildingMono authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BuildingDataComponent());
                
                AddBuffer<InputSlot>(entity);
                AddBuffer<OutputSlot>(entity);
            }
        }
    }
}