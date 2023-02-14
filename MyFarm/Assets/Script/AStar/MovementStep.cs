using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Coordinate协调
namespace MFarm.AStar{
    /// <summary>
    /// 这个代码是为了记录NPC将会在哪个时间走到哪个场景的哪个网格上
    /// </summary>
    public class MovementStep
    {
        public string sceneName;
        public int hour;
        public int minute;
        public int second;
        public Vector2Int gridCoordinate;
    }
}
