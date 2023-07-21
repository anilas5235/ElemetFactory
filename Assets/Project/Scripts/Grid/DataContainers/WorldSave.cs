using System;

namespace Project.Scripts.Grid.DataContainers
{
    [Serializable]
    public class WorldSave 
    {
        public ChunkSave[] chunkSaves = Array.Empty<ChunkSave>();
    }
}
