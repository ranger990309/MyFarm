using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Inventory{
    [RequireComponent(typeof(SlotUI))]
    public class ActionBarButton : MonoBehaviour
    {
        public KeyCode key;
        private SlotUI slotUI;
        private bool canUse;//按钮可不可用
        private void Awake() {
            slotUI = GetComponent<SlotUI>();
        }

        private void OnEnable() {
            EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        }

        private void OnDisable() {
            EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        }

        private void OnUpdateGameStateEvent(GameState gameState)
        {
            canUse = (gameState == GameState.GamePlay);
        }

        private void Update() {
            //??????????????
            if(Input.GetKeyDown(key) && canUse){
                if(slotUI.itemDetails != null){
                    slotUI.isSelected = !slotUI.isSelected;
                    if(slotUI.isSelected){
                        slotUI.inventoryUI.UpdateHighLightUI(slotUI.slotIndex);
                    }else{
                        slotUI.inventoryUI.UpdateHighLightUI(-1);
                    }
                    //手举起物品
                    EventHandler.CallItemSelectedEvent(slotUI.itemDetails, slotUI.isSelected);
                }
            }
        }
    }
}
