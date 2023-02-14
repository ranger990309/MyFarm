using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

//在编辑模式下运行
[ExecuteInEditMode]
public class GridMap : MonoBehaviour
{//挂载在地图每一张瓦片上

    public MapData_SO mapData;
    public GridType gridType;
    /// <summary>
    /// 得到可挖掘的瓦片地图
    /// </summary>
    private Tilemap currentTilemap;
    private void OnEnable()
    {
        //检测附着这个代码的不在运行才能继续
        if (!Application.IsPlaying(this))
        {
            currentTilemap = GetComponent<Tilemap>();
            //开始编辑地图时先清空地图数据,下面结束编辑地图再将数据重新导入
            if (mapData != null){
                mapData.tileProperties.Clear();
            }
        }
    }

    private void OnDisable()
    {
        //检测附着这个代码的不在运行才能继续
        if (!Application.IsPlaying(this))
        {
            currentTilemap = GetComponent<Tilemap>();
            UpdateTileProperties();
            //这段只有在编辑器中才会运行
#if UNITY_EDITOR
            //这样才能实时保存和更新????????????
            if(mapData != null){
                EditorUtility.SetDirty(mapData);
            }
#endif
        }
    }

    /// <summary>
    /// 将地图数据加载进去
    /// </summary>
    private void UpdateTileProperties(){
        //获取现有真是有效的压缩地图
        currentTilemap.CompressBounds();
        //在这个代码的附着物没有运行的情况下
        if(!Application.IsPlaying(this)){
            if(mapData!=null){
                //已绘制范围的左下角坐标
                Vector3Int startPos = currentTilemap.cellBounds.min;
                //已绘制范围的右上角坐标
                Vector3Int endPos = currentTilemap.cellBounds.max;
                //拿到大部分瓦片
                for (int x = startPos.x; x < endPos.x;x++){
                    for (int y = startPos.y; y < endPos.y;y++){
                        //得到一个基础瓦片
                        TileBase tile = currentTilemap.GetTile(new Vector3Int(x, y, 0));
                        if(tile != null){
                            //瓦片基础信息
                            TileProperty newTile = new TileProperty
                            {
                                tileCoordinate = new Vector2Int(x, y),
                                gridType = this.gridType,
                                booltypeValue = true
                            };
                            //将其加入mapData
                            mapData.tileProperties.Add(newTile);

                        }
                    }
                }
            }
        }
    }
}
