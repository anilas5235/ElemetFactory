using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Buildings;
using Project.Scripts.Utilities;
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
        private GridField<GridObject> _buildingGrid;
        private BuildingDataBase.Directions _direction = BuildingDataBase.Directions.Down;

        public static Vector2Int GridSize = new Vector2Int(10, 10);
        public static readonly float CellSize = 10f;
        

        private void Awake()
        {
            _buildingGrid = new GridField<GridObject>(GridSize.x, GridSize.y, CellSize, transform,
                (field, pos) => new GridObject(field, pos));
            _selectedBuilding = buildings.First();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                _direction = BuildingDataBase.GetNextDirection(_direction);
                Debug.Log($"rotation: {_direction}");
            }
            if (Input.GetMouseButton(0)&& _selectedBuilding)
            {
                GridObject gridObject =  _buildingGrid.GetCellData(GeneralUtilities.GetMousePosition());
                if(gridObject == null) return;
                List<Vector2Int> positions = _selectedBuilding.GetGridPositionList(gridObject.Position, _direction);
                bool canPlace = true;
                foreach (Vector2Int position in positions)
                {
                    GridObject gridObj = _buildingGrid.GetCellData(position);
                    if(gridObj == null|| gridObj.Occupied){ canPlace = false; break;}
                }
                if (canPlace)
                {
                    PlacedBuilding placedBuilding =  PlacedBuilding.CreateBuilding(_buildingGrid.GetLocalPosition(gridObject.Position),
                        gridObject.Position, positions.ToArray(), _direction, _selectedBuilding, transform, _buildingGrid.CellSize);
                    foreach (Vector2Int blockedCell in positions)
                    {
                        _buildingGrid.GetCellData(blockedCell).Occupy(placedBuilding);
                        _buildingGrid.TriggerGridObjectChanged(blockedCell);
                    }
                }
            }
            if (Input.GetMouseButton(1))
            {
                GridObject gridObject =  _buildingGrid.GetCellData(GeneralUtilities.GetMousePosition());
                if(gridObject == null) return;
                if (gridObject.Occupied)
                {
                    PlacedBuilding placedBuilding = gridObject.Building;
                    placedBuilding.Destroy();
                    
                    foreach (Vector2Int occupiedCell in placedBuilding.OccupiedCells)
                    {
                        _buildingGrid.GetCellData(occupiedCell).ClearBuilding();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) { _selectedBuilding = buildings[0]; }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { _selectedBuilding = buildings[1]; }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { _selectedBuilding = buildings[2]; }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { _selectedBuilding = buildings[3]; }

        }
    }

    public class GridObject
    {
        private GridField<GridObject> _gridField;
        public Vector2Int Position { get; }
        public bool Occupied => Building;

        public PlacedBuilding Building { get; private set; }

        public GridObject(GridField<GridObject> gridField, Vector2Int position)
        {
            _gridField = gridField;
            Position = position;
        }

        public void Occupy(PlacedBuilding building)
        {
            if(Building) return;
            Building = building;
            _gridField.TriggerGridObjectChanged(Position);
        }

        public void ClearBuilding()
        {
            if(!Building) return;
            Building = null;
            _gridField.TriggerGridObjectChanged(Position);
        }

        public override string ToString()
        {
            return Position+"\n" + Building;
        }
    }
}
