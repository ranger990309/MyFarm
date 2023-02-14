using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData_SO", menuName = "Map/MapData")]
public class MapData_SO : ScriptableObject
{
    [SceneName] public string sceneName;
    //这个场景的所有瓦片属性???????

    [Header("地图信息")]
    public int gridWidth;
    public int gridHeight;
    [Header("左下角原点")]
    public int originX;
    public int originY;


    public List<TileProperty> tileProperties;
}
