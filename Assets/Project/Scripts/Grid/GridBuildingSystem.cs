using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Systems;
using Project.Scripts.General;
using Project.Scripts.Interaction;
using Project.Scripts.Utilities;
using UI.Windows;
using UnityEngine;

namespace Project.Scripts.Grid
{
    [RequireComponent(typeof(UnityEngine.Grid))]
    public class GridBuildingSystem : Singleton<GridBuildingSystem>
    {
        public static bool Work = false;
        [SerializeField] private bool buildingEnabled = true;

        private int _selectedBuilding = 0;
        private FacingDirection _facingDirection = FacingDirection.Down;
        public Camera PlayerCam => CameraMovement.Instance.Camera;
        private void OnEnable()
        {
            UIWindowMaster.Instance.OnActiveUIChanged += CanBuild;
        }

        private void Update()
        {
            if (!buildingEnabled || !Work) return;

            if (Input.GetKeyDown(KeyCode.R))
            {
                _facingDirection = PlacedBuildingUtility.GetNextDirectionClockwise(_facingDirection);
                Debug.Log($"rotation: {_facingDirection}");
            }
            if (Input.GetMouseButton(0))
            {
                Vector3 mousePos = GeneralUtilities.GetMousePosition();
                bool place = PlacingSystem.Instance.TryToPlaceBuilding(mousePos,_selectedBuilding, _facingDirection);
                Debug.Log("placed "+place);
            }

            if (Input.GetMouseButton(1))
            {
                Vector3 mousePos = GeneralUtilities.GetMousePosition();
                bool del = PlacingSystem.Instance.TryToDeleteBuilding(mousePos);
                Debug.Log("deleted "+del);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) _selectedBuilding = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) _selectedBuilding = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) _selectedBuilding = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) _selectedBuilding = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha5)) _selectedBuilding = 4;
        }
        private void CanBuild(bool val)
        {
            buildingEnabled = !val;
        }
    }
}
