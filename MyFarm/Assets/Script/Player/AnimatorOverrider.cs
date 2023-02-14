using System.ComponentModel;
using Debug = UnityEngine.Debug;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Inventory;
public class AnimatorOverrider : MonoBehaviour
{
    private Animator[] animators;
    public SpriteRenderer holdItem;
    [Header("各部分动画列表")]
    public List<AnimatorType> animatorTypes;
    //字典:包含动画器和其名字
    private Dictionary<string, Animator> animatorNameDict = new Dictionary<string, Animator>();
    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>();

        foreach (var anim in animators)
        {
            animatorNameDict.Add(anim.name, anim);
        }
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
    }

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
    }

    private void OnHarvestAtPlayerPosition(int ID)
    {
        //收获举起来农作物的动画
        Sprite itemSprite = InventoryManager.Instance.GetItemDetails(ID).itemOnWorldSprite;
        if(holdItem.enabled == false){
            StartCoroutine(ShowItem(itemSprite));
        }
    }

    /// <summary>
    /// 举起收获物展示1s就收起来
    /// </summary>
    /// <param name="itemSprite"></param>
    /// <returns></returns>
    private IEnumerator ShowItem(Sprite itemSprite){
        holdItem.sprite = itemSprite;
        holdItem.enabled = true;
        yield return new WaitForSeconds(1f);
        holdItem.enabled = false;
    }

    private void OnBeforeSceneUnloadEvent()
    {
        holdItem.enabled = false;
        SwitchAnimator(PartType.None);
    }

    //WORKFLOW:不同的工具商品种子判断能不能举起然后运行动画
    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        //得知其是种子商品可以举起来
        PartType currentType = itemDetails.itemType switch
        {
            ItemType.seed => PartType.Carry,
            ItemType.Commodity => PartType.Carry,
            ItemType.ChopTool => PartType.Chop,
            ItemType.HoeTool => PartType.Hoe,
            ItemType.WaterTool => PartType.Water,
            ItemType.CollectTool => PartType.Collect,
            ItemType.breakTool => PartType.Break,
            ItemType.ReapTool => PartType.Reap,
            ItemType.Furniture => PartType.Carry,
            _ => PartType.None
        };
        if (isSelected == false)
        {
            currentType = PartType.None;
            holdItem.enabled = false;
        }
        else
        {

            if (currentType == PartType.Carry)
            {
                holdItem.sprite = itemDetails.itemOnWorldSprite;
                holdItem.enabled = true;
            }else{
                holdItem.enabled = false;
            }
        }
        //得知其是种子商品可以举起来后,那就开始改变动画,让其可以举起来
        SwitchAnimator(currentType);
    }

    /// <summary>
    /// 那就开始改变动画,让其可以举起来
    /// </summary>
    /// <param name="partType"></param>
    private void SwitchAnimator(PartType partType)
    {
        foreach (var item in animatorTypes)
        {

            //判断列表里几个动画哪个类型相符就播放哪个carry == carry
            if (item.partType == partType)
            {
                //将Arm变成Arm_Hold动画器然后播放
                animatorNameDict[item.partName.ToString()].runtimeAnimatorController = item.overrideController;
            }
        }
    }
}
