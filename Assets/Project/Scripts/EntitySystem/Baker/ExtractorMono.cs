using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class ExtractorMono : MonoBehaviour
    {
    }

    public class ExtractorDataBaker : Baker<ExtractorMono>
    {
        public override void Bake(ExtractorMono authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity,new ExtractorDataComponent());
        }
    }
}
