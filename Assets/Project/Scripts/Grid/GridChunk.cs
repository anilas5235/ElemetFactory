using System;
using Project.Scripts.CellType;
using UnityEngine;

namespace Project.Scripts.Grid
{
    public class GridChunk : MonoBehaviour
    {
        public GridField<GridObject> _buildingGrid { get; private set; }
        public Vector3 localPosition { get; }
        
        public GridChunk()
        {
            
        }

        private void Awake()
        {
            _buildingGrid = new GridField<GridObject>(GridBuildingSystem.GridSize,GridBuildingSystem.CellSize, transform,
                            (field, pos) => new GridObject(field, pos, new CellResources(CellResources.ResourcesType.Hydrogen)));
        }
    }
}
