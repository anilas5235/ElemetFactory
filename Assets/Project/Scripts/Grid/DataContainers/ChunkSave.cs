using System;
using Project.Scripts.Buildings.BuildingFoundation;
using Unity.Mathematics;

namespace Project.Scripts.Grid.DataContainers
{
    [Serializable]
    public struct ChunkResourcePatch
    {
        public int2[] positions;
        public int resourceID;
    }
    
    [Serializable]
    public class ChunkSave
    {
        public ChunkResourcePatch[] chunkResourcePatches;
        public PlacedBuildingData[] placedBuildingData;
        public int2 chunkPosition;
    }
}