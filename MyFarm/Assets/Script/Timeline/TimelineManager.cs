using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : Singleton<TimelineManager>
{
    public PlayableDirector startDirector;
    private PlayableDirector currentDiector;
    private bool isDone;
    public bool IsDone{ set => isDone = value; }
    private bool isPause;

    protected override void Awake() {
        base.Awake();
        currentDiector = startDirector;
    }

    private void OnEnable() {

        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable() {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        currentDiector.Play();
    }

    private void OnAfterSceneLoadedEvent()
    {
        currentDiector = FindObjectOfType<PlayableDirector>();
    }


    private void Update() {
        if(isPause && Input.GetKeyDown(KeyCode.Space) && isDone){//空格继续
            isPause = false;
            currentDiector.playableGraph.GetRootPlayable(0).SetSpeed(1d);
        }
    }

    public void PauseTimeline(PlayableDirector director){
        currentDiector = director;
        //得到Timeline那条时间线,得到Playable,设置指针速度为0
        currentDiector.playableGraph.GetRootPlayable(0).SetSpeed(0d);
        isPause = true;
    }
}
