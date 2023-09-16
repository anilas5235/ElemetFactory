using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.Utilities;
using UnityEngine;

namespace Project.Scripts.Grid.HeatMap
{
    public class Test : MonoBehaviour
    {
        private GridField<HeatMapDataObject> _heatMap;
        void Start()
        {
            _heatMap = new GridField<HeatMapDataObject>(20, 20, 5f, transform,((field, pos) =>new HeatMapDataObject(field,pos)));
            GetComponent<HeatMapGenericVisual>()?.SetGrid(_heatMap);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 position = GeneralUtilities.GetMousePosition();
                _heatMap.GetCellData(position).AddValue(5);
            }
        }

        private void FixedUpdate()
        {
            Vector3 position = GeneralUtilities.GetMousePosition();
            _heatMap.GetCellData(position)?.AddValue(1);
            
            for (int x = 0; x < _heatMap.Width; x++)
            {
                for (int y = 0; y < _heatMap.Height; y++)
                {
                    _heatMap.GetCellData(x, y).AddValue(-.1f);
                }
            }
        }
    }
}
