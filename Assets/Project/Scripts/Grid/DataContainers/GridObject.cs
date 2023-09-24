using Project.Scripts.Buildings;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using UnityEngine;

namespace Project.Scripts.Grid.DataContainers
{
    public class GridObject
    {
        private GridField<GridObject> GridField => Chunk.ChunkBuildingGrid;
        public Vector2Int Position { get; }
        public bool Occupied => Building != null;
        public PlacedBuildingEntity Building { get; private set; }
        public ResourceType ResourceNode { get; private set; }
        public GridChunk Chunk { get; }

        public GridObject(GridChunk chunk, Vector2Int position, ResourceType resource = ResourceType.None)
        {
            Chunk = chunk;
            Position = position;
            ResourceNode = resource;

            if (ResourceNode != ResourceType.None) SetResource(ResourceNode);
        }

        #region Functions
        public void Occupy(PlacedBuildingEntity building)
        {
            if (Building != null) return;
            Building = building;
            GridField.TriggerGridObjectChanged(Position);
        }

        public void ClearBuilding()
        {
            if (Building == null) return;
            Building = null;
            GridField.TriggerGridObjectChanged(Position);
        }

        public void SetResource(ResourceType resourceType)
        {
            if(ResourceNode != ResourceType.None || resourceType == ResourceType.None)return;
            ResourceNode = resourceType;
            Vector3Int tilePos = new Vector3Int(
                Mathf.FloorToInt(Position.x - GridBuildingSystem.ChunkSize.x / 2f),
                Mathf.FloorToInt(Position.y - GridBuildingSystem.ChunkSize.y / 2f),
                0);
            Chunk.ChunkTilemap.SetTile(tilePos, ResourcesUtility.GetResourceTile(ResourceNode));
        }

        public override string ToString()
        {
            return Position + "\n" + Building + "\n" + ResourceNode;
        }
        #endregion
    }
}
