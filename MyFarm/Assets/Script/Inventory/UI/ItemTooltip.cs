using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MFarm.Inventory;

public class ItemTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Text valueText;
    [SerializeField] private GameObject bottomPart;
    [Header("建造")]
    public GameObject resourcePanel;//提示图的建材
    [SerializeField] private Image[] resourceItem;//提示里只放得下3个图片

    /// <summary>
    /// 设置信息面板
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <param name="slotType"></param>
    public void SetupTooltip(ItemDetails itemDetails, SlotType slotType)
    {
        nameText.text = itemDetails.itemName;
        typeText.text = GetItemType(itemDetails.itemType);
        descriptionText.text = itemDetails.itemDescription;
        //若物品是种子商品等就会有售卖价格
        if (itemDetails.itemType == ItemType.seed || itemDetails.itemType == ItemType.Commodity || itemDetails.itemType == ItemType.Furniture)
        {
            bottomPart.SetActive(true);
            var price = itemDetails.itemPrice;
            if (slotType == SlotType.Bag)
            {
                price = (int)(price * itemDetails.sellPercentage);
            }
            valueText.text = price.ToString();
        }
        else
        {
            bottomPart.SetActive(false);
        }
        //有时信息面板解释又两行变回一行时有延迟来不及刷新,这里强制刷新一下
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
    /// <summary>
    /// 将物品类型的中文打出来
    /// </summary>
    /// <param name="itemType"></param>
    /// <returns></returns>
    private string GetItemType(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.seed => "种子",
            ItemType.Commodity => "商品",
            ItemType.Furniture => "家具",
            ItemType.breakTool => "工具",
            ItemType.ChopTool => "工具",
            ItemType.HoeTool => "工具",
            ItemType.ReapTool => "工具",
            ItemType.WaterTool => "工具",
            ItemType.CollectTool => "工具",
            _ => "无"
        };
    }

    /// <summary>
    /// 设置提示框所需要的建材
    /// </summary>
    /// <param name="bluePrintDatails"></param>
    public void SetupResourcePanel(int ID){
        var bluePrintDatails = InventoryManager.Instance.bluePrintData.GetBluePrintDatails(ID);
        for (int i = 0; i < resourceItem.Length;i++){//提示里的图片不得超过三个
            if(i<bluePrintDatails.resourceItem.Length){//建造物品所需要的建材
                var item = bluePrintDatails.resourceItem[i];
                resourceItem[i].gameObject.SetActive(true);//显示木材
                resourceItem[i].sprite = InventoryManager.Instance.GetItemDetails(item.itemID).itemIcon;//显示木材
                resourceItem[i].transform.GetChild(0).GetComponent<Text>().text = item.itemAmount.ToString();
            }else{
                resourceItem[i].gameObject.SetActive(false);
            }
        }        
    }
}
