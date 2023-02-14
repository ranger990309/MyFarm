using System.Collections;
using System.Collections.Generic;
using MFarm.Dialogue;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DialogueUI : MonoBehaviour
{
    public GameObject dialogueBox;
    public Text dialogueText;
    public Image faceRight, faceLeft;
    public Text nameRight, nameLeft;
    public GameObject continueBox;

    private void Awake() {
        continueBox.SetActive(false);
    }

    private void OnEnable() {
        EventHandler.ShowDialogueEvent += OnShowDialogueEvent;
    }

    private void OnDisable() {
        EventHandler.ShowDialogueEvent -= OnShowDialogueEvent;
    }

    private void OnShowDialogueEvent(DialoguePiece piece)
    {
        StartCoroutine(ShowDialogue(piece));
    }

    /// <summary>
    /// 展示对话
    /// </summary>
    /// <param name="piece"></param>
    /// <returns></returns>
    private IEnumerator ShowDialogue(DialoguePiece piece){
        if(piece != null){
            //先把isDone变False先,因为要展示对话了
            piece.isDone = false;
            dialogueBox.SetActive(true);//展示出人头和对话框
            continueBox.SetActive(false);//按钮先不要展示
            dialogueText.text = string.Empty;

            if(piece.name != string.Empty){
                //如果是左边的人
                if(piece.onLeft){
                    faceRight.gameObject.SetActive(false);//把右边的头像关了
                    faceLeft.gameObject.SetActive(true);//左边的头像开了
                    faceLeft.sprite = piece.faceImage;//左边的头像开了
                    nameLeft.text = piece.name;//左边的头像下面的框显示名字
                }else{
                    faceRight.gameObject.SetActive(true);//把右边的头像开了
                    faceLeft.gameObject.SetActive(false);//左边的头像关了
                    faceRight.sprite = piece.faceImage;//右边的头像开了
                    nameRight.text = piece.name;//右边的头像下面的框显示名字
                }
            }else{//如果name没空的话
                faceRight.gameObject.SetActive(false);
                faceLeft.gameObject.SetActive(false);
                nameLeft.gameObject.SetActive(false);
                nameRight.gameObject.SetActive(false);
            }
            //会一个字一个字的打在文本框上
            yield return dialogueText.DOText(piece.dialogueText, 1f).WaitForCompletion();

            piece.isDone = true;

            //这段话显示完了需要空格进行下一句话,空格会显示
            if(piece.hasToPause && piece.isDone){
                continueBox.SetActive(true);
            }else{
                continueBox.SetActive(true);
            }

        }else{
            dialogueBox.SetActive(false);
            yield break;
        }
    }
}
