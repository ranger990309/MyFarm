using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.CropPlant
{
    public class ReapItem : MonoBehaviour
    {
        private CropDetails cropDetails;
        private Transform PlayerTransform => FindObjectOfType<Player>().transform;

        public void InitCropData(int ID){
            cropDetails = CropManager.Instance.GetCropDetails(ID);
        }

        /// <summary>
        /// 作物果实生成到手
        /// </summary>
        public void SpawnHarvestItems()
        {
            Debug.Log("10");
            for (int i = 0; i < cropDetails.producedItemID.Length; i++)
            {
                int amountToProduce;
                if (cropDetails.producedMinAmount[i] == cropDetails.producedMaxAmount[i])
                {
                    //代表只生成指定数量的
                    amountToProduce = cropDetails.producedMinAmount[i];
                }
                else
                { //物品随机数量
                    amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i] + 1);
                }
                //执行生成指定数量的物品
                for (int j = 0; j < amountToProduce; j++)
                {
                    //作物属性可以拿在手上的就拿在手上
                    if (cropDetails.generateAtPlayerPosition)
                    {
                        EventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[i]);
                    }
                    else
                    { //地图上生成物品
                      //判断生成的物品方向
                        var dirX = transform.position.x > PlayerTransform.position.x ? 1 : -1;
                        //一定范围内随机
                        var spawnPos = new Vector3(transform.position.x + Random.Range(dirX, cropDetails.spawnRadius.x * dirX),
                        transform.position.y + Random.Range(-cropDetails.spawnRadius.y, cropDetails.spawnRadius.y), 0);

                        EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], spawnPos);
                    }
                }
            }
        }
    }
}

