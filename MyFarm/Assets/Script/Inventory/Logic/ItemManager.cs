using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MFarm.Save;

namespace MFarm.Inventory
{
    public class ItemManager : MonoBehaviour,ISaveable
    {
        //基础未赋值ID的物体
        public Item itemPerfab;
        /// <summary>
        /// 要扔的基础未赋值ID的物体
        /// </summary>
        public Item bounceItemPrefabs;
        private Transform itemParent;//???????

        /// <summary>
        /// Player坐标
        /// </summary>
        /// <typeparam name="Player"></typeparam>
        /// <returns></returns>
        private Transform playerTransform => FindObjectOfType<Player>().transform;

        public string GUID => GetComponent<DataGUID>().guid;

        /// <summary>
        /// (场景-物品列表)记录场景有什么物品列表的字典
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<SceneItem>> sceneItemDict = new Dictionary<string, List<SceneItem>>();

        /// <summary>
        /// 记录(场景-家具列表)
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,List<SceneFurniture>> sceneFurnitureDict = new Dictionary<string, List<SceneFurniture>>();
        private void OnEnable()
        {
            EventHandler.InstantiateItemInScene += OnInstantiateItemInScene;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaderEvent;
            EventHandler.DropItemEvent += OnDropItemEvent;
            //建造
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.InstantiateItemInScene -= OnInstantiateItemInScene;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaderEvent;
            EventHandler.DropItemEvent -= OnDropItemEvent;
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }

        private void OnStartNewGameEvent(int obj)
        {
            sceneItemDict.Clear();//新游戏场景物品清空
            sceneFurnitureDict.Clear();//新游戏场景家具清空
        }

        private void Start() {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }

        //在场景建造家具
        private void OnBuildFurnitureEvent(int ID,Vector3 mousePos)
        {
            BluePrintDatails bluePrint = InventoryManager.Instance.bluePrintData.GetBluePrintDatails(ID);
            var buildItem = Instantiate(bluePrint.buildPrefab, mousePos, Quaternion.identity, itemParent);//生成家具
            if(buildItem.GetComponent<Box>()){
                buildItem.GetComponent<Box>().index = InventoryManager.Instance.BoxDataAmount;//给家具赋值index
                buildItem.GetComponent<Box>().InitBox(buildItem.GetComponent<Box>().index);//生成Box
            }
        }

        /// <summary>
        /// 扔东西()
        /// </summary>
        /// <param name="物体ID"></param>
        /// <param name="要扔的位置"></param>
        private void OnDropItemEvent(int ID, Vector3 mousePos,ItemType itemType)
        {
            if(itemType ==ItemType.seed) return;
            //动画放完再生成东西在脚底,要扔的时候再移到头顶去
            var item = Instantiate(bounceItemPrefabs, playerTransform.position, Quaternion.identity, itemParent);
            item.itemID = ID;
            var dir = (mousePos - playerTransform.position).normalized;
            item.GetComponent<ItemBounce>().InitbounceItem(mousePos, dir);
        }

        private void OnBeforeSceneUnloadEvent()
        {
            GetAllSceneItems();//保存这个场景的物品列表
            GetAllSceneFurniture();//保存这个场景的家具列表
        }

        /// <summary>
        /// (旧场景删除新场景通过加载时)
        /// </summary>
        private void OnAfterSceneLoaderEvent()
        {
            itemParent = GameObject.FindWithTag("ItemParent").transform;
            RecreateAllItems();
            RebuildFurniture();//为这个场景加上应有的家具
        }

        /// <summary>
        /// 在地面生成物品
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="pos"></param>
        private void OnInstantiateItemInScene(int ID, Vector3 pos)
        {
            //在地面生成物体(itemPerfab),Quaternion.identity是无旋转
            var item = Instantiate(bounceItemPrefabs, pos, Quaternion.identity, itemParent);
            //赋值ID
            item.itemID = ID;
            item.GetComponent<ItemBounce>().InitbounceItem(pos, Vector3.up);
        }

        /// <summary>
        /// 得到场景所有物品和位置
        /// </summary>
        private void GetAllSceneItems()
        {
            List<SceneItem> currentSceneItems = new List<SceneItem>();
            //在场景中搜寻所有物品并加入currentSceneItems列表
            foreach (var item in FindObjectsOfType<Item>())
            {
                SceneItem sceneItem = new SceneItem
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position)
                };
                currentSceneItems.Add(sceneItem);
            }
            //如果sceneItemDict有该场景的名字,则将该场景的物品列表复制给他
            if (sceneItemDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneItemDict[SceneManager.GetActiveScene().name] = currentSceneItems;
            }
            else
            {//如果是新场景,则在sceneItemDict创建个新场景的名字
                sceneItemDict.Add(SceneManager.GetActiveScene().name, currentSceneItems);
            }
        }

        /// <summary>
        /// 删除旧场景所有物品加载新场景物品
        /// </summary>
        private void RecreateAllItems()
        {
            List<SceneItem> CurrentSceneItems = new List<SceneItem>();
            //查询字典通过新场景名字找新场景物品返回给CurrentSceneItems列表
            if (sceneItemDict.TryGetValue(SceneManager.GetActiveScene().name, out CurrentSceneItems))
            {
                //如果新场景有物品,就删除旧场景所有东西,附上新场景物品
                if (CurrentSceneItems != null)
                {
                    //清场
                    foreach (var item in FindObjectsOfType<Item>())
                    {
                        Destroy(item.gameObject);
                    }
                    foreach (var item in CurrentSceneItems)
                    {
                        Item newItem = Instantiate(itemPerfab, item.position.ToVector3(), Quaternion.identity, itemParent);
                        newItem.Init(item.itemID);
                    }
                }
            }
        }

        /// <summary>
        /// 取得这个场景的家具
        /// </summary>
        private void GetAllSceneFurniture(){
            List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();
            //寻找场景中的家具,将其注册成SceneFurniture类型并加入队列
            foreach(var item in FindObjectsOfType<Furniture>()){
                SceneFurniture sceneFurniture = new SceneFurniture
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position)
                };
                if(item.GetComponent<Box>()){
                    sceneFurniture.boxIndex = item.GetComponent<Box>().index;
                }
                currentSceneFurniture.Add(sceneFurniture);
            }

            //如果(场景-家具列表)中有这个场景,就更新家具列表
            if(sceneFurnitureDict.ContainsKey(SceneManager.GetActiveScene().name)){
                //
                sceneFurnitureDict[SceneManager.GetActiveScene().name] = currentSceneFurniture;
            }
            else{//如果是新场景
                sceneFurnitureDict.Add(SceneManager.GetActiveScene().name, currentSceneFurniture);
            }
        }

        /// <summary>
        /// 重建场景家具
        /// </summary>
        private void RebuildFurniture(){
            List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();
            //将这个场景的家具列表取出
            if(sceneFurnitureDict.TryGetValue(SceneManager.GetActiveScene().name,out currentSceneFurniture)){
                if(currentSceneFurniture != null){
                    //从家具列表里循环一个个家具,建造在现有场景中
                    foreach(SceneFurniture sceneFurniture in currentSceneFurniture){
                        BluePrintDatails bluePrint = InventoryManager.Instance.bluePrintData.GetBluePrintDatails(sceneFurniture.itemID);//蓝图
                        var buildItem = Instantiate(bluePrint.buildPrefab, sceneFurniture.position.ToVector3(), Quaternion.identity, itemParent);
                        if(buildItem.GetComponent<Box>()){
                            buildItem.GetComponent<Box>().InitBox(sceneFurniture.boxIndex);//生成家具
                        }
                    }
                }
            }
        }

        public GameSaveData GenerateSaveData()
        {
            GetAllSceneItems();
            GetAllSceneFurniture();

            GameSaveData saveData = new GameSaveData();
            saveData.sceneItemDict = this.sceneItemDict;
            saveData.sceneFurnitureDict = this.sceneFurnitureDict;

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            this.sceneItemDict = saveData.sceneItemDict;
            this.sceneFurnitureDict = saveData.sceneFurnitureDict;

            RecreateAllItems();
            RebuildFurniture();
        }
    }
}
