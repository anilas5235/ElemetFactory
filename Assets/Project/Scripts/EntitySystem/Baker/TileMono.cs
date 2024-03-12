using Project.Scripts.EntitySystem.Components.Tags;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class TileMono : MonoBehaviour
    {
        private class TileBaker : Baker<TileMono>
        {
            public override void Bake(TileMono authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,new TileTag());
            }
        }
    }
}