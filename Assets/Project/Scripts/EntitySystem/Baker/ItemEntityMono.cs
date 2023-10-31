using Project.Scripts.EntitySystem.Components;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class ItemEntityMono : MonoBehaviour
    { private class ItemEntityMonoBaker : Baker<ItemEntityMono>
        {
            public override void Bake(ItemEntityMono authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,new ItemEntityStateDataComponent());
            }
        }
    }
}