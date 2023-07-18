using UnityEngine;

namespace Project.Scripts.Grid
{
    public class GridChunk : MonoBehaviour
    {
        public GridField<GridObject> _buildingGrid { get; private set; }
        public Vector3 LocalPosition { get; private set; }
        public Vector2Int ChunkPosition { get; private set; }

        private void Awake()
        {
            _buildingGrid = new GridField<GridObject>(GridBuildingSystem.GridSize,GridBuildingSystem.CellSize, transform,
                            (field, pos) => new GridObject(field, pos));
        }

        public void SetPosition(Vector2Int chunkPosition, Vector3 localPosition)
        {
            ChunkPosition = ChunkPosition;
            LocalPosition = localPosition;
            transform.localPosition = LocalPosition;
        }
    }
}
