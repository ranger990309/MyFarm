using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Inventory
{
    public class ItemPickedUp : MonoBehaviour
    {
        private void OnTriggerExit2D(Collider2D other)
        {
            Item item = other.GetComponent<Item>();
            if (item != null)
            {
                if (item.itemDetails.canPickedup)
                {
                    //拾取物品添加进背包
                    InventoryManager.Instance.AddItem(item, true);

                    //播放音效
                    EventHandler.CallPlaySoundEvent(SoundName.Pickup);
                }
            }
        }
    }
}
