
using UnityEngine;

namespace MFarm.CropPlant{
    public class CropManager : Singleton<CropManager>
    {
        /// <summary>
        /// 多个种子
        /// </summary>
        public CropDataList_SO cropData;
        /// <summary>
        /// 地里所有的种子都会生成在这里
        /// </summary>
        private Transform cropParent;
        private Grid currentGrid;//??????????????
        private Season currentSeason;

        private void OnEnable() {
            EventHandler.plantSeedEvent += OnPlantSeedEvent;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
        }

        private void OnDisable() {
            EventHandler.plantSeedEvent -= OnPlantSeedEvent;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
        }

        /// <summary>
        /// 及时更新季节
        /// </summary>
        /// <param name="day"></param>
        /// <param name="season"></param>
        private void OnGameDayEvent(int day, Season season)
        {
            currentSeason = season;
        }

        /// <summary>
        /// 换场景更新土地和种子母体
        /// </summary>
        private void OnAfterSceneLoadedEvent()
        {
            currentGrid = FindObjectOfType<Grid>();
            cropParent = GameObject.FindWithTag("CropParent").transform;
        }

        /// <summary>
        /// 第一次种植显示种植或已有作物显示
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="tileDetails"></param>
        private void OnPlantSeedEvent(int ID, TileDetails tileDetails)
        {
            CropDetails currentCrop = GetCropDetails(ID);//通过ID去找种子
            if(currentCrop != null && SeasonAvailable(currentCrop) && tileDetails.seedItemID == -1){ //用于第一次种植(看低有无种子季节对不对)
                tileDetails.seedItemID = ID;
                tileDetails.growthDays = 0;
                //显示农作物
                DisplayCropPlant(tileDetails, currentCrop);
            }else if(tileDetails.seedItemID != -1){ //用于刷新地图(地里已经有种子)
                //显示农作物
                DisplayCropPlant(tileDetails, currentCrop);
            }
        }

        /// <summary>
        /// 显示农作物
        /// </summary>
        /// <param name="tileDetails"></param>
        /// <param name="cropDetails"></param>
        private void DisplayCropPlant(TileDetails tileDetails,CropDetails cropDetails){
            //成长阶段
            int growthStages = cropDetails.growthDays.Length;//有几个成长阶段(土豆5个)
            int currentStage = 0;//现在处于哪个阶段
            int dayCounter = cropDetails.TotalGrowthDays;//完全长成需要多久

            //倒序计算当前的成长阶段,还有多少天可以完全长大
            for (int i = growthStages - 1; i >= 0;i--){
                if(tileDetails.growthDays >= dayCounter){
                    currentStage = i;
                    break;
                }
                dayCounter -= cropDetails.growthDays[i];
            }

            //生成作物在网格中间
            GameObject cropPrefab = cropDetails.growthPrefabs[currentStage];
            Sprite cropSprite = cropDetails.growthSprites[currentStage];
            //要加上0.5才是网格中间
            Vector3 pos = new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f, 0);
            GameObject cropInstance = Instantiate(cropPrefab, pos, Quaternion.identity, cropParent);
            cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;
            //给生成的作物附上一些10大信息
            cropInstance.GetComponent<Crop>().cropDetails = cropDetails;
            cropInstance.GetComponent<Crop>().tileDetails = tileDetails;


        }

        /// <summary>
        /// 通过ID去找种子
        /// </summary>
        /// <param name="种子ID"></param>
        /// <returns></returns>
        public CropDetails GetCropDetails(int ID){
            return cropData.cropDatailsList.Find(c => (c.seedItemID == ID));
        }

        /// <summary>
        /// 判断种子在现在这个季节是否适合种植
        /// </summary>
        /// <param name="crop"></param>
        /// <returns></returns>
        private bool SeasonAvailable(CropDetails crop){
            for (int i = 0; i < crop.seasons.Length;i++){
                if(crop.seasons[i] == currentSeason){
                    return true;
                }
            }
            return false;
        }
    }   
}

