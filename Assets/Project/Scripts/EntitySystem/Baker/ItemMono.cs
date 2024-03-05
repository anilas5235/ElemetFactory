using Project.Scripts.EntitySystem.Components.Item;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class ItemMono : MonoBehaviour
    {
        private class ItemDataMonoBaker : Baker<ItemMono>
        {
            public override void Bake(ItemMono authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,new ItemDataComponent());
                AddComponent(entity,new ItemEntityStateDataComponent());
            }
        }
    }
}