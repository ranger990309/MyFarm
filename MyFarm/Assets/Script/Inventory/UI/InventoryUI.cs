using Debug = UnityEngine.Debug;
using System;
using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MFarm.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        public ItemTooltip itemTooltip;//物品信息
        [Header("拖拽图片")]
        public Image dragItem;
        //玩家背包UI
        [Header("玩家背包")]
        [SerializeField] private GameObject bagUI;
        /// <summary>
        /// 背包开关此时状态
        /// </summary>
        private bool bagOpened;

        [Header("通用背包")]
        [SerializeField]private GameObject baseBag;//背包UI
        public GameObject shopSlotPerfab;
        public GameObject boxSlotPrefab;

        [Header("交易UI")]
        public TradeUI tradeUI;
        public TextMeshProUGUI playerMoneyText;//背包金钱text

        /// <summary>
        /// 26个格子
        /// </summary>
        [SerializeField] private SlotUI[] playerSlot;
        [SerializeField] private List<SlotUI> baseBagSlots;

        //注册事件
        private void OnEnable()
        {
            EventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent += OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI += OnShowTradeUI;
        }
        private void OnDisable()
        {
            EventHandler.UpdateInventoryUI -= OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent -= OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI -= OnShowTradeUI;
        }

        private void OnShowTradeUI(ItemDetails item, bool isSell)
        {
            tradeUI.gameObject.SetActive(true);
            tradeUI.SetupTradeUI(item, isSell);
        }

        /// <summary>
        /// 打开通用背包UI
        /// </summary>
        /// <param name="slotType"></param>
        /// <param name="bagData"></param>
        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            //判断是啥类型格子
            GameObject prefab = slotType switch
            {//有两种情况:1是商店的,那就要商店的格子,要会自动收缩,2是木头箱子的,不会自动收缩
                SlotType.Shop => shopSlotPerfab,
                SlotType.Box =>boxSlotPrefab,
                _ => null,
            };

            //生成基本背包UI
            baseBag.SetActive(true);
            //一个存着一堆格子的列表
            baseBagSlots = new List<SlotUI>();
            //有多少物品就生成多少个格子
            for (int i = 0; i < bagData.itemList.Count;i++){
                //生成格子
                var slot = Instantiate(prefab, baseBag.transform.GetChild(0)).GetComponent<SlotUI>();
                //格子是箱子里的第几个
                slot.slotIndex = i;
                //加入列表
                baseBagSlots.Add(slot);
            }
            //Debug.Log(baseBagSlots.Count);
            //加完格子要立即刷新,不然UI排序出问题
            LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());

            if(slotType == SlotType.Shop){
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(-1f, 0.5f);
                bagUI.SetActive(true);
                bagOpened = true;
            }

            //更新UI显示
            OnUpdateInventoryUI(InventoryLocation.Box, bagData.itemList);
        }

        private void Start()
        {
            //循环给26个格子赋值上Index
            for (int i = 0; i < playerSlot.Length; i++)
            {
                playerSlot[i].slotIndex = i;
            }
            //检测Hieracrchy中的玩家背包UI是否打开
            bagOpened = bagUI.activeInHierarchy;
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();//更新金钱
        }
        private void Update()
        {
            //用键盘控制背包UI的开启
            if (Input.GetKeyDown(KeyCode.B))
            {
                OpenbagUI();
            }
        }

        /// <summary>
        /// 关闭通用背包UI
        /// </summary>
        /// <param name="slotType"></param>
        /// <param name="bagData"></param>
        private void OnBaseBagCloseEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            baseBag.SetActive(false);//背包UI关闭
            itemTooltip.gameObject.SetActive(false);//物品信息UI关闭
            UpdateHighLightUI(-1);

            //一个存着一堆格子的列表中一个个格子删除
            foreach(var slot in baseBagSlots){
                Destroy(slot.gameObject);
            }
            baseBagSlots.Clear();
            if(slotType == SlotType.Shop){
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                bagUI.SetActive(false);
                bagOpened = false;
            }
        }

        private void OnBeforeSceneUnloadEvent()
        {
            UpdateHighLightUI(-1);
        }

        /// <summary>
        /// 更新背包UI
        /// </summary>
        /// <param name="location"></param>
        /// <param name="list"></param>
        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
        {
            switch (location)
            {
                //PLayer的背包
                case InventoryLocation.Player:
                    for (int i = 0; i < playerSlot.Length; i++)
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            playerSlot[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            playerSlot[i].UpdateEmptySlot();
                        }
                    }
                    break;
                //商店和箱子的背包
                case InventoryLocation.Box:
                    for (int i = 0; i < baseBagSlots.Count; i++)
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            baseBagSlots[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            baseBagSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;
            }
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();//更新金钱
        }

        /// <summary>
        /// 打开背包UI
        /// </summary>
        public void OpenbagUI()
        {
            bagOpened = !bagOpened;
            bagUI.SetActive(bagOpened);
        }
        /// <summary>
        /// 参数是点击哪个位置的物品槽,循环把其他槽的高亮关了只保留点中的物品槽高亮UI
        /// </summary>
        /// <param name="index"></param>
        public void UpdateHighLightUI(int index)
        {
            foreach (var slot in playerSlot)
            {
                if (slot.isSelected && slot.slotIndex == index)
                {
                    slot.slotHighlight.gameObject.SetActive(true);
                }
                else
                {
                    slot.isSelected = false;
                    slot.slotHighlight.gameObject.SetActive(false);
                }
            }
        }

    }
}

