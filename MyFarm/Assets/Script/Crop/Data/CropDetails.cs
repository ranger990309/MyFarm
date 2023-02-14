
using UnityEngine;

[System.Serializable]
public class CropDetails
{
    public int seedItemID;
    [Header("不同阶段所需要的天数")]
    public int[] growthDays;

    /// <summary>
    /// 完整生长完成所需要的时间
    /// </summary>
    /// <value></value>
    public int TotalGrowthDays{
        get{
            int amount = 0;
            foreach(var days in growthDays){
                amount += days;
            }
            return amount;
        }
    }

    [Header("不同生长阶段作物的prefabs")]
    public GameObject[] growthPrefabs;

    [Header("不同阶段作物的图片")]
    public Sprite[] growthSprites;
    [Header("可种植的季节")]
    public Season[] seasons;

    [Space]
    [Header("可用什么工具收割")]
    public int[] harvestToolItemID;
    [Header("每种工具使用次数(比如砍树砍几刀倒)")]
    public int[] requireActionCount;
    [Header("转化为新物品ID(比如树砍了变木头,葡萄收了变葡萄架)")]
    public int transformItemID;

    [Space]
    [Header("收割果实信息")]
    public int[] producedItemID;//生成果实ID
    public int[] producedMinAmount;//生成果实最小数量
    public int[] producedMaxAmount;//生成果实最大数量
    public Vector2 spawnRadius;//生成果实飞出的范围

    [Header("再次生长时间")]
    public int daysToRegrow;//收获到重生的时间
    public int regrowTimes;//重生次数?

    [Header("Options")]
    public bool generateAtPlayerPosition;//萝卜就生成在头顶
    public bool hasAnimation;//收获动画
    public bool hasParticalEffect;//拥有什么粒子特效
    public ParticaleEffectType effectType;//粒子掉落特效
    public Vector3 effectPos;//粒子位置
    public SoundName soundEffect;//声音特效

    /// <summary>
    /// 检查这个工具是否可以收割
    /// </summary>
    /// <param name="工具ID"></param>
    /// <returns></returns>
    public bool CheckToolAvailable(int toolID){
        foreach(var tool in harvestToolItemID){
            if(tool == toolID){
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获得工具需要使用次数
    /// </summary>
    /// <param name="工具ID"></param>
    /// <returns></returns>
    public int GetTotalRequireCount(int toolID){
        for (int i = 0; i < harvestToolItemID.Length;i++){
            if(harvestToolItemID[i] == toolID){
                return requireActionCount[i];
            }
        }
        return -1;
    }
}
