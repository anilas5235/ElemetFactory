using Project.Scripts.Grid;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class PlacedBuilding : MonoBehaviour
    {
        public static PlacedBuilding CreateBuilding(Vector3 localPosition, Vector2Int origin,
            BuildingDataBase.Directions direction, BuildingGridResources.PossibleBuildings buildingData,
            Transform transformParent,
            float cellSize)
        {
            Transform buildingTransform = Instantiate(BuildingGridResources.GetBuildingDataBase(buildingData).prefab,
                localPosition,
                Quaternion.Euler(0, 0, BuildingDataBase.GetRotation(direction))).transform;

            PlacedBuilding placedBuilding = buildingTransform.GetComponent<PlacedBuilding>();

            placedBuilding.PlacedBuildingData = new PlacedBuildingData()
            {
                origin = origin,
                buildingDataID =(int) buildingData,
                directionID =(int) direction,
            };

            buildingTransform.SetParent(transformParent);
            buildingTransform.localScale = new Vector3(cellSize, cellSize);
            buildingTransform.localPosition = localPosition;

            return placedBuilding;
        }

        public PlacedBuildingData PlacedBuildingData { get; private set; }

        public Vector2Int[] GetGridPositionList()
        {
            return BuildingGridResources.GetBuildingDataBase( PlacedBuildingData.buildingDataID)
                .GetGridPositionList(PlacedBuildingData);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public override string ToString()
        {
            return PlacedBuildingData.buildingDataID.ToString();
        }
    }
}
