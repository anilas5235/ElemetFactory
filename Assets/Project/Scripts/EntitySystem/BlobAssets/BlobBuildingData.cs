using Project.Scripts.Buildings.BuildingFoundation;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.BlobAssets
{
    public struct BlobBuildingData
    {
        public FixedString32Bytes nameString;
        public int BuildingID;
        public Entity Prefab;
        public BlobArray<int2> neededTiles;
        public BlobArray<PortData> inputOffsets;
        public BlobArray<PortData> outputOffsets;
    }
}