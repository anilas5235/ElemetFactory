using System;
using Project.Scripts.ItemSystem;
using Unity.Collections;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Grid
{
    [Serializable]
    public struct ResourcePatch
    {
        public NativeArray<int2> Positions;
        public Item Resource;
    }
    
    public struct ResourcePatchTemp
    {
        public NativeArray<int2> Positions;
        public NativeArray<uint> ResourceIDs;
    }
}