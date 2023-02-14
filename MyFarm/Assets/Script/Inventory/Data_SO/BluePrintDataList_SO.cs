using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BluePrintDataList_SO", menuName = "Inventory/BluePrintDataList_SO")]
public class BluePrintDataList_SO : ScriptableObject {
    public List<BluePrintDatails> bluePrintDataList;//一堆需要建造的物品

    //在一堆需要建造的物品中查找想要的
    public BluePrintDatails GetBluePrintDatails(int itemID){
        //这里b指的是BluePrintDatails,
        return bluePrintDataList.Find(b => b.ID == itemID);
    }
}

[System.Serializable]
public class BluePrintDatails{
    public int ID;
    public InventoryItem[] resourceItem = new InventoryItem[4];//需要的资源
    public GameObject buildPrefab;//建造的物品
}
