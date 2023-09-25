using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class TrashCanMono : MonoBehaviour
    {
    }
    
    public class TrashCanDataBaker : Baker<TrashCanMono>
    {
        public override void Bake(TrashCanMono authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity,new TrashCanDataComponent());
        }
    }
}
