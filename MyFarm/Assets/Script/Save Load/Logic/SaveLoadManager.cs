using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
namespace MFarm.Save
{
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {
        private List<ISaveable> saveableList = new List<ISaveable>();//一堆存储档案
        public List<DataSlot> dataSlots = new List<DataSlot>(new DataSlot[3]);//把一堆背包数据啊地图数据啊npc数据全集成在一个DataSlot中
        private string jsonFolder;//存档保存路径 文件夹
        private int currentDataIndex;
        protected override void Awake()
        {
            base.Awake();
            jsonFolder = Application.persistentDataPath + "/SAVE DATA/";//存档路径 文件夹
            ReadSaveData();
        }

        private void OnEnable() {
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable() {
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void OnEndGameEvent()
        {
            Save(currentDataIndex);
        }

        private void OnStartNewGameEvent(int index)
        {
            currentDataIndex = index;
        }

        private void Update() {
            if(Input.GetKeyDown(KeyCode.I)){
                Save(currentDataIndex);
            }
            if(Input.GetKeyDown(KeyCode.O)){
                Load(currentDataIndex);
            }
        }

        public void RegisterSaveable(ISaveable saveable){//每一个ISaveable就加入存储列表saveableList中
            if(!saveableList.Contains(saveable)){
                saveableList.Add(saveable);
            }
        }

        #region 通过路径拿到存储文件,然后读取
        private void ReadSaveData(){
            if(Directory.Exists(jsonFolder)){
                for (int i = 0; i < dataSlots.Count;i++){
                    var resultPath = jsonFolder + "data" + i + ".json";
                    if(File.Exists(resultPath)){
                        var stringData = File.ReadAllText(resultPath);//读取存档文件
                        var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);//解析反序列化存档信息
                        dataSlots[i] = jsonData;
                    }
                }
            }
        }
        #endregion

        private void Save(int index){//把一堆背包数据啊地图数据啊npc数据全集成在一个DataSlot中
            DataSlot data = new DataSlot();
            foreach(var saveable in saveableList){
                data.dataDict.Add(saveable.GUID, saveable.GenerateSaveData());
            }
            dataSlots[index] = data;

            var resultPath = jsonFolder + "data" + index + ".json";//创建存档文件

            var jsonData = JsonConvert.SerializeObject(dataSlots[index], Formatting.Indented);//将存档文件序列化

            if(!File.Exists(resultPath)){//没有路径就创建路径
                Directory.CreateDirectory(jsonFolder);
            }
            Debug.Log("DATA" + index + "SAVED!");

            File.WriteAllText(resultPath, jsonData);//将存档信息写进存档文件里
        }

        public void Load(int index){
            currentDataIndex = index;
            var resultPath = jsonFolder + "data" + index + ".json";//取得存档文件
            var stringData = File.ReadAllText(resultPath);//读取存档文件
            var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);//解析反序列化存档信息
            foreach(var saveable in saveableList){//将得到的信息重新赋值回去各背包网格
                saveable.RestoreData(jsonData.dataDict[saveable.GUID]);
            }
        }
    }
}