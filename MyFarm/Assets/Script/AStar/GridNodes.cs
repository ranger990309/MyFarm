using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.AStar{
    public class GridNodes
    {
        private int width;
        private int height;
        private Node[,] gridNode;//一个起始点为(0,0)的 宽高的 网格

        /// <summary>
        /// 构建函数初始化节点范围数组
        /// </summary>
        /// <param name="width">地图宽度</param>
        /// <param name="height">地图高度</param>
        public GridNodes(int width,int height){
            this.width = width;
            this.height = height;

            gridNode = new Node[width, height];

            for (int x = 0; x < width;x++){
                for (int y = 0; y < height;y++){
                    gridNode[x, y] = new Node(new Vector2Int(x, y));
                }
            }
        }

        /// <summary>
        /// 在大格子中找某一个小格子
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns></returns>
        public Node GetGridNode(int xPos,int yPos){
            if (xPos < width && yPos < height)
            {
                return gridNode[xPos, yPos];
            }
            Debug.Log("超出网格范围");
            return null;
        }
    }
}
