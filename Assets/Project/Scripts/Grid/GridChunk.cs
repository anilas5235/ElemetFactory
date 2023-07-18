using Project.Scripts.Visualisation;
using UnityEngine;

namespace Project.Scripts.Grid
{
    public class GridChunk : MonoBehaviour
    {
        public GridField<GridObject> BuildingGrid { get; private set; }
        public Vector3 LocalPosition { get; private set; }
        public Vector2Int ChunkPosition { get; private set; }
        
        public void SetPosition(Vector2Int chunkPosition, Vector3 localPosition)
        {
            ChunkPosition = chunkPosition;
            LocalPosition = localPosition;
            transform.localPosition = LocalPosition;
            BuildingGrid = new GridField<GridObject>(GridBuildingSystem.GridSize,GridBuildingSystem.CellSize, transform,
                (field, pos) => new GridObject(field, pos, this));
        }
    }
}
