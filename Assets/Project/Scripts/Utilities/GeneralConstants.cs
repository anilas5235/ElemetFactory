﻿using UnityEngine;

namespace Project.Scripts.Utilities
{
    public static class GeneralConstants
    {
        public static readonly Vector2Int[] NeighbourOffsets2D4 =
            { new Vector2Int(0, 1),new Vector2Int(1,0), new Vector2Int(0,-1),new Vector2Int(-1,0),};
        
        public static readonly Vector2Int[] NeighbourOffsets2D8 =
        {
            new Vector2Int(0, 1), new Vector2Int(0,-1),new Vector2Int(1,0),new Vector2Int(-1,0),
            new Vector2Int(1, 1), new Vector2Int(1,-1),new Vector2Int(-1,1),new Vector2Int(-1,-1),
        };
        
        public static readonly Vector2Int[] NeighbourOffsets2D17 =
        {
            new Vector2Int(0, 1), new Vector2Int(0,-1),new Vector2Int(1,0),new Vector2Int(-1,0),
            new Vector2Int(1, 1), new Vector2Int(1,-1),new Vector2Int(-1,1),new Vector2Int(-1,-1),
            new Vector2Int(0,2), new Vector2Int(-1,2), new Vector2Int(1,2),
            new Vector2Int(0,-2), new Vector2Int(-1,-2), new Vector2Int(1,-2),
            new Vector2Int(2,0), new Vector2Int(2,1), new Vector2Int(2,-1),
            new Vector2Int(-2,0), new Vector2Int(-2,1), new Vector2Int(-2,-1),
        };
    }
}