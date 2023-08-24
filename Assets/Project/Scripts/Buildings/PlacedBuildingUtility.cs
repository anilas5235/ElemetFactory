using Project.Scripts.Grid.DataContainers;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public static class PlacedBuildingUtility 
    {
        public static bool CheckForBuilding(Vector2Int targetPos, GridChunk myChunk,out PlacedBuilding building)
        {
            GridObject cell = myChunk.ChunkBuildingGrid.IsValidPosition(targetPos)
                ? myChunk.ChunkBuildingGrid.GetCellData(targetPos)
                : myChunk.myGridBuildingSystem.GetGridObjectFormPseudoPosition(targetPos, myChunk);
            building = cell.Building;
            return building;
        }
    }
}
