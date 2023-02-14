using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Transition;
namespace MFarm.Save
{
    //把一堆背包数据啊地图数据啊npc数据全集成在一个DataSlot中
    public class DataSlot
    {
        /// <summary>
        /// 进度条,string就是GUID
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="GameSaveData"></typeparam>
        /// <returns></returns>
        public Dictionary<string, GameSaveData> dataDict = new Dictionary<string, GameSaveData>();

        #region 用UI显示进度详细
        public string DataTime{
            get{
                var key = TimeManager.Instance.GUID;

                if(dataDict.ContainsKey(key)){
                    var timeData =dataDict[key];
                    return timeData.timeDict["gameYear"]+"年/"+ (Season)timeData.timeDict["gameSeason"]+"/"+ timeData.timeDict["gameMonth"]+"月/"+timeData.timeDict["gameDay"]+"日/";
                }
                else return string.Empty;
                

            }
        }

        public string DataScene{
            get{
                var key = TransitionManager.Instance.GUID;
                if(dataDict.ContainsKey(key)){
                    var TransitionData = dataDict[key];
                    return TransitionData.dataSceneName;
                }
                else return string.Empty;
            }
            
        }

        #endregion

        
    }
}