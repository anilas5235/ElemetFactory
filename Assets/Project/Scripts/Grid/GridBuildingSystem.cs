using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Buildings;
using Project.Scripts.General;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.Interaction;
using Project.Scripts.Utilities;
using UnityEngine;

namespace Project.Scripts.Grid
{
    /*
     * Code based on the work of Code Monkey
     */
    [RequireComponent(typeof(UnityEngine.Grid))]
    public class GridBuildingSystem : MonoBehaviour
    {
        [SerializeField] private BuildingGridResources.PossibleBuildings buildings;
        private BuildingGridResources.PossibleBuildings _selectedBuilding;
        private BuildingDataBase.Directions _direction = BuildingDataBase.Directions.Down;
        
        public static readonly Vector2Int GridSize = new Vector2Int(10, 10);
        public const float CellSize = 10f;
        public static readonly Vector2 ChunkSize = new Vector2(CellSize * GridSize.x, CellSize * GridSize.y);
        private GameObject chunkPrefap;

        private Dictionary<Vector2Int, GridChunk> Chunks { get; } = new Dictionary<Vector2Int, GridChunk>();
        private Vector2Int chunkPosWithPlayer = new Vector2Int(-10,-10);
        private int playerViewRadius;
        private List<Vector2Int> LoadedChunks { get; } = new List<Vector2Int>();

        private Camera PlayerCam => CameraMovement.Instance.Camera;

        private void Awake()
        {
            chunkPrefap = Resources.Load<GameObject>("Prefaps/chunk");
            GetComponent<UnityEngine.Grid>().cellSize = new Vector3(CellSize, CellSize, 0);
            LoadAllChunksFromSave(WorldSaveHandler.GetWorldSave());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                _direction = BuildingDataBase.GetNextDirection(_direction);
                Debug.Log($"rotation: {_direction}");
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 mousePos =GeneralUtilities.GetMousePosition();
                GridChunk.TryToPlaceBuilding(GetChunk(mousePos),_selectedBuilding,mousePos,_direction);
            }

            if (Input.GetMouseButton(1))
            {
                Vector3 mousePos =GeneralUtilities.GetMousePosition();
                GridChunk.TryToDeleteBuilding(GetChunk(mousePos),mousePos);
            }
        }

        private void FixedUpdate()
        {
            UpdateLoadedChunks();
        }

        private void OnApplicationQuit()
        {
            SaveAllChunksToFile();
        }

        #region PlayTimeLoad&UnLoadChunksSystem
        private void UpdateLoadedChunks()
        {
            Vector2Int currentPos = GetChunkPosition(PlayerCam.transform.position);
            int radius = Mathf.CeilToInt(PlayerCam.orthographicSize * PlayerCam.aspect / ChunkSize.x);
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
            targetChunk.Load();
            if(!LoadedChunks.Contains(position)) LoadedChunks.Add(position);
            yield return null;
        }

        private IEnumerator  UnLoadChunk(Vector2Int position)
        {
            GridChunk targetChunk = GetChunk(position);
            targetChunk.UnLoad();
            LoadedChunks.Remove(position);
            yield return null;
        }
        #endregion

        #region InfoFunctions
        public static Vector2Int GetChunkPosition(Vector3 worldPosition)
        {
            return new Vector2Int(Mathf.RoundToInt(worldPosition.x / ChunkSize.x),
                Mathf.RoundToInt(worldPosition.y / ChunkSize.y));
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
        private GridChunk CreateChunk(Vector2Int chunkPosition)
        {
            GridChunk newChunk = Instantiate(chunkPrefap,transform).GetComponent<GridChunk>();
            Vector3 localPosition = new Vector3(chunkPosition.x * ChunkSize.x, chunkPosition.y * ChunkSize.y);
            newChunk.Initialization(chunkPosition, localPosition);
            newChunk.gameObject.name = $"Chunk {chunkPosition}";
            Chunks.Add(chunkPosition, newChunk);
            LoadedChunks.Add(chunkPosition);
            return newChunk;
        }

        private void SaveAllChunksToFile()
        {
            ChunkSave[] chunkSaves = new ChunkSave[Chunks.Count];
            bool[] controlList = new bool[chunkSaves.Length];
            int index = 0;
            foreach (KeyValuePair<Vector2Int,GridChunk> chunk in Chunks)
            {
                StartCoroutine(JobCreateChunkSave(chunkSaves, index, chunk.Value, controlList));
                index++;
            }

            bool done = false;
            while (!done)
            {
                done = true;
                foreach (bool job in controlList)
                {
                    if (!job)
                    {
                        done = false;
                        break;
                    }
                }
            }

            WorldSaveHandler.CurrentWorldSave.chunkSaves = chunkSaves;
            WorldSaveHandler.SaveWorldToFile();
        }

        private IEnumerator JobCreateChunkSave(ChunkSave[] chunkSaves, int index, GridChunk chunk, bool[] controlList)
        {
            chunkSaves[index] = new ChunkSave(){chunkPosition = chunk.ChunkPosition, localPosition = chunk.LocalPosition, 
               chunkResourcePoints = chunk.ChunkResources.ToArray(), placedBuildingData = chunk.BuildingsData.ToArray()};
            controlList[index] = true;
            yield return null;
        }
        
        private void LoadAllChunksFromSave(WorldSave worldSave)
        {
            foreach (ChunkSave chunkSave in worldSave.chunkSaves)
            {
                StartCoroutine(LoadChunkFormSave(chunkSave));
            }
        }
        
        private IEnumerator LoadChunkFormSave(ChunkSave chunkSave)
        {
            GridChunk newChunk = Instantiate(chunkPrefap,transform).GetComponent<GridChunk>();
            newChunk.Initialization(chunkSave.chunkPosition, chunkSave.localPosition, chunkSave.placedBuildingData, chunkSave.chunkResourcePoints);
            newChunk.gameObject.name = $"Chunk {chunkSave.chunkPosition}";
            Chunks.Add(chunkSave.chunkPosition, newChunk);
            LoadedChunks.Add(chunkSave.chunkPosition);
            yield return null;
        }
        #endregion
    }
}
