using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Item;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class ItemDataMono : MonoBehaviour
    {
        private class ItemDataMonoBaker : Baker<ItemDataMono>
        {
            public override void Bake(ItemDataMono authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,new ItemDataComponent());
            }
        }
    }
}