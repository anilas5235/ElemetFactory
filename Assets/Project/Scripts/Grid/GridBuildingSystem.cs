using System.Collections;
using System.Collections.Generic;
using Project.Scripts.Buildings;
using Project.Scripts.General;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.Interaction;
using Project.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Project.Scripts.Grid
{
    /*
     * Code based on the work of Code Monkey
     */
    [RequireComponent(typeof(UnityEngine.Grid))]
    public class GridBuildingSystem : MonoBehaviour
    {
        private PossibleBuildings _selectedBuilding = PossibleBuildings.Extractor;
        private FacingDirection _facingDirection = FacingDirection.Down;
        
        public static readonly Vector2Int ChunkSize = new Vector2Int(10, 10);
        public static readonly Vector2 ChunkSizeIntUnits = new Vector2(CellSize * ChunkSize.x, CellSize * ChunkSize.y);
        public const float CellSize = 10f;
        private GameObject chunkPrefap;

        public Dictionary<Vector2Int, GridChunk> Chunks { get; } = new Dictionary<Vector2Int, GridChunk>();
        private Vector2Int chunkPosWithPlayer = new Vector2Int(-10,-10);
        private int playerViewRadius;
        private List<Vector2Int> LoadedChunks { get; } = new List<Vector2Int>();

        private Camera PlayerCam => CameraMovement.Instance.Camera;

        private void Awake()
        {
            chunkPrefap = Resources.Load<GameObject>("Prefaps/chunk");
            GetComponent<UnityEngine.Grid>().cellSize = new Vector3(CellSize, CellSize, 0);
        }

        private void OnEnable()
        {
            LoadAllChunksFromSave(WorldSaveHandler.GetWorldSave());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                _facingDirection = PlacedBuildingUtility.GetNextDirectionClockwise(_facingDirection);
                Debug.Log($"rotation: {_facingDirection}");
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 mousePos =GeneralUtilities.GetMousePosition();
                GridChunk.TryToPlaceBuilding(GetChunk(mousePos),_selectedBuilding,mousePos,_facingDirection);
            }

            if (Input.GetMouseButton(1))
            {
                Vector3 mousePos =GeneralUtilities.GetMousePosition();
                GridChunk.TryToDeleteBuilding(GetChunk(mousePos),mousePos);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) _selectedBuilding = PossibleBuildings.Extractor;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) _selectedBuilding = PossibleBuildings.Conveyor;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) _selectedBuilding = PossibleBuildings.Combiner;
        }

        private void FixedUpdate()
        {
            UpdateLoadedChunks();
        }

        private void OnApplicationQuit()
        {
            SaveAllChunksToFile(Chunks);
        }

        #region PlayTimeLoad&UnLoadChunksSystem
        private void UpdateLoadedChunks()
        {
            Vector2Int currentPos = GetChunkPosition(PlayerCam.transform.position);
            int radius = Mathf.CeilToInt(PlayerCam.orthographicSize * PlayerCam.aspect / ChunkSizeIntUnits.x);
            if (chunkPosWithPlayer == currentPos && playerViewRadius == radius) return;
            chunkPosWithPlayer = currentPos;
            playerViewRadius = radius;

            List<Vector2Int> chunksToLoad = new List<Vector2Int>();
            List<Vector2Int> chunksToUnLoad = new List<Vector2Int>();
            
            for (int x = -playerViewRadius; x < playerViewRadius+1; x++)
            {
                for (int y = -playerViewRadius; y < playerViewRadius+1; y++)
                { 
                    chunksToLoad.Add(new Vector2Int(x, y) + chunkPosWithPlayer);
                }
            }

            foreach (Vector2Int loadedChunkPos in LoadedChunks)
            {
                if (chunksToLoad.Contains(loadedChunkPos)) chunksToLoad.Remove(loadedChunkPos);
                else chunksToUnLoad.Add(loadedChunkPos);
            }

            foreach (Vector2Int chunkPos in chunksToLoad) StartCoroutine(LoadChunk(chunkPos));
            foreach (Vector2Int chunkPos in chunksToUnLoad) StartCoroutine( UnLoadChunk(chunkPos));
        }

        private IEnumerator LoadChunk(Vector2Int position)
        {
            GridChunk targetChunk = GetChunk(position);
            targetChunk.Load(LoadedChunks);
            yield return null;
        }

        private IEnumerator  UnLoadChunk(Vector2Int position)
        {
            GridChunk targetChunk = GetChunk(position);
            targetChunk.UnLoad(LoadedChunks);
            yield return null;
        }
        #endregion

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

        public GridChunk GetChunk(Vector2Int chunkPosition)
        {
            return Chunks.TryGetValue(chunkPosition, out var chunk) ? chunk : CreateChunk(chunkPosition);
        }
        
        public GridChunk GetChunk(Vector3 worldPosition)
        {
            return GetChunk(GetChunkPosition(worldPosition));
        }
        
        #endregion

        #region ChunkCreationHandeling
        /// <summary>
        /// Places a newly generated chunk in the world
        /// </summary>
        /// <param name="chunkPosition">position that identifies the new chunk and determines its position</param>
        /// <returns>Ref to the new chunk Instance</returns>
        private GridChunk CreateChunk( Vector2Int chunkPosition)
        {
            GridChunk newChunk = Instantiate(chunkPrefap,transform).GetComponent<GridChunk>();
            Vector3 localPosition = new Vector3(chunkPosition.x * ChunkSizeIntUnits.x, chunkPosition.y * ChunkSizeIntUnits.y);
            newChunk.Initialization(this,chunkPosition, localPosition);
            newChunk.gameObject.name = $"Chunk {chunkPosition}";
            Chunks.Add(chunkPosition, newChunk);
            LoadedChunks.Add(chunkPosition);
            return newChunk;
        }
        
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
        }
        
        /// <summary>
        /// Recreates the world from specific data 
        /// </summary>
        /// <param name="worldSave">data of the world to load</param>
        private void LoadAllChunksFromSave(WorldSave worldSave)
        {
            ChunkSave[] chunkSaves = worldSave.chunkSaves;

            foreach (ChunkSave chunkSave in chunkSaves) LoadChunkFormSave(chunkSave);
            foreach (ChunkSave chunkSave in chunkSaves)
            {
                GridChunk gridChunk = GetChunk(chunkSave.chunkPosition);
                gridChunk.LoadBuildingsFromSave(chunkSave.placedBuildingData);
                gridChunk.UnLoad(LoadedChunks);
            }
        }
        
        /// <summary>
        /// Recreates a chunk from data and places in the world
        /// </summary>
        /// <param name="chunkSave">data of the chunk to load</param>
        private void LoadChunkFormSave(ChunkSave chunkSave)
        {
            GridChunk newChunk = Instantiate(chunkPrefap,transform).GetComponent<GridChunk>();
            newChunk.Initialization(this,chunkSave.chunkPosition, GetChunkLocalPosition(chunkSave.chunkPosition), chunkSave.chunkResourcePatches);
            newChunk.gameObject.name = $"Chunk {chunkSave.chunkPosition}";
            Chunks.Add(chunkSave.chunkPosition, newChunk);
            LoadedChunks.Add(chunkSave.chunkPosition);
        }
        #endregion
    }
}
