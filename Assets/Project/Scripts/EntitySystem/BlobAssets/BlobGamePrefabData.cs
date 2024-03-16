using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.EntitySystem.BlobAssets
{
    public struct BlobGamePrefabData
    {
        public BlobArray<BlobBuildingData> Buildings;
        public BlobArray<BlobIDInt2Pair> ItemAtlasLookUp;
        public BlobArray<BlobIDInt2Pair> TileAtlasLookUp;
        public Entity ItemPrefab;
        public Entity TilePrefab;
        public Entity TiledBackgroundPrefab;

        public BlobBuildingData GetBuildingDataFormID(int id)
        {
            for (var i = 0; i < Buildings.Length; i++)
            {
                if (Buildings[i].BuildingID == id)
                {
                    return Buildings[i];
                }
            }

            Debug.LogError($"There was no match found for building id {id} in GameBlob asset");
            return default;
        }
        
        public int2 GetAtlasPositionForItem(int id)
        {
            for (var i = 0; i < ItemAtlasLookUp.Length; i++)
            {
                if (ItemAtlasLookUp[i].ID == id)
                {
                    return ItemAtlasLookUp[i].Position;
                }
            }

            Debug.LogError($"There was no match found for item id {id} in GameBlob asset");
            return default;
        }
        
        public int2 GetAtlasPositionForTile(int id)
        {
            for (var i = 0; i < TileAtlasLookUp.Length; i++)
            {
                if (TileAtlasLookUp[i].ID == id)
                {
                    return TileAtlasLookUp[i].Position;
                }
            }

            Debug.LogError($"There was no match found for tile id {id} in GameBlob asset");
            return default;
        }
    }
}