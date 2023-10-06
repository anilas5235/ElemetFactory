using System.Collections.Generic;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Systems;
using Project.Scripts.General;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.Interaction;
using Project.Scripts.Utilities;
using UI.Windows;
using UnityEngine;

namespace Project.Scripts.Grid
{
    /*
     * Code based on the work of Code Monkey
     */
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
            //LoadAllChunksFromSave(WorldSaveHandler.GetWorldSave());
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

        private void OnApplicationQuit()
        {
            //SaveAllChunksToFile(Chunks);
        }

        private void CanBuild(bool val)
        {
            buildingEnabled = !val;
        }

        /*
        #region InfoFunctions
        public static Vector2Int GetChunkPosition(Vector3 worldPosition)
        {
            return new Vector2Int(Mathf.RoundToInt(worldPosition.x / ChunkSizeIntUnits.x),
                Mathf.RoundToInt(worldPosition.y / ChunkSizeIntUnits.y));
        }

        public static Vector3 GetChunkLocalPosition(Vector2Int chunkPosition)
        {
            return new Vector3(chunkPosition.x * ChunkSizeIntUnits.x, chunkPosition.y * ChunkSizeIntUnits.y);
        }

        public GridObject GetGridObjectFormPseudoPosition(Vector2Int position, GridChunk originChunk)
        {
            Vector2Int chunkOffset = new Vector2Int( Mathf.FloorToInt((float)position.x / ChunkSize.x), Mathf.FloorToInt((float)position.y / ChunkSize.y));
            Vector2Int newPos = new Vector2Int(position.x - chunkOffset.x * ChunkSize.x, position.y - chunkOffset.y * ChunkSize.y);
            return GetChunk(originChunk.ChunkPosition + chunkOffset).ChunkBuildingGrid.GetCellData(newPos);
        }
        
        public static Vector2Int GetPseudoPosition(GridChunk chunk, GridChunk myChunk, Vector2Int position)
        {
            Vector2Int chunkOffset = chunk.ChunkPosition - myChunk.ChunkPosition;
            return position + chunkOffset * ChunkSize;
        }
        
        #endregion
        
        /// <summary>
        /// Saves the current world to a datafile 
        /// </summary>
        /// <param name="gridChunks">all the chunk data that has been generated for this world</param>
        private static void SaveAllChunksToFile(Dictionary<Vector2Int,GridChunk> gridChunks)
        {
            ChunkSave[] chunkSaves = new ChunkSave[gridChunks.Count];
            int index = 0;
            foreach (KeyValuePair<Vector2Int,GridChunk> chunk in gridChunks)
            {
                CreateChunkSave(chunkSaves, index, chunk.Value);
                index++;
            }
            WorldSaveHandler.CurrentWorldSave.chunkSaves = chunkSaves;
            WorldSaveHandler.SaveWorldToFile();
        }

        private static void CreateChunkSave(ChunkSave[] chunkSaves, int index, GridChunk chunk)
        {

            PlacedBuildingData[] placedBuildingData = new PlacedBuildingData[chunk.Buildings.Count];
            for (int i = 0; i < placedBuildingData.Length; i++) placedBuildingData[i] = chunk.Buildings[i].MyPlacedBuildingData;

            chunkSaves[index] = new ChunkSave
            {
                chunkPosition = chunk.ChunkPosition, chunkResourcePatches = chunk.ChunkResourcePatches,
                placedBuildingData = placedBuildingData,
            };
        }*/
        
    }
}
