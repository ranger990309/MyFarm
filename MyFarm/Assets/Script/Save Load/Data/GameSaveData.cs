
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.Save{
    [System.Serializable]
    public class GameSaveData
    {
        public string dataSceneName;//场景名字
        /// <summary>
        /// 存储人物坐标,string人物名字
        /// </summary>
        public Dictionary<string, SerializableVector3> charcacterPosDict;
        /// <summary>
        /// 场景名字-物品树草列表
        /// </summary>
        public Dictionary<string, List<SceneItem>> sceneItemDict;
        /// <summary>
        /// 场景名字-家具列表
        /// </summary>
        public Dictionary<string, List<SceneFurniture>> sceneFurnitureDict;
        /// <summary>
        /// 场景名字+坐标和对应的瓦片信息
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="TileDetails"></typeparam>
        /// <returns></returns>
        public Dictionary<string, TileDetails> tileDetailsDict;
        /// <summary>
        /// 场景第一次加载
        /// </summary>
        public Dictionary<string, bool> firstLoadDict;
        public Dictionary<string, List<InventoryItem>> inventoryDict;//背包
        public Dictionary<string, int> timeDict;
        public int playerMoney;

        //NPC
        public string targetScene;//NPC目标场景
        public bool interactable;//NPC是否可互动
        public int animationInstanceID;//动画片段,比如NPC这时在伸懒腰
    }

}
