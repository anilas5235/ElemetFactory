using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Buildings;
using Project.Scripts.CellType;
using Project.Scripts.Utilities;
using Project.Scripts.Visualisation;
using UnityEngine;

namespace Project.Scripts.Grid
{
    /*
     * Code based on the work of Code Monkey
     */
    public class GridBuildingSystem : MonoBehaviour
    {
        [SerializeField] private List<BuildingDataBase> buildings;
        private BuildingDataBase _selectedBuilding;
        private BuildingDataBase.Directions _direction = BuildingDataBase.Directions.Down;

        public static readonly Vector2Int GridSize = new Vector2Int(10, 10);
        public const float CellSize = 10f;
        public static readonly Vector2 ChunkSize = new Vector2(CellSize * GridSize.x, CellSize * GridSize.y);
        [SerializeField] private GameObject chunkPrefap;

        public Dictionary<Vector2Int, GridChunk> Chunks { get; private set; } = new Dictionary<Vector2Int, GridChunk>();

        private void Awake()
        {
            _selectedBuilding = buildings.First();
            InitialChunks();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                _direction = BuildingDataBase.GetNextDirection(_direction);
                Debug.Log($"rotation: {_direction}");
            }

            if (Input.GetMouseButton(0) && _selectedBuilding)
            {
                Vector2 chunkPos = GeneralUtilities.GetMousePosition();
                if (!Chunks.TryGetValue(
                        new Vector2Int(Mathf.RoundToInt(chunkPos.x / ChunkSize.x),
                            Mathf.RoundToInt(chunkPos.y / ChunkSize.y)), out var chunk)) return;

                if (chunk == null) return;
                GridField<GridObject> buildingGrid = chunk.BuildingGrid;
                GridObject gridObject = buildingGrid.GetCellData(GeneralUtilities.GetMousePosition());
                if (gridObject == null) return;
                List<Vector2Int> positions = _selectedBuilding.GetGridPositionList(gridObject.Position, _direction);
                bool canPlace = true;
                foreach (Vector2Int position in positions)
                {
                    GridObject gridObj = buildingGrid.GetCellData(position);
                    if (gridObj == null || gridObj.Occupied)
                    {
                        canPlace = false;
                        break;
                    }
                }

                if (canPlace)
                {
                    PlacedBuilding placedBuilding = PlacedBuilding.CreateBuilding(
                        buildingGrid.GetLocalPosition(gridObject.Position),
                        gridObject.Position, positions.ToArray(), _direction, _selectedBuilding, transform,
                        buildingGrid.CellSize);
                    foreach (Vector2Int blockedCell in positions)
                    {
                        buildingGrid.GetCellData(blockedCell).Occupy(placedBuilding);
                        buildingGrid.TriggerGridObjectChanged(blockedCell);
                    }
                }
            }

            if (Input.GetMouseButton(1))
            {
                Vector2 chunkPos = GeneralUtilities.GetMousePosition();
                if (!Chunks.TryGetValue(
                        new Vector2Int(Mathf.RoundToInt(chunkPos.x / ChunkSize.x),
                            Mathf.RoundToInt(chunkPos.y / ChunkSize.y)), out GridChunk chunk)) return;

                if(chunk == null) return;
                GridField<GridObject> buildingGrid = chunk.BuildingGrid;
                GridObject gridObject =  buildingGrid.GetCellData(GeneralUtilities.GetMousePosition());
                if(gridObject == null) return;
                if (gridObject.Occupied)
                {
                    PlacedBuilding placedBuilding = gridObject.Building;
                    placedBuilding.Destroy();
                    
                    foreach (Vector2Int occupiedCell in placedBuilding.OccupiedCells)
                    {
                        buildingGrid.GetCellData(occupiedCell).ClearBuilding();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) { _selectedBuilding = buildings[0]; }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { _selectedBuilding = buildings[1]; }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { _selectedBuilding = buildings[2]; }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { _selectedBuilding = buildings[3]; }

        }

        private void InitialChunks()
        {
            Vector2Int size = new Vector2Int(10, 10);
            for (int x = -size.x; x < size.x+1; x++)
            {
                for (int y = -size.y; y < size.y+1; y++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    CreateChunk(position);
                }
            }
        }

        private void CreateChunk(Vector2Int chunkPosition)
        {
            GridChunk newChunk = Instantiate(chunkPrefap,transform).GetComponent<GridChunk>();
            Vector3 localPosition = new Vector3(chunkPosition.x * ChunkSize.x, chunkPosition.y * ChunkSize.y);
            newChunk.Initialization(chunkPosition, localPosition);
            Chunks.Add(chunkPosition, newChunk);
        }
    }

    public class GridObject
    {
        private readonly GridField<GridObject> _gridField;
        public Vector2Int Position { get; }
        public bool Occupied => Building;
        public PlacedBuilding Building { get; private set; }
        public CellResources ResourceNode { get; }
        public GridChunk Chunk { get; }

        public GridObject(GridField<GridObject> gridField, Vector2Int position, GridChunk chunk,
            CellResources resource = null)
        {
            _gridField = gridField;
            Position = position;
            Chunk = chunk;
            ResourceNode = new CellResources();

            Vector3Int tilePos = new Vector3Int(
                Mathf.FloorToInt(Position.x + GridBuildingSystem.GridSize.x * Chunk.ChunkPosition.x -
                                 GridBuildingSystem.GridSize.x / 2),
                Mathf.FloorToInt(Position.y + GridBuildingSystem.GridSize.y * Chunk.ChunkPosition.y -
                                 GridBuildingSystem.GridSize.y / 2), 0);
            ResourcesTileMap.Instance.ResourceMap.SetTile(tilePos,
                VisualResources.Instance.GetTileSource(ResourceNode.NodeType));
        }

        public void Occupy(PlacedBuilding building)
        {
            if (Building) return;
            Building = building;
            _gridField.TriggerGridObjectChanged(Position);
        }

        public void ClearBuilding()
        {
            if (!Building) return;
            Building = null;
            _gridField.TriggerGridObjectChanged(Position);
        }

        public override string ToString()
        {
            return Position + "\n" + Building + "\n" + ResourceNode;
        }
    }
}
