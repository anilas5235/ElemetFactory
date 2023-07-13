using Project.Scripts.Utilities;
using UnityEngine;

namespace Project.Scripts.Grid
{
    public class Test : MonoBehaviour
    {
        private GridField gridField;
        void Start()
        {
            gridField = new GridField(10, 10,10f,transform.position);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 position = GeneralUtilities.GetMousePosition();
                gridField.SetCellValue(position,gridField.GetCellValue(position)+ 1);
            }
        }
    }
}
