using Project.Scripts.EntitySystem.Components;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class PrefabsDataMono : MonoBehaviour
    {
        public GameObject
            ItemGas,
            ItemLiquid,
            ItemSolid,
            Tile,
            GasResource,
            LiquidResource,
            SolidResource,
            Extractor,
            Conveyor,
            Combiner,
            Separator,
            TrashCan;
    }

    public class PrefabsDataBaker : Baker<PrefabsDataMono>
    {
        public override void Bake(PrefabsDataMono authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PrefabsDataComponent(
                entity,
                GetEntity(authoring.ItemGas, TransformUsageFlags.Dynamic),
                GetEntity(authoring.ItemLiquid, TransformUsageFlags.Dynamic),
                GetEntity(authoring.ItemSolid, TransformUsageFlags.Dynamic),
                GetEntity(authoring.Tile, TransformUsageFlags.Dynamic),
                GetEntity(authoring.GasResource, TransformUsageFlags.Dynamic),
                GetEntity(authoring.LiquidResource, TransformUsageFlags.Dynamic),
                GetEntity(authoring.SolidResource, TransformUsageFlags.Dynamic),
                GetEntity(authoring.Extractor, TransformUsageFlags.Dynamic),
                GetEntity(authoring.Conveyor, TransformUsageFlags.Dynamic),
                GetEntity(authoring.Combiner, TransformUsageFlags.Dynamic),
                GetEntity(authoring.Separator, TransformUsageFlags.Dynamic),
                GetEntity(authoring.TrashCan, TransformUsageFlags.Dynamic)
            ));
        }
    }
}
