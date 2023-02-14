
using UnityEngine;
using TMPro;
using UnityEngine.UI;
//点按的事件我们需要事件的接口
using UnityEngine.EventSystems;


namespace MFarm.Inventory
{
    //开始拖拽物品的接口
    public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        //组件获取
        [Header("组件获取")]

        [SerializeField] private Image slotImage;
        [SerializeField] private TextMeshProUGUI amountText;
        public Image slotHighlight;
        [SerializeField] private Button button;
        //格子类型
        [Header("格子类型")]
        public SlotType slotType;
        //是否被选中判定
        public bool isSelected;

        public InventoryLocation Location{
            get{
                return slotType switch
                {
                    SlotType.Bag => InventoryLocation.Player,
                    SlotType.Box => InventoryLocation.Box,
                    _ => InventoryLocation.Player
                };
            }
        }
        
        //格子物品信息
        public ItemDetails itemDetails;
        //物品数量
        public int itemAmount;
        //格子是在箱子里的第几个
        public int slotIndex;
        public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();
        private void Start()
        {
            //一开始先将按钮互动给关了
            button.interactable = false;
            //若格子为空,则运行相关方法???????
            if (itemDetails.itemID == null)
            {
                UpdateEmptySlot();
            }
        }

        //方法:更新slot(物品,数量)
        public void UpdateSlot(ItemDetails item, int amount)
        {
            itemDetails = item;
            slotImage.sprite = itemDetails.itemIcon;
            itemAmount = amount;
            amountText.text = amount.ToString();
            slotImage.enabled = true;
            button.interactable = true;
        }


        /// <summary>
        /// 当slot为空时
        /// </summary>
        public void UpdateEmptySlot()
        {
            //当slot为空时,如果被选中则要求不显示灰框,无动效
            if (isSelected)
            {
                isSelected = false;
                inventoryUI.UpdateHighLightUI(-1);
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
            }
            //关闭框内可以放image的功能
            slotImage.enabled = false;
            //数量也设置为空
            amountText.text = string.Empty;
            //按钮的互动也关了
            button.interactable = false;
        }

        /// <summary>
        /// 点击物品槽会发生的事
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            //若物品槽没东西,点击没反应
            if (itemAmount == null) return;
            //这下物品槽有东西了,就会在选中状态和未选中状态之间切换
            isSelected = !isSelected;
            //将指定位置的高光的开关打开或关闭
            inventoryUI.UpdateHighLightUI(slotIndex);
            //点击槽有东西会触发事件动画把东西举起(这槽需要是下栏和背包才行)
            if (slotType == SlotType.Bag)
            {
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
            }

        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            //当本框数量不为0时,就说明有物体,就把拖拽的功能打开图片给上再设个本地尺寸,
            if (itemAmount != 0)
            {
                inventoryUI.dragItem.enabled = true;
                inventoryUI.dragItem.sprite = slotImage.sprite;
                inventoryUI.dragItem.SetNativeSize();
                isSelected = true;
                inventoryUI.UpdateHighLightUI(slotIndex);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.enabled = false;
            //Debug.Log(eventData.pointerCurrentRaycast.gameObject);
            //先判断是不是空的点到地面了,再判断点到的是不是有SlotUI组件
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null)
                    return;

                //拿到目标点的slotUI组件
                var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
                //再拿到目标的index
                int targetIndex = targetSlot.slotIndex;

                //鼠标拖拽物品从player包栏到包栏,内部流转
                if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
                {
                    InventoryManager.Instance.SwapItem(slotIndex, targetIndex);
                }else if(slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag){ //买
                    EventHandler.CallShowTradeUI(itemDetails, false);//买东西,false就是买
                }else if(slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop){ //卖
                    EventHandler.CallShowTradeUI(itemDetails, true);//卖东西,true就是卖
                }else if(slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType){ //为什么不直接用Box
                    //跨背包数据交换物品
                    InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex);
                }

                //清空所有高亮
                inventoryUI.UpdateHighLightUI(-1);

            }
            else
            {//测试扔在地上
                if (itemDetails.canDropped)
                {
                    var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
                    //事件:生成物品在地面,需要物品ID和坐标
                    EventHandler.CallInstantiateItemInScene(itemDetails.itemID, pos);
                }

            }
        }
    }
}
