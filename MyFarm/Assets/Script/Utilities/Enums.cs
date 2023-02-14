using System.ComponentModel.Design;
public enum ItemType
{
    seed, Commodity, Furniture,
    HoeTool, ChopTool, breakTool, ReapTool, WaterTool, CollectTool,
    ReapableScenery
}

public enum SlotType
{
    Bag, Box, Shop
}

/// <summary>
/// Player或者Box的背包
/// </summary>
public enum InventoryLocation
{
    Player, Box
}

/// <summary>
/// 判断动画是举起还是手持等
/// </summary>
public enum PartType
{
    None, Carry, Hoe, Break, Water,Collect,Chop,Reap
}

public enum PartName
{
    Body, Hair, Arm, Tool
}

public enum Season
{
    春天, 夏天, 秋天, 冬天
}

/// <summary>
/// 地面类型是可挖掘还是什么的
/// </summary>
public enum GridType
{
    Diggable, DropItem, PlaceFurniture, NPCObstacle
}

public enum ParticaleEffectType{
    None,LeavesFalling01,LeavesFalling02,Rock,ReapableScenery
}

public enum GameState{
    GamePlay,Pause
}

public enum LightShift{
    Morning,Night
}

public enum SoundName{
    none,FootStepSoft,FootStepHard,
    Axe,Pickaxe,Hoe,Reap,Water,Basket,Chop,
    Pickup,Plant,TreeFalling,Rustle,
    AmbientCountryside1,AmbientCountryside2,MusicCalm1,MusicCalm2,MusicCalm3,MusicCalm4,MusicCalm5,MusicCalm6,AmbientIndoor1
}
