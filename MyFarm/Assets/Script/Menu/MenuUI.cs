using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public GameObject[] panels;
    public void SwitchPanel(int index){
        for (int i = 0; i < panels.Length;i++){
            if(i == index){
                panels[i].transform.SetAsLastSibling();//把选中的移到下面去
            }
            
        }
    }

    public void ExitGame(){
        Application.Quit();
        Debug.Log("Exit GAME");
    }
}