using System.IO;
using Debug = UnityEngine.Debug;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Save;

namespace MFarm.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>,ISaveable
    {
        [Header("物品数据")]
        public ItemDataList_SO itemDataList_SO;
        [Header("建造蓝图")]
        public BluePrintDataList_SO bluePrintData;
        [Header("背包数据")]
        public InventoryBag_SO playerBagTemp;//背包
        public InventoryBag_SO playerBag;
        private InventoryBag_SO currentBoxBag;
        [Header("交易")]
        public int playerMoney;
        /// <summary>
        /// 箱子名字+箱子物品列表
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<InventoryItem>> boxDataDict = new Dictionary<string, List<InventoryItem>>();
        //这个游戏的箱子数量
        public int BoxDataAmount => boxDataDict.Count;

        public string GUID => GetComponent<DataGUID>().guid;

        private void OnEnable() {
            EventHandler.DropItemEvent += OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
            //建造
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void OnDisable() {
            EventHandler.DropItemEvent -= OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }

        private void OnStartNewGameEvent(int obj)
        {
            playerBag = Instantiate(playerBagTemp);
            playerMoney = Settings.playerStartMoney;//新游戏背包金钱默认100
            boxDataDict.Clear();//新游戏背包清空
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bag_SO)
        {
            currentBoxBag = bag_SO;
        }

        private void OnBuildFurnitureEvent(int ID, Vector3 mouse)
        {
            //选择建造后删除蓝图
            RemoveItem(ID, 1);
            BluePrintDatails bluePrint = bluePrintData.GetBluePrintDatails(ID);
            foreach(var item in bluePrint.resourceItem){
                RemoveItem(item.itemID, item.itemAmount);//删除所需建材
            }
        }

        private void OnHarvestAtPlayerPosition(int ID)
        {
            //背包是否已经有该农作物,有就加1
            var index = GetItemIndexBag(ID);
            AddItemAtItem(ID, index, 1);
        }

        private void OnDropItemEvent(int ID, Vector3 pos,ItemType itemType)
        {
            RemoveItem(ID, 1);
        }

        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();//将这一份ISaveable代码加入到List<ISaveable>中
            //EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        /// <summary>
        /// 通过ID找物品数据
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ItemDetails GetItemDetails(int ID)
        {
            return itemDataList_SO.itemDetailsList.Find(i => i.itemID == ID);
        }
        /// <summary>
        /// 背包加入物品
        /// </summary>
        /// <param name="item"></param>
        /// <param name="toDestory"></param>
        public void AddItem(Item item, bool toDestory)
        {
            //检测是否已有物品,有则返回物品在背包的位置,没有则返回-1
            int index = GetItemIndexBag(item.itemID);
            //在背包中添加物品有就加1没有新建
            AddItemAtItem(item.itemID, index, 1);

            if (toDestory)
            {
                Destroy(item.gameObject);
            }
            //一旦有新物品进入背包系统,那就给UpdateInventoryUI事件注入值,首先其是player的背包下栏,然后是
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
        /// <summary>
        /// 背包是否有空位
        /// </summary>
        public bool CheckBagCapcity()
        {
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 通过物品id找其在背包的位置,若背包没有则返回-1
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private int GetItemIndexBag(int ID)
        {
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == ID)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 在背包中添加物品有就加1没有新建
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        private void AddItemAtItem(int ID, int index, int amount)
        {
            if (index == -1 && CheckBagCapcity())
            {//背包没有这个物品,同时有空位
                var item = new InventoryItem { itemID = ID, itemAmount = amount };
                for (int i = 0; i < playerBag.itemList.Count; i++)
                {
                    if (playerBag.itemList[i].itemID == 0)
                    {
                        playerBag.itemList[i] = item;
                        break;
                    }
                }
            }
            else
            {//背包有这个物品,
                int currentAmount = playerBag.itemList[index].itemAmount + amount;
                var item = new InventoryItem { itemID = ID, itemAmount = currentAmount };
                playerBag.itemList[index] = item;
            }
        }

        /// <summary>
        /// 鼠标拖拽物品从包栏到包栏,
        /// </summary>
        /// <param name="fromIndex"></param>
        /// <param name="targetIndex"></param>
        public void SwapItem(int fromIndex, int targetIndex)
        {
            InventoryItem currentItem = playerBag.itemList[fromIndex];
            InventoryItem targetItem = playerBag.itemList[targetIndex];
            if (targetItem.itemID != 0)
            {
                playerBag.itemList[fromIndex] = targetItem;
                playerBag.itemList[targetIndex] = currentItem;
            }
            else
            {
                playerBag.itemList[targetIndex] = currentItem;
                playerBag.itemList[fromIndex] = new InventoryItem();
            }

            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
        
        /// <summary>
        /// 跨背包交换数据
        /// </summary>
        /// <param name="locationFrom"></param>
        /// <param name="fromIndex"></param>
        /// <param name="locationTarget"></param>
        /// <param name="targetIndex"></param>
        public void SwapItem(InventoryLocation locationFrom,int fromIndex,InventoryLocation locationTarget,int targetIndex){
            var currentList = GetItemList(locationFrom);//得到左边物品列表
            var targetList = GetItemList(locationTarget);//得到右边物品列表

            InventoryItem currentItem = currentList[fromIndex];//得到来源物品
            if(targetIndex < targetList.Count){//拉到右边的格子的坐标在合理范围内
                InventoryItem targetItem = targetList[targetIndex];//得到右边某格子的物品

                if(targetItem.itemID != 0 && currentItem.itemID != targetItem.itemID){//有两种不同的物品
                    currentList[fromIndex] = targetItem;
                    targetList[targetIndex] = currentItem;
                }else if(currentItem.itemID == targetItem.itemID){//相同的两个物品
                    targetItem.itemAmount += currentItem.itemAmount;
                    targetList[targetIndex] = targetItem;
                    currentList[fromIndex] = new InventoryItem();
                }else{//目标空格子
                    targetList[targetIndex] = currentItem;
                    currentList[fromIndex] = new InventoryItem();
                }
                EventHandler.CallUpdateInventoryUI(locationFrom, currentList);
                EventHandler.CallUpdateInventoryUI(locationTarget, targetList);
            }
        }

        /// <summary>
        /// 根据位置返回背包数据列表
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private List<InventoryItem> GetItemList(InventoryLocation location){
            return location switch
            {
                InventoryLocation.Player => playerBag.itemList,
                InventoryLocation.Box => currentBoxBag.itemList,
                _ => null
            };
        }  

        /// <summary>
        /// 通过物品ID从背包栏删除某个物品
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="删除数量"></param>
        private void RemoveItem(int ID,int removeAmount){
            var index = GetItemIndexBag(ID);

            if(playerBag.itemList[index].itemAmount>removeAmount){
                var amount = playerBag.itemList[index].itemAmount - removeAmount;
                var item = new InventoryItem
                {
                    itemID = ID,
                    itemAmount=amount
                };
                playerBag.itemList[index] = item;
            }else if(playerBag.itemList[index].itemAmount == removeAmount){
                var item = new InventoryItem();
                playerBag.itemList[index] = item;
            }
            //如果某框物品数量少于要删除的数量呢?

            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        public void TradeItem(ItemDetails itemDetails,int amount,bool isSellTrade){
            //计算原价
            int cost = itemDetails.itemPrice * amount;
            //物品在背包的位置
            int index = GetItemIndexBag(itemDetails.itemID);
            if(isSellTrade){ //卖
                if(playerBag.itemList[index].itemAmount>=amount){
                    RemoveItem(itemDetails.itemID, amount);
                    //卖出总价
                    cost = (int)(cost * itemDetails.sellPercentage);
                    playerMoney += cost;
                }
            }else if(playerMoney - cost>0){  //买
                if(CheckBagCapcity()){//检查背包是否有空位
                    AddItemAtItem(itemDetails.itemID, index, amount);
                }
                playerMoney -= cost;
            }
            //刷新UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        /// <summary>
        /// 检查建造资源物品库存
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool CheckStock(int ID){
            //取得蓝图建造物品
            var bluePrintDetails = bluePrintData.GetBluePrintDatails(ID);
            //取得蓝图所需建材和背包拥有的建材,看背包建材多过蓝图就允许建造
            foreach(var resourceItem in bluePrintDetails.resourceItem){
                var itemStock = playerBag.GetInventoryItem(resourceItem.itemID);
                if(itemStock.itemAmount >= resourceItem.itemAmount){
                    continue;
                }
                else return false;
            }
            return true;
        }

        /// <summary>
        /// 查找箱子数据
        /// </summary>
        /// <param name="key"></param>s
        /// <returns></returns>
        public List<InventoryItem> GetBoxDataList(string key){
            if(boxDataDict.ContainsKey(key)){
                return boxDataDict[key];
            }
            return null;
        }

        /// <summary>
        /// 加入箱子数据字典
        /// </summary>
        /// <param name="box"></param>
        public void AddBoxDataDict(Box box){
            var key = box.name + box.index;
            if(!boxDataDict.ContainsKey(key)){
                boxDataDict.Add(key, box.boxBagData.itemList);
            }
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.playerMoney = this.playerMoney;

            saveData.inventoryDict = new Dictionary<string, List<InventoryItem>>();
            saveData.inventoryDict.Add(playerBag.name, playerBag.itemList);

            foreach(var item in boxDataDict){
                saveData.inventoryDict.Add(item.Key, item.Value);
            }
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            this.playerMoney = saveData.playerMoney;
            playerBag = Instantiate(playerBagTemp);
            playerBag.itemList = saveData.inventoryDict[playerBag.name];

            foreach(var item in saveData.inventoryDict){
                if(boxDataDict.ContainsKey(item.Key)){
                    boxDataDict[item.Key] = item.Value;
                }
            }

            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
    }
}
