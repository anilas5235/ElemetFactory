using Project.Scripts.EntitySystem.Components;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class PrefapsDataMono : MonoBehaviour
    {
        public GameObject Excavator, Conveyor, Separator, Combiner, TrashCan, ItemGas, ItemLiquid, ItemSolid,Tile;
    }

    public class PrefapsDataBaker : Baker<PrefapsDataMono>
    {
        public override void Bake(PrefapsDataMono authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new PrefapsDataComponent(
            
                entity,
                GetEntity(authoring.Excavator, TransformUsageFlags.Dynamic),
                GetEntity(authoring.Separator, TransformUsageFlags.Dynamic),
                GetEntity(authoring.Conveyor, TransformUsageFlags.Dynamic),
                GetEntity(authoring.Combiner, TransformUsageFlags.Dynamic),
                GetEntity(authoring.TrashCan, TransformUsageFlags.Dynamic),
                GetEntity(authoring.ItemGas, TransformUsageFlags.Dynamic),
                GetEntity(authoring.ItemLiquid, TransformUsageFlags.Dynamic),
                GetEntity(authoring.ItemSolid, TransformUsageFlags.Dynamic),
                GetEntity(authoring.Tile, TransformUsageFlags.Dynamic)
            ));
        }
    }
}
