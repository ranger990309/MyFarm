using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using MFarm.Map;

namespace MFarm.CropPlant{
    public class CropGenerator : MonoBehaviour
    {
        private Grid currentGrid;
        public int seedItemID;
        public int growthDays;
        private void Awake() {
            currentGrid = FindObjectOfType<Grid>();
            //GenerateCrop();
        }
        private void OnEnable() {
            EventHandler.GenerateCropEvent += GenerateCrop;
        }

        private void OnDisable() {
            EventHandler.GenerateCropEvent -= GenerateCrop;
        }

        //生成这个位置的瓦片信息
        private void GenerateCrop(){
            Vector3Int cropGridPos = currentGrid.WorldToCell(transform.position);//网格坐标
            if(seedItemID != 0){
                var tile = GridMapManager.Instance.GetTileDetailsOnMousePosition(cropGridPos);//瓦片10大信息
                if(tile == null){
                    tile = new TileDetails();
                    tile.gridX = cropGridPos.x;
                    tile.gridY = cropGridPos.y;
                }
                tile.daysSinceWatered = -1;
                tile.seedItemID = seedItemID;
                tile.growthDays = growthDays;

                // if(tile.gridX==0 && tile.gridY==0){
                //     //Debug.Log("666" + SceneManager.GetActiveScene().name);
                // }

                GridMapManager.Instance.UpdateTileDetails(tile);
            }
        }
    }
}
