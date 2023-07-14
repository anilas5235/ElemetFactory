using System;
using Project.Scripts.Buildings;
using Project.Scripts.Utilities;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Project.Scripts.Grid
{
    public class GridBuildingSystem : MonoBehaviour
    {
        [SerializeField] private GameObject testBuilding;
        private GridField<GridObject> buildingGrid;

        public static Vector2Int GridSize = new Vector2Int(10, 10);
        public static readonly float CellSize = 10f;
        

        private void Awake()
        {
            buildingGrid = new GridField<GridObject>(GridSize.x, GridSize.y, CellSize, transform,
                ((field, pos) => new GridObject(field, pos,transform)));
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
               GridObject gridObject =  buildingGrid.GetCellData(GeneralUtilities.GetMousePosition());
               if(gridObject == null) return;
               if(!gridObject.Occupied) gridObject.PlaceBuilding(Instantiate(testBuilding).transform);
            }
            if (Input.GetMouseButton(1))
            {
                GridObject gridObject =  buildingGrid.GetCellData(GeneralUtilities.GetMousePosition());
                if(gridObject == null) return;
                if(gridObject.Occupied) gridObject.ClearBuilding();
            }
        }
    }

    public class GridObject
    {
        private GridField<GridObject> _gridField;
        public Vector2Int Position { get; }
        public Vector3 LocalPosition { get; }

        public bool Occupied => _transformBuilding;

        private Transform _transformBuilding;
        private readonly Transform _transformParent;

        public GridObject(GridField<GridObject> gridField, Vector2Int position, Transform transformParent)
        {
            _gridField = gridField;
            Position = position;
            _transformParent = transformParent;
            LocalPosition = _gridField.GetLocalPosition(Position);
        }

        public void PlaceBuilding(Transform building)
        {
            if(_transformBuilding) return;
            _transformBuilding = building;
            _transformBuilding.SetParent(_transformParent);
            _transformBuilding.localScale = new Vector3(_gridField.CellSize, _gridField.CellSize);
            _transformBuilding.localPosition = LocalPosition;
            _gridField.TriggerGridObjectChanged(Position);
        }

        public void ClearBuilding()
        {
            _transformBuilding.gameObject.GetComponent<BuildingBase>().Destroy();
            _transformBuilding = null;
            _gridField.TriggerGridObjectChanged(Position);
        }

        public override string ToString()
        {
            return Position+"\n" + _transformBuilding?.name;
        }
    }
}
