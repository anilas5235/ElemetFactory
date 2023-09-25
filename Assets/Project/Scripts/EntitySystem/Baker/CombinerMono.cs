using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class CombinerMono : MonoBehaviour
    {
    }
    
    public class CombinerDataBaker : Baker<CombinerMono>
    {
        public override void Bake(CombinerMono authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity,new CombinerDataComponent());
        }
    }
}
