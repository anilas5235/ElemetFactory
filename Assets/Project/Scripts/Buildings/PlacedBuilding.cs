using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class PlacedBuilding : MonoBehaviour
    {
        public static PlacedBuilding CreateBuilding(Vector3 localPosition, Vector2Int origin, Vector2Int[] occupiedCells,
            BuildingDataBase.Directions direction, BuildingDataBase buildingData, Transform transformParent,
            float cellSize)
        {
            Transform buildingTransform = Instantiate(buildingData.prefab, localPosition,
                Quaternion.Euler(0, 0, buildingData.GetRotation(direction))).transform;

            PlacedBuilding placedBuilding = buildingTransform.GetComponent<PlacedBuilding>();

            placedBuilding._origin = origin;
            placedBuilding.BuildingData = buildingData;
            placedBuilding._direction = direction;
            placedBuilding.OccupiedCells = occupiedCells;

            buildingTransform.SetParent(transformParent);
            buildingTransform.localScale = new Vector3(cellSize, cellSize);
            buildingTransform.localPosition = localPosition;

            return placedBuilding;
        }


        public BuildingDataBase BuildingData { get; private set; }
        public Vector2Int[] OccupiedCells { get; private set; }
        private Vector2Int _origin;
        private BuildingDataBase.Directions _direction;

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public override string ToString()
        {
            return BuildingData.ToString();
        }
    }
}
