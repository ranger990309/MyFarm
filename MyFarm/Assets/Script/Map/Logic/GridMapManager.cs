
using System.Runtime.ExceptionServices;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using MFarm.CropPlant;
using MFarm.Save;

namespace MFarm.Map{
    public class GridMapManager : Singleton<GridMapManager>,ISaveable
    {
        [Header("种地瓦片切换信息")]
        /// <summary>
        /// 挖地智能瓦片
        /// </summary>
        public RuleTile digTile;
        /// <summary>
        /// 浇水智能瓦片
        /// </summary>
        public RuleTile waterTile;
        private Tilemap digTilemap;
        private Tilemap waterTilemap;

        [Header("地图信息")]
        public List<MapData_SO> mapDataList;//两个场景的所有瓦片信息

        /// <summary>
        /// 当前的季节
        /// </summary>
        private Season currentSeason;

        /// <summary>
        /// 场景名字+坐标和对应的瓦片信息
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="TileDetails"></typeparam>
        /// <returns></returns>
        public Dictionary<string, TileDetails> tileDetailsDict = new Dictionary<string, TileDetails>();

        //场景第一次加载
        private Dictionary<string, bool> firstLoadDict = new Dictionary<string, bool>();

        /// <summary>
        /// 范围内的所有物品中的草
        /// </summary>
        private List<ReapItem> itemsInRadius;

        /// <summary>
        /// 当前场景的大网格
        /// </summary>
        private Grid currentGrid;

        public string GUID => GetComponent<DataGUID>().guid;

        private void OnEnable() {
            EventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
            EventHandler.RefreshCurrentMap += RefreshMap;
        }

        private void OnDisable() {
            EventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
            EventHandler.RefreshCurrentMap -= RefreshMap;
        }

        private void Start() {
            
            ISaveable saveable = this;
            saveable.RegisterSaveable();
    
            foreach(var mapData in mapDataList){
                firstLoadDict.Add(mapData.sceneName, true);//?????????
                InitTileDetailsDict(mapData);
            }
        }

        private void OnRefreshCurrentMap()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 每天都更新一下田地作物
        /// </summary>
        /// <param name="day"></param>
        /// <param name="season"></param>
        private void OnGameDayEvent(int day, Season season)
        {
            currentSeason = season;
            //大于-1说明昨天浇过水了,今天还没浇水,要改回-1
            foreach(var tile in tileDetailsDict){
                if(tile.Value.daysSinceWatered>-1){
                    tile.Value.daysSinceWatered = -1;
                }
                //每过一天已经挖的地就加1,到了第五天坑里还没作物的话就恢复未挖坑的土地
                if(tile.Value.daysSinceDug>-1){
                    tile.Value.daysSinceDug++;
                }
                if(tile.Value.daysSinceDug>5 && tile.Value.seedItemID == -1){
                    tile.Value.daysSinceDug = -1;
                    tile.Value.canDig = true;
                    tile.Value.growthDays = -1;
                }
                if(tile.Value.seedItemID != -1){ //有作物的情况下
                    tile.Value.growthDays++;
                }
            }
            RefreshMap();
        }

        private void OnAfterSceneLoadedEvent()//这是另一个方法
        {
            //获取当前场景的大网格
            currentGrid = FindObjectOfType<Grid>();
            digTilemap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();
            waterTilemap = GameObject.FindWithTag("Water").GetComponent<Tilemap>();
            //DisplayMap(SceneManager.GetActiveScene().name);
            if(firstLoadDict[SceneManager.GetActiveScene().name]){
                //预先生成农作物
                EventHandler.CallGenerateCropEvent();
                firstLoadDict[SceneManager.GetActiveScene().name] = false;
            }
            RefreshMap();
        }

        /// <summary>
        ///创建一个包含所有瓦格坐标是否可挖掘的字典
        /// </summary>
        /// <param name="mapData"></param>
        private void InitTileDetailsDict(MapData_SO mapData){
            foreach(TileProperty tileProperty in mapData.tileProperties){
                TileDetails tileDetails = new TileDetails
                {
                    gridX = tileProperty.tileCoordinate.x,
                    gridY = tileProperty.tileCoordinate.y
                };
                //字典的key
                string key = tileDetails.gridX + "X" + tileDetails.gridY + "Y" + mapData.sceneName;
                //??????????
                if(GetTileDetails(key) != null){
                    tileDetails = GetTileDetails(key);
                }

                switch(tileProperty.gridType){
                    case GridType.Diggable:
                        tileDetails.canDig = tileProperty.booltypeValue;
                        break;
                    case GridType.DropItem:
                        tileDetails.canDropItem = tileProperty.booltypeValue;
                        break;
                    case GridType.NPCObstacle:
                        tileDetails.isNPCObstacle = tileProperty.booltypeValue;
                        break;
                    case GridType.PlaceFurniture:
                        tileDetails.canPlaceFurniture = tileProperty.booltypeValue;
                        break;
                }

                if(GetTileDetails(key) != null){
                    tileDetailsDict[key] = tileDetails;
                }else{
                    tileDetailsDict.Add(key, tileDetails);
                }
            }
        }

        /// <summary>
        /// 利用key找出TileDetails瓦片信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TileDetails GetTileDetails(string key){
            if(tileDetailsDict.ContainsKey(key)){
                return tileDetailsDict[key];
            }
            return null;
        }

        /// <summary>
        /// 根据鼠标网格坐标返回瓦片信息(坐标,可浇水等等)
        /// </summary>
        /// <param name="mouseGridPos"></param>
        /// <returns></returns>
        public TileDetails GetTileDetailsOnMousePosition(Vector3Int mouseGridPos){
            // if(mouseGridPos.x==0 && mouseGridPos.y==0){
            //         Debug.Log("666" + SceneManager.GetActiveScene().name);
            //     }
            string key = mouseGridPos.x + "X" + mouseGridPos.y + "Y" + SceneManager.GetActiveScene().name;
            return GetTileDetails(key);
        }

        /// <summary>
        /// 播放完动作动画后直接开始行动
        /// </summary>
        /// <param name="mouseWorldPos"></param>
        /// <param name="itemDetails"></param>
        private void OnExecuteActionAfterAnimation(Vector3 mouseWorldPos, ItemDetails itemDetails)
        {
            //得到鼠标的网格坐标
            var mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);
            //得到这个瓦片的是否可挖掘等10大信息
            var currentTile = GetTileDetailsOnMousePosition(mouseGridPos);
            
            //Debug.Log(mouseGridPos.x + " " + mouseGridPos.y);
            // foreach(var i in tileDetailsDict){
            //     Debug.Log(i.Key+" "+i.Value);
            // }

            //生成背包所选物品在地面
            if(currentTile != null){
                Crop currentCrop = GetCropObject(mouseWorldPos);
                switch(itemDetails.itemType){
                    case ItemType.seed:
                        EventHandler.CallplantSeedEvent(itemDetails.itemID, currentTile);
                        EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos, itemDetails.itemType);
                        EventHandler.CallPlaySoundEvent(SoundName.Plant);//播放种种子的音效
                        break;
                    case ItemType.Commodity:
                        //Debug.Log("1");
                        EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos, itemDetails.itemType);
                        break;
                    case ItemType.HoeTool:
                        //给所点击的瓦片加个挖地瓦片
                        SetDigGround(currentTile);
                        //从挖下那一刻开始算时间,到一定天数挖的地会回归自然
                        currentTile.daysSinceDug = 0;
                        currentTile.canDig = false;
                        currentTile.canDropItem = false;
                        EventHandler.CallPlaySoundEvent(SoundName.Hoe);//音效
                        break;
                    case ItemType.WaterTool:
                        SetWaterGround(currentTile);
                        currentTile.daysSinceWatered = 0;
                        EventHandler.CallPlaySoundEvent(SoundName.Water);//音效
                        break;
                    case ItemType.breakTool:
                    case ItemType.ChopTool:
                        //执行收割方法
                        currentCrop?.ProcessToolAction(itemDetails, currentCrop.tileDetails);
                        break;
                    case ItemType.CollectTool:
                        //执行收割方法
                        currentCrop?.ProcessToolAction(itemDetails, currentTile);
                        break;
                    case ItemType.ReapTool:
                        var reapCount = 0;
                        for (int i = 0; i < itemsInRadius.Count;i++){
                            EventHandler.CallParticleEffectEvent(ParticaleEffectType.ReapableScenery, itemsInRadius[i].transform.position + Vector3.up);//播放特效
                            itemsInRadius[i].SpawnHarvestItems();
                            Destroy(itemsInRadius[i].gameObject);
                            reapCount++;
                            if(reapCount >= Settings.reapAmount)//一次割草不能超过限制
                                break;
                        }
                        EventHandler.CallPlaySoundEvent(SoundName.Reap);
                        break;
                    case ItemType.Furniture:
                        //在地图生成物品
                        //移除当前物品(蓝图)
                        //移除资源物品
                        EventHandler.CallBuildFurnitureEvent(itemDetails.itemID, mouseWorldPos);
                        break;
                }
                UpdateTileDetails(currentTile);
            }
        }

        /// <summary>
        /// 通过鼠标点按的位置得到农作物Crop
        /// </summary>
        /// <param name="mouseWorldPos"></param>
        /// <returns></returns>
        public Crop GetCropObject(Vector3 mouseWorldPos){
            Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);
            Crop currentCrop = null;
            for (int i = 0; i < colliders.Length; i++)
            {
                if(colliders[i].GetComponent<Crop>()){
                    currentCrop = colliders[i].GetComponent<Crop>();
                }
            }
            return currentCrop;
        }

        /// <summary>
        /// 返回工具范围内的杂草
        /// </summary>
        /// <param name="锄头等工具"></param>
        /// <returns></returns>
        public bool HaveReapableItemInRadius(Vector3 mouseWorldPos,ItemDetails tool){
            itemsInRadius = new List<ReapItem>();//范围内的所有物品中的草

            Collider2D[] colliders = new Collider2D[20];//范围内的所有物品

            //鼠标范围,锄头等工具范围,可以找到范围内很多的草碰撞体
            Physics2D.OverlapCircleNonAlloc(mouseWorldPos, tool.itemUseRadius, colliders);//???????????????

            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if(colliders[i] != null){
                        if (colliders[i].GetComponent<ReapItem>())
                        {
                            var item = colliders[i].GetComponent<ReapItem>();
                            itemsInRadius.Add(item);
                        }
                    }
                }
            }
            return itemsInRadius.Count > 0;
        }

        /// <summary>
        /// 显示挖坑瓦片
        /// </summary>
        /// <param name="鼠标所点击的瓦片"></param>
        private void SetDigGround(TileDetails tile){
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if(digTilemap!=null){
                digTilemap.SetTile(pos, digTile);
            }
        }

        /// <summary>
        /// 显示浇水瓦片
        /// </summary>
        /// <param name="tile"></param>
        private void SetWaterGround(TileDetails tile){
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if(waterTilemap!=null){
                waterTilemap.SetTile(pos, waterTile);
            }
        }

        /// <summary>
        /// 更新瓦片信息
        /// </summary>
        /// <param name="tileDetails"></param>
        public void UpdateTileDetails(TileDetails tileDetails){
            string key = tileDetails.gridX + "X" + tileDetails.gridY + "Y" + SceneManager.GetActiveScene().name;
            if(tileDetailsDict.ContainsKey(key)){
                tileDetailsDict[key] = tileDetails;
            }else{
                tileDetailsDict.Add(key, tileDetails);
            }
        }

        /// <summary>
        /// 先删除坑和水作物再刷新一遍地图
        /// </summary>
        private void RefreshMap(){
            if(digTilemap != null){
                digTilemap.ClearAllTiles();
            }
            if(waterTilemap != null){
                waterTilemap.ClearAllTiles();
            }
            foreach(var crop in FindObjectsOfType<Crop>()){
                Destroy(crop.gameObject);
            }
            DisplayMap(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// 更新地图显示坑和水地
        /// </summary>
        /// <param name="sceneName"></param>
        private void DisplayMap(string sceneName){
            foreach(var tile in tileDetailsDict){
                var key = tile.Key;
                var tileDetails = tile.Value;
                if(key.Contains(sceneName)){
                    if(tileDetails.daysSinceDug>-1){
                        SetDigGround(tileDetails);
                    }
                    if(tileDetails.daysSinceWatered>-1){
                        SetWaterGround(tileDetails);
                    }
                    if(tileDetails.seedItemID > -1){
                        EventHandler.CallplantSeedEvent(tileDetails.seedItemID, tileDetails);//显示作物
                    }
                }
            }
        }

        /// <summary>
        /// 根据场景名字构建在网格范围,输出范围和原点
        /// </summary>
        /// <param name="sceneName">场景名字</param>
        /// <param name="gridDimensions">网格范围</param>
        /// <param name="gridOrigin">网格原点</param>
        /// <returns>是否有当前场景的信息</returns>
        public bool GetGridDimensions(string sceneName,out Vector2Int gridDimensions,out Vector2Int gridOrigin){//?????????????????????
            gridDimensions = Vector2Int.zero;
            gridOrigin = Vector2Int.zero;

            foreach(var mapData in mapDataList){
                if(mapData.sceneName == sceneName){
                    gridDimensions.x = mapData.gridWidth;
                    gridDimensions.y = mapData.gridHeight;

                    gridOrigin.x = mapData.originX;
                    gridOrigin.y = mapData.originY;

                    return true;
                }
            }
            return false;
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.tileDetailsDict = this.tileDetailsDict;
            saveData.firstLoadDict = this.firstLoadDict;
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            this.tileDetailsDict = saveData.tileDetailsDict;
            this.firstLoadDict = saveData.firstLoadDict;
        }
    }
}

