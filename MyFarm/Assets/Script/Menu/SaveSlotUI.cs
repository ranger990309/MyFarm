using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MFarm.Save;

public class SaveSlotUI : MonoBehaviour
{
    public Text dataTime, dataScene;//这是三个存档框你的时间场景
    private Button currentButton;//这是存档按钮
    private DataSlot currentData;
    private int Index => transform.GetSiblingIndex();//是第几个按钮

    private void Awake() {
        currentButton = GetComponent<Button>();
        currentButton.onClick.AddListener(LoadGameData);
    }

    private void OnEnable() {
        SetupSlotUI();
    }

    private void SetupSlotUI(){
        currentData = SaveLoadManager.Instance.dataSlots[Index];//三个保存框里的数据

        if(currentData != null){
            dataTime.text = currentData.DataTime;//年月日时间
            dataScene.text = currentData.DataScene;
        }else{
            dataTime.text = "这个时间还没开始";
            dataScene.text = "梦还没开始";
        }
    }

    /// <summary>
    /// 加载游戏数据
    /// </summary>
    private void LoadGameData(){//?????????????????????
        if(currentData != null){//若这个保存框里有数据
            SaveLoadManager.Instance.Load(Index);//就加载他的数据
        }else{//若这个保存框里没数据,就说明要新建个游戏
            Debug.Log("新游戏");
            EventHandler.CallStartNewGameEvent(Index);
        }
    }
}
