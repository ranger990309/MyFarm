using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : Singleton<NPCManager>
{
    public SceneRouteDataList_SO sceneRouteDate;//????????????
    public List<NPCPosition> npcPositionList;//NPC的初始坐标
    /// <summary>
    /// 这路径字典来的,包含有两个路径,一个屋外到屋内,另一个相反
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <typeparam name="SceneRoute"></typeparam>
    /// <returns></returns>
    private Dictionary<string, SceneRoute> sceneRouteDict = new Dictionary<string, SceneRoute>();

    protected override void Awake(){
        base.Awake();
        InitSceneRouteDict();
    }

    private void OnEnable() {
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable() {
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        foreach(var character in npcPositionList){
            character.npc.position = character.position;//npc现有坐标等于初始坐标
            character.npc.GetComponent<NPCMovement>().StartScene = character.startScene;
        }
    }

    /// <summary>
    /// 初始化路线字典
    /// </summary>
    private void InitSceneRouteDict(){
        if(sceneRouteDate.sceneRouteList.Count > 0){
            //看有几条路线,
            foreach(SceneRoute route in sceneRouteDate.sceneRouteList){
                var key = route.fromSecondName + route.gotoSceneName;

                if(sceneRouteDict.ContainsKey(key)){//字典中有这个key就跳过,没就加进去
                    continue;
                }else{
                    sceneRouteDict.Add(key, route);
                }
            }
        }
    }

    /// <summary>
    /// 查找两个场景的路径
    /// </summary>
    /// <param name="fromSecondName"></param>
    /// <param name="gotoSceneName"></param>
    /// <returns></returns>
    public SceneRoute GetSceneRoute(string fromSecondName,string gotoSceneName){
        return sceneRouteDict[fromSecondName + gotoSceneName];
    }
}
