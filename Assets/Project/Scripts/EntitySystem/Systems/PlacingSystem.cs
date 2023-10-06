using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Systems
{
    public partial struct PlacingSystem : ISystem
    {
        public static PlacingSystem Instance;
        private static bool buildingEnabled = false;
        private static FacingDirection _facingDirection;
        private static int currentBuildingID;

        public void OnCreate(ref SystemState state)
        {
            Instance = this;
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!buildingEnabled)return;

            if (Input.GetKeyDown(KeyCode.R))
            {
                _facingDirection = PlacedBuildingUtility.GetNextDirectionClockwise(_facingDirection);
                Debug.Log($"rotation: {_facingDirection}");
            }
            if (Input.GetMouseButton(0))
            {
                float3 mousePos = GeneralUtilities.GetMousePosition();

                if (GenerationSystem.TryGetChunk(GenerationSystem.GetChunkPosition(mousePos),
                        out ChunkDataAspect chunkDataAspect))
                {
                    TryToPlaceBuilding(chunkDataAspect, currentBuildingID, mousePos, _facingDirection);
                }
            }

            if (Input.GetMouseButton(1))
            {
                float3 mousePos = GeneralUtilities.GetMousePosition();

                if (GenerationSystem.TryGetChunk(GenerationSystem.GetChunkPosition(mousePos),
                        out ChunkDataAspect chunkDataAspect))
                {
                    TryToDeleteBuilding(chunkDataAspect, mousePos);
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) currentBuildingID = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) currentBuildingID = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) currentBuildingID = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) currentBuildingID = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha5)) currentBuildingID = 4;
        }

        private bool TryToDeleteBuilding(ChunkDataAspect chunkDataAspect, float3 mousePos)
        {
            chunkDataAspect.GetCell(,chunkDataAspect.ChunksPosition)
        }

        public static bool TryToPlaceBuilding(ChunkDataAspect chunkDataAspect, int buildingID, float3 mousePos,
            FacingDirection facingDirection)
        {
            
        }
    }
}
