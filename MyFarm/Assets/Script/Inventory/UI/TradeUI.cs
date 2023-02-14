using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace MFarm.Inventory
{
    public class TradeUI : MonoBehaviour
    {
        public Image itemIcon;
        public Text itemName;
        public InputField tradeAmount;
        public Button submitButton;
        public Button cancelButton;

        private ItemDetails item;
        private bool isSellTrade;

        private void Awake()
        {
            //点关闭按钮直接关闭
            cancelButton.onClick.AddListener(CancelTrade);
            submitButton.onClick.AddListener(TradeItem);//点√进行交易
        }

        /// <summary>
        /// 设置TradeUI显示详细
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isSell"></param>
        public void SetupTradeUI(ItemDetails item, bool isSell)
        {
            this.item = item;
            itemIcon.sprite = item.itemIcon;
            itemName.text = item.itemName;
            isSellTrade = isSell;
            tradeAmount.text = string.Empty;
        }

        //交易物品
        private void TradeItem(){
            var amount = Convert.ToInt32(tradeAmount.text);//取得输入框交易数量
            InventoryManager.Instance.TradeItem(item, amount, isSellTrade);//更新价格背包物品
            CancelTrade();// 关闭交易窗口
        }

        /// <summary>
        /// 关闭交易窗口
        /// </summary>
        private void CancelTrade()
        {
            this.gameObject.SetActive(false);
        }
    }
}
