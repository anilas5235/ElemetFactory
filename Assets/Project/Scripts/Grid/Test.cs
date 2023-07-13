using Project.Scripts.Utilities;
using UnityEngine;

namespace Project.Scripts.Grid
{
    public class Test : MonoBehaviour
    {
        private GridField<bool> _gridField;
        void Start()
        {
            _gridField = new GridField<bool>(10, 10,10f,transform.position);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 position = GeneralUtilities.GetMousePosition();
                _gridField.SetCellValue(position,!_gridField.GetCellValue(position));
            }
        }
    }
}
