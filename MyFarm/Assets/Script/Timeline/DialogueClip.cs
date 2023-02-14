using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

//这是对话片段来的(片段是需要轨道的,放在轨道上)
public class DialogueClip : PlayableAsset,ITimelineClipAsset//要添加到轨道上,所以是playerableAsset,作用实例化playable的资源
{//ITimelineClipAsset实现此接口可以支持TimeLine剪辑的高级功能
    public ClipCaps clipCaps => ClipCaps.None;
    public DialogueBehaviour dialogue = new DialogueBehaviour();//???????????
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        //创建一个可编辑的片段
        var playable = ScriptPlayable<DialogueBehaviour>.Create(graph, dialogue);//???????????
        return playable;
    }
}
