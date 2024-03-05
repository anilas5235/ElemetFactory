using System.Linq;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Flags;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.EntitySystem.Systems;
using Project.Scripts.Utilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Project.Scripts.EntitySystem.Others
{
    public class GenerationOfChunkData 
    {
        private readonly WorldDataAspect _worldDataAspect;
        private NativeArray<ChunkGenerationRequestBuffElement> _requests;
        private Random _randomGenerator;
        private NativeList<ChunkGenTempData> _generatedChunks;
        private EntityCommandBuffer _ecb;
        private readonly Entity _tilePrefab;

        private static float WorldScale => GenerationSystem.WorldScale;
        
        #region Consts

        public static readonly float[] ResourcePatchSizeProbabilities = { 60f, 39f, 1f };
        public static readonly float[] ChunkResourceNumberProbabilities = { 70f, 25f, 5f };

        public static readonly NativeArray<int2> Patch0Positions = new(new int2[] { new(0, 0) }, Allocator.Persistent);

        public static readonly NativeArray<int2> Patch1Positions = new(
            new int2[] { new(0, 1), new(1, 1), new(1, 0) }, Allocator.Persistent);

        public static readonly NativeArray<int2> Patch2Positions = new(new int2[]
        {
            new(-1, 1), new(-1, 0), new(-1, -1), new(0, -1),
            new(1, -1),
        }, Allocator.Persistent);

        public static readonly NativeArray<int2> Patch3Positions = new(new int2[]
        {
            new(-2, 0), new(-2, 1), new(-2, -1), new(2, -1),
            new(2, 1), new(2, 0), new(-1, -2), new(1, -2),
            new(0, -2), new(-1, 2), new(1, 2), new(0, 2),
        }, Allocator.Persistent);
        #endregion

        public GenerationOfChunkData(WorldDataAspect worldDataAspect, NativeArray<ChunkGenerationRequestBuffElement> requests, Random randomGenerator, EntityCommandBuffer ecb, Entity tilePrefab)
        {
            _worldDataAspect = worldDataAspect;
            _requests = requests;
            _randomGenerator = randomGenerator;
            _generatedChunks = new NativeList<ChunkGenTempData>(Allocator.TempJob);
            _ecb = ecb;
            _tilePrefab = tilePrefab;
        }

        public void Execute()
        {
            foreach (var request in _requests)
            {
                if (CheckIfChunkExitsWhileGenerating(request.ChunkPosition)) { continue; }

                var chunkData = GenerateChunkGenTempData(request.ChunkPosition);
                
                _generatedChunks.Add(chunkData);
                GenerateChunk(chunkData);
            }

            _generatedChunks.Dispose();
        }

        private ChunkGenTempData GenerateChunkGenTempData(int2 chunkPosition)
        {
            return new ChunkGenTempData(chunkPosition,GenerateResources(chunkPosition));
        }
        
        private void GenerateChunk(ChunkGenTempData chunkGenTempData)
        {
            var entity = _ecb.CreateEntity();
            var chunkPosition = chunkGenTempData.position;
            var worldPos = GenerationSystem.GetChunkWorldPosition(chunkPosition);
            _ecb.SetName(entity, $"Ch({chunkPosition.x},{chunkPosition.y})");
            _ecb.AddComponent(entity, new LocalTransform()
            {
                Position = worldPos,
                Scale = WorldScale,
            });
            var patches = new NativeArray<ResourcePatch>(chunkGenTempData.patches.Length,Allocator.Temp);

            for (var index = 0; index < chunkGenTempData.patches.Length; index++)
            {
                var tempData = chunkGenTempData.patches[index];
                patches[index] = new ResourcePatch()
                {
                    Positions = tempData.Positions,
                    ItemID = tempData.ItemID,
                };
            }

            _ecb.AddComponent(entity, new ChunkDataComponent(entity, chunkPosition, worldPos,
                _tilePrefab,patches, _ecb));
            patches.Dispose();
            _ecb.AddComponent(entity,new NewChunkDataComponent(chunkGenTempData.position,chunkGenTempData.patches.Length));
        }

        #region GenerationSteps

        private int GetRandom(float distanceToCenter)
        {
            var pool = 1;
            if (distanceToCenter >= 2f) pool += 3;
            if (distanceToCenter >= 5f) pool += 1;
            return _randomGenerator.NextInt(1, pool);
        }
        private float INT2Length(int2 vec)
        {
            return math.sqrt(vec.x * vec.x + vec.y * vec.y);
        }

        #region PatchGeneration
        private ResourcePatch GenerateResourcePatch(int patchSize, int resourceID, NativeList<int2> blocked)
        {
            using var cellPositions = GeneratePatchShape(patchSize, blocked);

            blocked.AddRange(cellPositions.AsArray());

            return new ResourcePatch()
            {
                Positions = new NativeArray<int2>(cellPositions.AsArray(), Allocator.Temp),
                ItemID = resourceID,
            };
        }
        private int GetPatchSize(int numberOfPatches = 1)
        {
            var returnVal = 1;
            var random = _randomGenerator.NextFloat(0f, 100f) - ((numberOfPatches - 1) * 20);
            var currentStep = 0f;
            for (var i = 0; i < ResourcePatchSizeProbabilities.Length; i++)
            {
                if (ResourcePatchSizeProbabilities[i] == 0) continue;
                currentStep += ResourcePatchSizeProbabilities[i];
                if (random > currentStep) continue;
                returnVal += i;
                break;
            }

            return returnVal;
        }
        
        private NativeList<int2> GeneratePatchShape(int patchSize, NativeList<int2> blocked)
        {

            GetPathBaseShape(patchSize, out var cellPositions, out var outerCells);

            var minAndMaxCellCount = new int2((int)(outerCells.Length / 2f + cellPositions.Length),
                (int)(cellPositions.Length * 2f));
            int2 center;
            do
            {
                center = new int2(_randomGenerator.NextInt(patchSize + 1, ChunkDataComponent.ChunkSize - patchSize - 1),
                    _randomGenerator.NextInt(patchSize + 1, ChunkDataComponent.ChunkSize - patchSize - 1));
            } while (blocked.Contains(center));

            for (var i = 0; i < cellPositions.Length; i++) cellPositions[i] += center;
            for (var i = 0; i < outerCells.Length; i++) outerCells[i] += center;

            var done = false;
            var emptyIterations = 0;

            do
            {
                using NativeList<int2> removeList = new(Allocator.TempJob);
                using NativeList<int2> addList = new(Allocator.TempJob);
                foreach (var outerCell in outerCells)
                {
                    if( cellPositions.Length + addList.Length >= minAndMaxCellCount.y){break;}
                        
                    if (blocked.Contains(outerCell))
                    {
                        removeList.Add(outerCell);
                        continue;
                    }

                    foreach (var neighbourOffset in GeneralConstants.NeighbourOffsets2D4)
                    {
                        if (cellPositions.Length + addList.Length >= minAndMaxCellCount.y) break;
                        var newCell = outerCell + new int2(neighbourOffset.x, neighbourOffset.y);
                        if (cellPositions.Contains(newCell) || addList.Contains(newCell) ||
                            blocked.Contains(newCell + center) ||
                            !ChunkDataAspect.IsValidPositionInChunk(newCell)) continue;
                        var prob = 1f / (INT2Length(newCell) + .5f) * 4f;
                        if (_randomGenerator.NextFloat(0f, 100f) / 100f >= prob) continue;
                        if (!removeList.Contains(outerCell)) removeList.Add(outerCell);
                        addList.Add(newCell);
                    }
                }

                foreach (var t in removeList)
                {
                    for (var i = 0; i < outerCells.Length; i++)
                    {
                        var condition = outerCells[i] == t;
                        if (condition is { x: true, y: true })
                        {
                            outerCells.RemoveAt(i);
                        }
                    }
                }

                foreach (var t in addList)
                {
                    outerCells.Add(t);
                    cellPositions.Add(t);
                }

                if (addList.Length < 1) emptyIterations++;

                if (cellPositions.Length > minAndMaxCellCount.x || emptyIterations >= 20) done = true;

            } while (!done);

            return cellPositions;
        }

        private static void GetPathBaseShape(int patchSize, out NativeList<int2> cellPositions,
            out NativeList<int2> outerCells)
        {
            cellPositions = new NativeList<int2>(Allocator.Temp);
            outerCells = new NativeList<int2>(Allocator.Temp);

            switch (patchSize)
            {
                case 1:
                    cellPositions.AddRange(Patch0Positions);
                    cellPositions.AddRange(Patch1Positions);
                    outerCells.AddRange(cellPositions.AsArray());
                    break;
                case 2:
                    cellPositions.AddRange(Patch0Positions);
                    cellPositions.AddRange(Patch1Positions);
                    cellPositions.AddRange(Patch2Positions);
                    outerCells.AddRange(Patch1Positions);
                    outerCells.AddRange(Patch2Positions);
                    break;
                case 3:
                    cellPositions.AddRange(Patch0Positions);
                    cellPositions.AddRange(Patch1Positions);
                    cellPositions.AddRange(Patch2Positions);
                    cellPositions.AddRange(Patch3Positions);
                    outerCells.AddRange(Patch3Positions);
                    break;
            }
        }
        #endregion
        private NativeArray<ResourcePatch> GenerateResources(int2 chunkPosition)
        {
            var distToCenter = math.sqrt(chunkPosition.x * chunkPosition.x + chunkPosition.y * chunkPosition.y);
            var numberOfPatches = GetNumberOfChunkResources(7f, chunkPosition);
            if (distToCenter < 2f || numberOfPatches < 1) return new NativeArray<ResourcePatch>();
            var resourcePatches = new NativeArray<ResourcePatch>(numberOfPatches, Allocator.Temp);
            var chunkResources = new NativeArray<int>(numberOfPatches, Allocator.Temp);
            var blockPositions = new NativeList<int2>(Allocator.Temp);

            for (var i = 0; i < numberOfPatches; i++)
            {
                bool done;
                do
                {
                    var type = GetRandom(distToCenter);
                    done = chunkResources.All(resource => type != resource);

                    if (!done) continue;
                    chunkResources[i] = type;
                    resourcePatches[i] = GenerateResourcePatch(GetPatchSize(numberOfPatches), type, blockPositions);
                } while (!done);
            }

            return resourcePatches;
        }
        private int GetNumberOfChunkResources(float antiCrowdingMultiplier, int2 chunkPosition)
        {
            var returnVal = 0;
            var randomNum = _randomGenerator.NextFloat(0f, 100f);

            foreach (var neighbourOffset in GeneralConstants.NeighbourOffsets2D8)
            {
                var chunkPos = new int2(neighbourOffset.x, neighbourOffset.y) + chunkPosition;
                if (TryGetChunkWhileGenerating(chunkPos, out var chunk))
                {
                    randomNum -= chunk.NumPatches * antiCrowdingMultiplier;
                }
            }

            var currentStep = 0f;
            for (var i = 0; i < ChunkResourceNumberProbabilities.Length; i++)
            {
                if (ChunkResourceNumberProbabilities[i] == 0) continue;
                currentStep += ChunkResourceNumberProbabilities[i];
                if (randomNum > currentStep) continue;
                returnVal += i;
                break;
            }

            return returnVal;
        }

        private bool TryGetChunkWhileGenerating(int2 chunkPosition, out ChunkGenTempData chunk)
        {
            chunk = default;
            if (_worldDataAspect.TryGetPositionChunkPair(chunkPosition, out var pair))
            {
                chunk = new ChunkGenTempData(chunkPosition, new NativeArray<ResourcePatch>(pair.NumOfPatches, Allocator.Temp));
                return true;
            }

            foreach (var chunkData in _generatedChunks)
            {
                if (chunkData.position.x != chunkPosition.x || chunkData.position.y != chunkPosition.y){continue;}
                chunk = chunkData;
                return true;
            }

            return false;
        }

        private bool CheckIfChunkExitsWhileGenerating(int2 chunkPosition)
        {
            if (_worldDataAspect.ChunkExits(chunkPosition)){ return true;}

            foreach (var chunkData in _generatedChunks)
            {
                var condition = chunkData.position == chunkPosition;
                if (condition is not { x: true, y: true }) continue;
                return true;
            }

            return false;
        }
        #endregion
    }
}