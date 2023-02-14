using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MFarm.Dialogue;

public static class EventHandler
{
    /// <summary>
    /// 事件:更新格子UI的,第一参数:看要更新的是player或box的背包,第二参数:再将里面的一些数据传进去
    /// </summary>
    public static event Action<InventoryLocation,List<InventoryItem>> UpdateInventoryUI;

    /// <summary>
    /// 更新格子UI的方法,第一参数:看要更新的是player或box的背包,第二参数:再将里面的一些数据传进去
    /// </summary>
    /// <param name="看要更新的是player或box的背包"></param>
    /// <param name="再将player或box里面的一些数据传进去"></param>
    public static void CallUpdateInventoryUI(InventoryLocation location,List<InventoryItem> list){
        //先检查一下UpdateInventoryUI是否为空,不空再加入
        UpdateInventoryUI?.Invoke(location,list);
    }

    /// <summary>
    /// 事件:生成物品在地面,需要物品ID和坐标
    /// </summary>
    public static event Action<int,Vector3> InstantiateItemInScene;
    
    /// <summary>
    /// 生成物品在地面,需要物品ID和坐标
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="pos"></param>
    public static void CallInstantiateItemInScene(int ID,Vector3 pos){
        //事件不为空则加入
        InstantiateItemInScene?.Invoke(ID,pos);
    }

    /// <summary>
    /// 丢物品到地上
    /// </summary>
    public static event Action<int,Vector3,ItemType> DropItemEvent;
    /// <summary>
    /// 丢物品到地上
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="pos"></param>
    public static void CallDropItemEvent(int ID,Vector3 pos,ItemType itemType){
        //事件不为空则加入
        DropItemEvent?.Invoke(ID, pos, itemType);
    }

    /// <summary>
    /// 事件:物品框中的物品被选中时手会举起物品
    /// </summary>
    public static event Action<ItemDetails,bool> ItemSelectedEvent;
    /// <summary>
    /// 物品框中的物品被选中时手会举起物品
    /// </summary>
    public static void CallItemSelectedEvent(ItemDetails itemDetails,bool isSelected){
        ItemSelectedEvent?.Invoke(itemDetails,isSelected);
    }

    /// <summary>
    /// 游戏时间事件???到了时间加时间块?
    /// </summary>
    public static event Action<int,int,int,Season> GameMinteEvent;
    public static void CallGameMinteEvent(int minute,int hour,int day,Season season){
        GameMinteEvent?.Invoke(minute,hour,day,season);
    }

    public static event Action<int, Season> GameDayEvent;
    public static void CallGameDayEvent(int day,Season season){
        GameDayEvent?.Invoke(day, season);
    }

    /// <summary>
    /// 游戏日期事件???
    /// </summary>
    public static event Action<int,int,int,int,Season> GameDataEvent;
    public static void CallGameDataEvent(int hour,int day,int month,int year,Season season){
        GameDataEvent?.Invoke(hour,day,month,year,season);
    }

    /// <summary>
    /// 场景切换,要切换哪个场景的哪个地点
    /// </summary>
    public static event Action<string,Vector3> TransitionEvent;
    public static void CallTransitionEvent(string sceneName,Vector3 pos){
        TransitionEvent?.Invoke(sceneName,pos);
    }

    /// <summary>
    /// 场景切换前
    /// </summary>
    public static event Action BeforeSceneUnloadEvent;
    public static void CallBeforeSceneUnloadEvent(){
        BeforeSceneUnloadEvent?.Invoke();
    }

    /// <summary>
    /// 场景切换后
    /// </summary>
    public static event Action AfterSceneLoadedEvent;
    public static void CallAfterSceneLoadedEvent(){
        AfterSceneLoadedEvent?.Invoke();
    }
    
    /// <summary>
    ///  移动人物坐标
    /// </summary>
    public static event Action<Vector3> MoveToPosition;
    public static void CallMoveToPosition(Vector3 pos){
        MoveToPosition?.Invoke(pos);
    }

    /// <summary>
    /// 鼠标点击事件
    /// </summary>
    public static event Action<Vector3, ItemDetails> MouseClickedEvent;
    public static void CallMouseClickedEvent(Vector3 pos,ItemDetails itemDetails){
        MouseClickedEvent?.Invoke(pos, itemDetails);
    }

    /// <summary>
    /// 在播放了(砍树)动画后实行的(砍树)数据的实现
    /// </summary>
    public static event Action<Vector3, ItemDetails> ExecuteActionAfterAnimation;
    public static void CallExecuteActionAfterAnimation(Vector3 pos,ItemDetails itemDetails){
        ExecuteActionAfterAnimation?.Invoke(pos, itemDetails);
    }

    public static event Action<int, TileDetails> plantSeedEvent;
    public static void CallplantSeedEvent(int ID,TileDetails tile){
        plantSeedEvent?.Invoke(ID, tile);
    }

    public static event Action<int> HarvestAtPlayerPosition;
    public static void CallHarvestAtPlayerPosition(int ID){
        HarvestAtPlayerPosition?.Invoke(ID);
    }

    public static event Action RefreshCurrentMap;
    public static void CallRefreshCurrentMap(){
        RefreshCurrentMap?.Invoke();
    }

    public static event Action<ParticaleEffectType, Vector3> ParticleEffectEvent;
    public static void CallParticleEffectEvent(ParticaleEffectType effectType,Vector3 pos){
        ParticleEffectEvent?.Invoke(effectType, pos);
    }

    public static event Action GenerateCropEvent;
    public static void CallGenerateCropEvent(){
        GenerateCropEvent?.Invoke();
    }

    /// <summary>
    /// 显示对话事件
    /// </summary>
    public static event Action<DialoguePiece> ShowDialogueEvent;
    public static void CallShowDialogueEvent(DialoguePiece piece){
        ShowDialogueEvent?.Invoke(piece);
    }

    public static event Action<SlotType, InventoryBag_SO> BaseBagOpenEvent;
    public static void CallBaseBagOpenEvent(SlotType slotType,InventoryBag_SO bag_SO){
        BaseBagOpenEvent?.Invoke(slotType, bag_SO);
    }

    public static event Action<SlotType, InventoryBag_SO> BaseBagCloseEvent;
    public static void CallBaseBagCloseEvent(SlotType slotType,InventoryBag_SO bag_SO){
        BaseBagCloseEvent?.Invoke(slotType, bag_SO);
    }

    /// <summary>
    /// 暂停或继续游戏
    /// </summary>
    public static event Action<GameState> UpdateGameStateEvent;
    public static void CallUpdateGameStateEvent(GameState gameState){
        UpdateGameStateEvent?.Invoke(gameState);
    }

    public static event Action<ItemDetails, bool> ShowTradeUI;
    public static void CallShowTradeUI(ItemDetails item,bool isSell){
        ShowTradeUI?.Invoke(item, isSell);
    }

    //建造
    public static event Action<int, Vector3> BuildFurnitureEvent;
    public static void CallBuildFurnitureEvent(int ID,Vector3 pos){
        BuildFurnitureEvent?.Invoke(ID, pos);
    }

    //灯光
    public static event Action<Season, LightShift, float> LightShiftChangeEvent;
    public static void CallLightShiftChangeEvent(Season season,LightShift lightShift,float timeDifference){
        LightShiftChangeEvent?.Invoke(season, lightShift, timeDifference);
    }

    //音效
    public static event Action<SoundDetails> InitSoundEffect;
    public static void CallInitSoundEffect(SoundDetails soundDetails){
        InitSoundEffect?.Invoke(soundDetails);
    }

    public static event Action<SoundName> PlaySoundEvent;
    public static void CallPlaySoundEvent(SoundName soundName){
        PlaySoundEvent?.Invoke(soundName);
    }

    public static event Action<int> StartNewGameEvent;
    public static void CallStartNewGameEvent(int index){
        StartNewGameEvent?.Invoke(index);
    }

    public static event Action EndGameEvent;
    public static void CallEndGameEvent(){
        EndGameEvent?.Invoke();
    }

}
