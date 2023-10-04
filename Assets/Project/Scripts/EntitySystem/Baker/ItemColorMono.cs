using Project.Scripts.EntitySystem.Components.MaterialModify;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class ItemColorMono : MonoBehaviour
    {
    }
    
    public class ItemColorDataBaker : Baker<ItemColorMono>
    {
        public override void Bake(ItemColorMono authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity,new ItemColor());
        }
    }
}