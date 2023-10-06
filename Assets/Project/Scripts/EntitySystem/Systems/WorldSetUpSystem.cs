using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.Grid;
using Unity.Collections;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Systems
{
    [ UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct WorldSetUpSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            Entity worldData = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<WorldDataComponent>(worldData);
            state.EntityManager.SetComponentData(worldData, new WorldDataComponent()
            {
                entity = worldData,
                ChunkDataBank = new NativeList<PositionChunkPair>(Allocator.Persistent),
            });
            state.EntityManager.SetName(worldData,"WorldData");
            GenerationSystem.worldDataEntity = worldData;
            state.RequireForUpdate<PrefabsDataComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
        }
    }
}
