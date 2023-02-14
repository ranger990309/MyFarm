using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Inventory{
    public class ItemBounce : MonoBehaviour{
        /// <summary>
        /// 木头子体位置
        /// </summary>
        private Transform spriteTrans;
        private BoxCollider2D coll;
        public float gravity = -3.5f;
        private bool isGround;
        /// <summary>
        /// 人物脚底到目标点的距离
        /// </summary>
        private float distance;
        /// <summary>
        /// 飞的方向
        /// </summary>
        private Vector2 direction;
        private Vector3 targetPos;
        private void Awake() {
            spriteTrans = transform.GetChild(0);
            coll = GetComponent<BoxCollider2D>();
            coll.enabled = false;
        }
        private void Update() {
            Bounce();
        }
        /// <summary>
        /// 将木头字体生成在头顶准备飞出去
        /// </summary>
        /// <param name="target"></param>
        /// <param name="dir"></param>
        public void InitbounceItem(Vector3 target,Vector2 dir){
            coll.enabled = false;
            direction = dir;
            targetPos = target;
            //人物脚底到目标点的距离
            distance = Vector3.Distance(target, transform.position);
            //这是将子体的高度从脚底抬到头顶那里
            spriteTrans.position += Vector3.up * 1.5f;
        }

        /// <summary>
        /// 木头飞的过程
        /// </summary>
        private void Bounce(){
            //影子的坐标代表母体的坐标,影子和1子体重合就是到地面了
            isGround = (spriteTrans.position.y <= transform.position.y);
            //母体还没到目标地就一直飞
            if (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position += (Vector3)direction * distance * -gravity * Time.deltaTime;
            }
            
            //还没到地面时高度也要不断下降,下到了就重定义字体位置和母体重合
            if(!isGround){
                spriteTrans.position += Vector3.up * gravity * Time.deltaTime;
            }else{
                spriteTrans.position = transform.position;
                coll.enabled = true;
            }
        }
    }
}

