using Project.Scripts.Buildings;
using Project.Scripts.Visualisation;
using UnityEngine;

namespace Project.Scripts.Grid.DataContainers
{
    public class GridObject
    {
        private GridField<GridObject> GridField => Chunk.ChunkBuildingGrid;
        public Vector2Int Position { get; }
        public bool Occupied => Building;
        public PlacedBuilding Building { get; private set; }
        public BuildingGridResources.ResourcesType ResourceNode { get; private set; }
        public GridChunk Chunk { get; }

        public GridObject(GridChunk chunk, Vector2Int position, BuildingGridResources.ResourcesType resource = BuildingGridResources.ResourcesType.None)
        {
            Chunk = chunk;
            Position = position;
            ResourceNode = resource;

            if (ResourceNode != BuildingGridResources.ResourcesType.None) SetResource(ResourceNode);
        }

        #region Functions
        public void Occupy(PlacedBuilding building)
        {
            if (Building) return;
            Building = building;
            GridField.TriggerGridObjectChanged(Position);
        }

        public void ClearBuilding()
        {
            if (!Building) return;
            Building = null;
            GridField.TriggerGridObjectChanged(Position);
        }

        public void SetResource(BuildingGridResources.ResourcesType resourcesType)
        {
            if(ResourceNode != BuildingGridResources.ResourcesType.None || resourcesType == BuildingGridResources.ResourcesType.None)return;
            ResourceNode = resourcesType;
            Vector3Int tilePos = new Vector3Int(
                Mathf.FloorToInt(Position.x - GridBuildingSystem.ChunkSize.x / 2f),
                Mathf.FloorToInt(Position.y - GridBuildingSystem.ChunkSize.y / 2f),
                0);
            Chunk.ChunkTilemap.SetTile(tilePos, VisualResources.GetTileSource(ResourceNode));
        }

        public override string ToString()
        {
            return Position + "\n" + Building + "\n" + ResourceNode;
        }
        #endregion
    }
}
