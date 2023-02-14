using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MFarm.Dialogue{
    [RequireComponent(typeof(NPCMovement))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueController : MonoBehaviour{
        private NPCMovement npc => GetComponent<NPCMovement>();
        public UnityEvent OnFinishEvent;
        /// <summary>
        /// 存着一堆话
        /// </summary>
        /// <typeparam name="DialoguePiece"></typeparam>
        /// <returns></returns>
        public List<DialoguePiece> dialogueList = new List<DialoguePiece>();
        /// <summary>
        /// 也是存着一堆话
        /// </summary>
        private Stack<DialoguePiece> dialogueStack;
        private bool canTalk;
        private bool isTalking;//是否正在讲话
        //空格UI
        private GameObject uiSign;
        private void Awake() {
            uiSign = transform.GetChild(1).gameObject;
            FillDialogueStack();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if(other.CompareTag("Player")){
                //若NPC不在移动且可交流,那就允许PLayer和他交流
                canTalk = (!npc.isMoving && npc.interactable);
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if(other.CompareTag("Player")){
                //离开NPC范围就不允许讲话
                canTalk = false;
            }
        }

        private void Update() {
            //可以讲话的话空格UI就显示
            uiSign.SetActive(canTalk);
            
            //想和NPC讲话就按空格
            if(canTalk & Input.GetKeyDown(KeyCode.Space) && !isTalking){
                StartCoroutine(DialogueRoutine());
            }
        }

        /// <summary>
        /// 将List里的一堆话复制到Stack里
        /// </summary>
        private void FillDialogueStack(){
            dialogueStack = new Stack<DialoguePiece>();
            for (int i = dialogueList.Count - 1; i > -1;i--){
                dialogueList[i].isDone = false;
                dialogueStack.Push(dialogueList[i]);
            }
        }

        /// <summary>
        /// 显示对话传对话和结束对话
        /// </summary>
        /// <returns></returns>
        private IEnumerator DialogueRoutine(){
            isTalking = true;
            //从stack里一堆话一个个抽出来
            if(dialogueStack.TryPop(out DialoguePiece result)){
                //传到UI显示对话
                EventHandler.CallShowDialogueEvent(result);
                EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                //等候isDone变成true时再执行后面的内容
                yield return new WaitUntil(() => (result.isDone == true));
                isTalking = false;
            }else{//话说完了,该结束了
                EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
                EventHandler.CallShowDialogueEvent(null);
                FillDialogueStack();//说话结束后记得把stack填满,这样再一次说话说的还是重复的
                isTalking = false;

                if(OnFinishEvent != null){
                    OnFinishEvent?.Invoke();
                    canTalk = false;//打开商店后对话就要关闭了
                }
            }
        }
    }
}
