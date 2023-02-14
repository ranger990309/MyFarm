using UnityEngine.Events;
using UnityEngine;


namespace MFarm.Dialogue{
    [System.Serializable]
    public class DialoguePiece
    {
        [Header("对话详情")]
        public Sprite faceImage;
        public bool onLeft;
        public string name;

        [TextArea]
        public string dialogueText;
        public bool hasToPause;//
        /// <summary>
        /// 对话在面板上还没显示对话就false,显示完就变成true
        /// </summary>
        [HideInInspector]public bool isDone;
        public UnityEvent afterTalkEvent;
    }
}
