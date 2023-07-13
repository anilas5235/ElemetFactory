using Project.Scripts.Utilities;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Project.Scripts.Grid
{
    public class GridField<TGridObject>
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public float CellSize { get; protected set; }

        private readonly Vector3 _halfCellSize;
        private Vector3 _originPosition;
        private readonly TGridObject[,] _cellValues;
        private readonly TextMesh[,] _cellTexts;

        private static bool debug = true;

        public GridField(int width, int height, float cellSize, Vector3 originPosition)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
            _halfCellSize = new Vector3(cellSize, cellSize) * .5f;
            _originPosition = originPosition;

            _cellValues = new TGridObject[Width, Height];
            _cellTexts = new TextMesh[Width, Height];

            if (debug)
            {
                for (int x = 0; x < _cellValues.GetLength(0); x++)
                {
                    for (int y = 0; y < _cellValues.GetLength(1); y++)
                    {
                        _cellTexts[x, y] = GeneralUtilities.CreateWorldText($"{_cellValues[x, y]}", GetWorldPosition(x, y), 20);
                    }

                    Debug.DrawLine(GetWorldPosition(x, 0) - _halfCellSize, GetWorldPosition(x, Height) - _halfCellSize,
                        Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(0, x) - _halfCellSize, GetWorldPosition(Width, x) - _halfCellSize,
                        Color.white, 100f);
                }

                Debug.DrawLine(GetWorldPosition(0, Height) - _halfCellSize, GetWorldPosition(Width, Height) - _halfCellSize,
                    Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(Width, 0) - _halfCellSize, GetWorldPosition(Width, Height) - _halfCellSize,
                    Color.white, 100f);
            }

        }

        #region GetPositions

        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * CellSize + _originPosition;
        }

        public Vector3 GetWorldPosition(Vector2Int position)
        {
            return new Vector3(position.x, position.y) * CellSize;
        }

        public Vector2Int GetCellPosition(Vector3 worldPosition)
        {
            return new Vector2Int(Mathf.RoundToInt((worldPosition.x - _originPosition.x) / CellSize),
                Mathf.RoundToInt((worldPosition.y - _originPosition.y) / CellSize));
        }

        #endregion
        
        #region Set&GetCellValue

        public void SetCellValue(int x, int y, TGridObject value)
        {
            if (!IsValidPosition(x, y) || _cellValues[x, y].Equals(value)) return;
            _cellValues[x, y] = value;
            _cellTexts[x, y].text = $"{value}";
        }
        public void SetCellValue(Vector2Int position,TGridObject  value)
        {
            SetCellValue(position.x, position.y, value);
        }
        public void SetCellValue(Vector3 worldPosition, TGridObject  value)
        {
            SetCellValue(GetCellPosition(worldPosition), value);
        }

        public TGridObject  GetCellValue(int x, int y)
        {
            if (!IsValidPosition(x, y)) return default;
            return _cellValues[x, y];
        }
        public TGridObject  GetCellValue(Vector2Int position)
        {
            return GetCellValue(position.x, position.y);
        }
        
        public TGridObject  GetCellValue(Vector3 worldPosition)
        {
            return GetCellValue(GetCellPosition(worldPosition));
        }
        
        #endregion

        #region SetCellBlock

        public void SetCellBlockValues(int xStart, int xEnd, int yStart, int yEnd, TGridObject  value)
        {
            if (xStart < 0) xStart = 0;
            if (xEnd >= Width) xEnd = Width - 1;
            if (yStart < 0) yStart = 0;
            if (yEnd >= Height) yEnd = Height - 1;
            if (xStart - xEnd < 1 || yStart - yEnd < 1) return;

            for (int x = xStart; x < xEnd; x++)
            {
                for (int y = yStart; y < yEnd; y++) SetCellValue(x, y, value);
            }
        }

        public void SetCellBlockValues(Vector2Int leftBottomCorner, Vector2Int rightTopCorner, TGridObject  value)
        {
            int xStart = leftBottomCorner.x,
                yStart = leftBottomCorner.y,
                xEnd = rightTopCorner.x,
                yEnd = rightTopCorner.y;
            if (xStart < 0) xStart = 0;
            if (xEnd >= Width) xEnd = Width - 1;
            if (yStart < 0) yStart = 0;
            if (yEnd >= Height) yEnd = Height - 1;
            if (xStart - xEnd < 1 || yStart - yEnd < 1) return;

            for (int x = xStart; x < xEnd; x++)
            {
                for (int y = yStart; y < yEnd; y++) SetCellValue(x, y, value);
            }
        }

        #endregion

        #region IsValidPosition

        public bool IsValidPosition(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < Width && y < Height);
        }

        public bool IsValidPosition(Vector2Int position)
        {
            return (position.x >= 0 && position.y >= 0 && position.x < Width && position.y < Height);
        }

        #endregion
    }
}
