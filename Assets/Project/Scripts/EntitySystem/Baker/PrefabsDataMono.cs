using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.Utilities;
using Unity.Collections;
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
            SolidResource;
        
        public BuildingScriptableData
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
                GetEntity(authoring.Extractor.prefab, TransformUsageFlags.Dynamic),
                GetEntity(authoring.Conveyor.prefab, TransformUsageFlags.Dynamic),
                GetEntity(authoring.Combiner.prefab, TransformUsageFlags.Dynamic),
                GetEntity(authoring.Separator.prefab, TransformUsageFlags.Dynamic),
                GetEntity(authoring.TrashCan.prefab, TransformUsageFlags.Dynamic)
            ));
        }
    }
}
