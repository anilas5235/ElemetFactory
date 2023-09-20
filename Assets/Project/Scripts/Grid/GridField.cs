using System;
using Project.Scripts.Utilities;
using UnityEngine;

namespace Project.Scripts.Grid
{
    /*
     * Code based on the work of Code Monkey
     */
    public class GridField<TGridObject>
    {
        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }

        public readonly Vector3 _halfCellSize;
        private readonly Vector3 _originPosition,_fieldOffset;
        private readonly TGridObject[,] _cellValues;
        private readonly TextMesh[,] _cellTexts;

        private static readonly bool Debug = false, DebugDrawGrid = false;

        public Action OnGridFieldChanged;

        public GridField(int width, int height, float cellSize, Transform originTransform,
            Func<GridField<TGridObject>, Vector2Int, TGridObject> createGridObject)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
            _halfCellSize = new Vector3(cellSize, cellSize) * .5f;
            _fieldOffset = new Vector3(Width / 2f, Height / 2f) * CellSize - _halfCellSize;
            _originPosition = originTransform.position - _fieldOffset;

            _cellValues = new TGridObject[Width, Height];
            _cellTexts = new TextMesh[Width, Height];

            for (int x = 0; x < _cellValues.GetLength(0); x++)
            {
                for (int y = 0; y < _cellValues.GetLength(1); y++)
                {
                    _cellValues[x, y] = createGridObject(this, new Vector2Int(x, y));
                }
            }

            if (!Debug) return;

            for (int x = 0; x < _cellValues.GetLength(0); x++)
            {
                for (int y = 0; y < _cellValues.GetLength(1); y++)
                {
                    _cellTexts[x, y] = GeneralUtilities.CreateWorldText(_cellValues[x, y]?.ToString(),
                        GetLocalPosition(x, y), 20, Color.green, originTransform);
                }

                if (!DebugDrawGrid) continue;
                UnityEngine.Debug.DrawLine(GetWorldPosition(x, 0) - _halfCellSize,
                    GetWorldPosition(x, Height) - _halfCellSize,
                    Color.white, 100f);
                UnityEngine.Debug.DrawLine(GetWorldPosition(0, x) - _halfCellSize,
                    GetWorldPosition(Width, x) - _halfCellSize,
                    Color.white, 100f);
            }

            if (!DebugDrawGrid) return;
            UnityEngine.Debug.DrawLine(GetWorldPosition(0, Height) - _halfCellSize,
                GetWorldPosition(Width, Height) - _halfCellSize,
                Color.white, 100f);
            UnityEngine.Debug.DrawLine(GetWorldPosition(Width, 0) - _halfCellSize,
                GetWorldPosition(Width, Height) - _halfCellSize,
                Color.white, 100f);

        }

        public GridField(Vector2Int size, float cellSize, Transform originTransform, Func<GridField<TGridObject>, Vector2Int, TGridObject> createGridObject) 
            : this(size.x,size.y,cellSize,originTransform,createGridObject)
        {
        }

        #region GetPositions

        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * CellSize + _originPosition;
        }

        public Vector3 GetWorldPosition(Vector2Int position)
        {
            return GetWorldPosition(position.x,position.y);
        }

        public Vector3 GetLocalPosition(int x, int y)
        {
            return new Vector3(x, y) * CellSize - _fieldOffset;
        }
        public Vector3 GetLocalPosition(Vector2Int position)
        {
            return GetLocalPosition(position.x, position.y);
        }

        public Vector2Int GetCellPosition(Vector3 worldPosition)
        {
            return new Vector2Int(Mathf.RoundToInt((worldPosition.x - _originPosition.x) / CellSize),
                Mathf.RoundToInt((worldPosition.y - _originPosition.y) / CellSize));
        }

        #endregion
        
        #region Set&GetCellValue

        public void TriggerGridObjectChanged(Vector2Int position)
        {
            if(!Debug) return;
            _cellTexts[position.x, position.y].text = _cellValues[position.x, position.y]?.ToString();
            OnGridFieldChanged?.Invoke();
        }
        public void SetCellData(int x, int y, TGridObject value)
        {
            if (!IsValidPosition(x, y) || _cellValues[x, y].Equals(value)) return;
            _cellValues[x, y] = value;
            if(Debug) _cellTexts[x, y].text = value.ToString();
            OnGridFieldChanged?.Invoke();
        }
        public void SetCellData(Vector2Int position,TGridObject  value)
        {
            SetCellData(position.x, position.y, value);
        }
        public void SetCellData(Vector3 worldPosition, TGridObject  value)
        {
            SetCellData(GetCellPosition(worldPosition), value);
        }

        public TGridObject  GetCellData(int x, int y)
        {
            if (!IsValidPosition(x, y)) return default;
            return _cellValues[x, y];
        }
        public TGridObject  GetCellData(Vector2Int position)
        {
            return GetCellData(position.x, position.y);
        }
        
        public TGridObject  GetCellData(Vector3 worldPosition)
        {
            return GetCellData(GetCellPosition(worldPosition));
        }
        
        #endregion

        #region SetCellBlock

        public void SetCellBlockData(int xStart, int xEnd, int yStart, int yEnd, TGridObject value)
        {
            xStart = Mathf.Clamp(xStart, 0, Width);
            xEnd = Mathf.Clamp(xEnd, 0, Width);
            yStart = Mathf.Clamp(yStart, 0, Height);
            yEnd = Mathf.Clamp(yEnd, 0, Height);
            
            if (xStart - xEnd < 1 || yStart - yEnd < 1) return;

            for (int x = xStart; x < xEnd; x++) { for (int y = yStart; y < yEnd; y++) SetCellData(x, y, value); }
        }

        public void SetCellBlockData(Vector2Int leftBottomCorner, Vector2Int rightTopCorner, TGridObject value)
        {
            SetCellBlockData(leftBottomCorner.x,rightTopCorner.x,leftBottomCorner.y,rightTopCorner.y, value);
        }

        #endregion

        #region IsValidPosition

        public bool IsValidPosition(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < Width && y < Height);
        }

        public bool IsValidPosition(Vector2Int position)
        {
            return IsValidPosition(position.x, position.y);
        }
        #endregion
    }
}
