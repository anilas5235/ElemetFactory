using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class BuildingDataMono : MonoBehaviour
    {
    }
    
    public class BuildingDataBaker : Baker<BuildingDataMono>
    {
        public override void Bake(BuildingDataMono authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity,new BuildingDataComponent());
        }
    }
}