using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class ItemDetails
{
    public int itemID;
    public string itemName;
    public ItemType itemType;
    public Sprite itemIcon;
    public Sprite itemOnWorldSprite;
    public string itemDescription;
    public int itemUseRadius;
    public bool canPickedup;
    public bool canDropped;
    public bool canCarried;
    public int itemPrice;
    [Range(0, 1)]
    //自己卖东西时打的折扣
    public float sellPercentage;

}

/// <summary>
/// 物品ID,数量
/// </summary>
[System.Serializable]
public struct InventoryItem{
    public int itemID;
    public int itemAmount;
}

/// <summary>
/// 判断物体是否可以搬运
/// </summary>
[System.Serializable]
public class AnimatorType
{
    public PartType partType;
    public PartName partName;
    public AnimatorOverrideController overrideController;
}

/// <summary>
/// ?????????????
/// </summary>
[System.Serializable]
public class SerializableVector3
{
    public float x, y, z;
    public SerializableVector3(Vector3 pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int((int)x, (int)y);
    }
}

/// <summary>
/// 场景物品和位置
/// </summary>
[System.Serializable]
public class SceneItem
{
    public int itemID;
    public SerializableVector3 position;
}

[System.Serializable]
public class SceneFurniture{
    public int itemID;
    public SerializableVector3 position;
    public int boxIndex;//这个游戏第几个箱子
}

[System.Serializable]
public class TileProperty
{
    /// <summary>
    /// 瓦片坐标,类型,值?
    /// </summary>
    public Vector2Int tileCoordinate;
    public GridType gridType;
    public bool booltypeValue;//???????????
}

/// <summary>
/// 十几个瓦片基础信息
/// </summary>
[System.Serializable]
public class TileDetails{
    public int gridX, gridY;
    public bool canDig;
    public bool canDropItem;
    public bool canPlaceFurniture;
    public bool isNPCObstacle;
    public int daysSinceDug = -1;
    public int daysSinceWatered = -1;
    public int seedItemID = -1;
    /// <summary>
    /// 作物在土地上呆了几天
    /// </summary>
    public int growthDays = -1;
    public int daysSinceLostHarvest = -1;
}

[System.Serializable]
public class NPCPosition{
    public Transform npc;//npc现有坐标
    public string startScene;//npc初始场景
    public Vector3 position;//npc初始坐标
}

/// <summary>
/// 跨场景整条路线
/// </summary>
[System.Serializable]
public class SceneRoute{//跨场景整条路线
    public string fromSecondName;
    public string gotoSceneName;
    public List<ScenePath> scenePathList;
}

/// <summary>
/// 某场景的一个路线
/// </summary>
[System.Serializable]
public class ScenePath{
    public string sceneName;
    public Vector2Int fromGridCell;
    public Vector2Int gotoGridCell;
}