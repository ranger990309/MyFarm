using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MFarm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ShowItemTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private SlotUI slotUI;
        private InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();
        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
        }

        //先判断这个格子有物体且不为0,再启动信息面板,再将物体和格子的信息当参数传进去

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (slotUI.itemAmount != 0)
            {
                inventoryUI.itemTooltip.gameObject.SetActive(true);
                inventoryUI.itemTooltip.SetupTooltip(slotUI.itemDetails, slotUI.slotType);
                inventoryUI.itemTooltip.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
                inventoryUI.itemTooltip.transform.position = transform.position + Vector3.up * 60;

                if(slotUI.itemDetails.itemType == ItemType.Furniture){//如果是建造蓝图的话
                    inventoryUI.itemTooltip.resourcePanel.SetActive(true);//建材显示
                    inventoryUI.itemTooltip.SetupResourcePanel(slotUI.itemDetails.itemID);//设置提示框所需要的建材
                }else{//如果是其他东西
                    inventoryUI.itemTooltip.resourcePanel.SetActive(false);//就关闭建材需要
                }
            }
            else
            {
                inventoryUI.itemTooltip.gameObject.SetActive(false);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            inventoryUI.itemTooltip.gameObject.SetActive(false);
        }


    }
}
