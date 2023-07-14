using System;
using Project.Scripts.Utilities;
using UnityEngine;

namespace Project.Scripts.Grid
{
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public class HeatMapGenericVisual : MonoBehaviour
    {
        private GridField<HeatMapDataObject> _heatMap;
        private Mesh _mesh;
        private bool _updateMesh;

        private void Awake()
        {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
        }

        public void SetGrid(GridField<HeatMapDataObject> gridField)
        {
            if (_heatMap != null) _heatMap.OnGridFieldChanged -= OnGridFieldValueChanged;
            _heatMap = gridField;
            UpdateHeatMapVisual();
            
            _heatMap.OnGridFieldChanged += OnGridFieldValueChanged;
        }

        private void OnGridFieldValueChanged()
        {
            _updateMesh = true;
        }

        private void LateUpdate()
        {
            if (_updateMesh)
            {
                _updateMesh = false;
                UpdateHeatMapVisual();
            }
        }

        private void UpdateHeatMapVisual() {
            MeshUtils.CreateEmptyMeshArrays(_heatMap.Width * _heatMap.Height, out Vector3[] vertices, out Vector2[] uv, out int[] triangles);

            for (int x = 0; x < _heatMap.Width; x++) {
                for (int y = 0; y < _heatMap.Height; y++) {
                    int index = x * _heatMap.Height + y;
                    Vector3 quadSize = new Vector3(1, 1) * _heatMap.CellSize;

                    HeatMapDataObject gridValue = _heatMap.GetCellData(x, y);
                    float gridValueNormalized = gridValue.GetNormalizedValue();
                    Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);
                    MeshUtils.AddToMeshArrays(vertices, uv, triangles, index, _heatMap.GetLocalPosition(x, y), 0f, quadSize, gridValueUV, gridValueUV);
                }
            }

            _mesh.vertices = vertices;
            _mesh.uv = uv;
            _mesh.triangles = triangles;
        }

    }
}
