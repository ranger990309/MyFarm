using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : MonoBehaviour
{
    public CropDetails cropDetails;
    public TileDetails tileDetails;
    private int harvestActionCount;//收割次数
    public bool CanHarvest => (tileDetails.growthDays >= cropDetails.TotalGrowthDays);//是否可以收割
    private Animator anim;
    private Transform PlayerTransform => FindObjectOfType<Player>().transform;//????????????

    /// <summary>
    /// 执行收割方法
    /// </summary>
    /// <param name="tool"></param>
    public void ProcessToolAction(ItemDetails tool,TileDetails tile){
        tileDetails = tile;
        //工具使用次数
        int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);
        if(requireActionCount == -1) return;

        anim = GetComponentInChildren<Animator>();//????????????

        //判断是否有动画

        //点击计数器
        if(harvestActionCount<requireActionCount){
            harvestActionCount++;
            //判断是否有动画 树木
            if(anim != null && cropDetails.hasAnimation){
                if(PlayerTransform.position.x < transform.position.x){
                    anim.SetTrigger("RotateRight");
                }else{
                    anim.SetTrigger("RotateLeft");
                }
            }
            //播放粒子
            if(cropDetails.hasParticalEffect){
                EventHandler.CallParticleEffectEvent(cropDetails.effectType, transform.position + cropDetails.effectPos);
            }
            //播放声音
            if(cropDetails.soundEffect != SoundName.none){
                // var soundDetails = AudioManager.Instance.soundDetailsData.GetSoundDetails(cropDetails.soundEffect);//从音效库里找到对应名字的soundDetails
                // EventHandler.CallInitSoundEffect(soundDetails);//运行音效事件
                EventHandler.CallPlaySoundEvent(cropDetails.soundEffect);
            }
        }
        if(harvestActionCount >= requireActionCount){
            if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation)
            {
                //生成农作物到手
                SpawnHarvestItems();
            }else if(cropDetails.hasAnimation){
                if(PlayerTransform.position.x < transform.position.x){
                    anim.SetTrigger("FallingRight");
                }else{
                    anim.SetTrigger("FallingLeft");
                }
                EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
                StartCoroutine(HeavestAfterAnimation());
            }
        }
    }

    /// <summary>
    /// 在树倒下动画播放完后就收获
    /// </summary>
    /// <returns></returns>
    private IEnumerator HeavestAfterAnimation(){
        while(!anim.GetCurrentAnimatorStateInfo(0).IsName("END")){
            yield return null;
        }
        SpawnHarvestItems();
        //转换新物体
        if(cropDetails.transformItemID>0){
            CreateTransferCrop();
        }
    }

    private void CreateTransferCrop(){
        tileDetails.seedItemID = cropDetails.transformItemID;
        tileDetails.daysSinceLostHarvest = -1;
        tileDetails.growthDays = 0;
        EventHandler.CallRefreshCurrentMap();
    }

    /// <summary>
    /// 作物果实生成到手,植物二次结果
    /// </summary>
    public void SpawnHarvestItems(){
        for (int i = 0; i < cropDetails.producedItemID.Length; i++)
        {
            Debug.Log("1");
            int amountToProduce;
            if(cropDetails.producedMinAmount[i] == cropDetails.producedMaxAmount[i]){
                //代表只生成指定数量的
                amountToProduce = cropDetails.producedMinAmount[i];
            }else{ //物品随机数量
                amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i] + 1);
            }
            //执行生成指定数量的物品
            for (int j = 0; j < amountToProduce; j++)
            {
                //作物属性可以拿在手上的就拿在手上
                if(cropDetails.generateAtPlayerPosition){
                    EventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[i]);
                }else{ //地图上生成物品
                    //判断生成的物品方向
                    var dirX = transform.position.x > PlayerTransform.position.x ? 1 : -1;
                    //一定范围内随机
                    var spawnPos = new Vector3(transform.position.x + Random.Range(dirX, cropDetails.spawnRadius.x * dirX),
                    transform.position.y + Random.Range(-cropDetails.spawnRadius.y, cropDetails.spawnRadius.y), 0);

                    EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], spawnPos);
                }
            }
        }
        
        if(tileDetails != null){
            tileDetails.daysSinceLostHarvest++;

            //是否可以重复生长
            if(cropDetails.daysToRegrow > 0 && tileDetails.daysSinceLostHarvest < cropDetails.regrowTimes){
                tileDetails.growthDays = cropDetails.TotalGrowthDays - cropDetails.daysToRegrow;//??????????????????
                //刷新种子,刷新地图
                EventHandler.CallRefreshCurrentMap();
            }else{ //不可重复生长
                tileDetails.daysSinceLostHarvest = -1;
                tileDetails.seedItemID = -1;
                //
            }
            Destroy(gameObject);

        }
    }


}
