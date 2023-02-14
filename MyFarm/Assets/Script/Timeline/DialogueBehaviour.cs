using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using MFarm.Dialogue;

[System.Serializable]
public class DialogueBehaviour : PlayableBehaviour//PlayableBehaviour要连接外部TimeLine用的
{
    private PlayableDirector director;
    public DialoguePiece dialoguePiece;
    public override void OnPlayableCreate(Playable playable)//???????????
    {
        //拿到后强制转换
        director = (playable.GetGraph().GetResolver() as PlayableDirector);
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        //呼叫启动对话框
        EventHandler.CallShowDialogueEvent(dialoguePiece);
        if(Application.isPlaying){//这段代码正在运行,判断对话片段是否又暂停,有则暂停
            if(dialoguePiece.hasToPause){
                //暂停timeline
                TimelineManager.Instance.PauseTimeline(director);
            }else{
                EventHandler.CallShowDialogueEvent(null);
            }
        }
    }

    //timeline那边每一帧都执行,每一帧都检测这个对话显示完没有,显示完就能按空格跳是否按下了IsDone就下一个对话
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if(Application.isPlaying){
            TimelineManager.Instance.IsDone = dialoguePiece.isDone;
        }
    }

    //
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        EventHandler.CallShowDialogueEvent(null); 
    }

    public override void OnGraphStart(Playable playable)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
    }

    public override void OnGraphStop(Playable playable)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
    }
}
