using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFuntion : MonoBehaviour
{
    public InventoryBag_SO shopData;//商店对象池
    private bool isOpen;

    private void Update() {
        //关闭背包
        if(isOpen && Input.GetKeyDown(KeyCode.Escape)){
            //关闭背包
            Debug.Log("1");
            CloseShop();
            Debug.Log("2");
        }
    }

    public void OpenShop(){
        isOpen = true;
        //打开背包
        EventHandler.CallBaseBagOpenEvent(SlotType.Shop, shopData);
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
    }

    public void CloseShop(){
        isOpen = false;
        EventHandler.CallBaseBagCloseEvent(SlotType.Shop, shopData);
        EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
    }
}
