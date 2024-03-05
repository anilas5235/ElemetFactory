using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class SeparatorMono : MonoBehaviour
    {
        public class SeparatorDataBaker : Baker<SeparatorMono>
        {
            public override void Bake(SeparatorMono authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SeparatorDataComponent());
            }
        }
    }
}
