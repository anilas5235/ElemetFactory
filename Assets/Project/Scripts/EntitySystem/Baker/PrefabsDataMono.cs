using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.Utilities;
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

         public BuildingScriptableData[] buildingScriptableData;
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
                GetEntity(authoring.SolidResource, TransformUsageFlags.Dynamic)
            ));

            BuildingData[] buildingData = new BuildingData[authoring.buildingScriptableData.Length];

            for (int i = 0; i < buildingData.Length; i++)
            {
                BuildingScriptableData data = authoring.buildingScriptableData[i];
                buildingData[i] = new BuildingData(data.nameString,
                    GetEntity(data.prefab, TransformUsageFlags.Dynamic),
                    data.InputOffsets, data.OutputOffsets, i,data.size);
            }
            
            ResourcesUtility.SetBuildingData(buildingData);
        }
    }
}
