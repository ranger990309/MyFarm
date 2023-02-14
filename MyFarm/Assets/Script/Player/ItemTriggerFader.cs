using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 人碰到树变半透明,离开颜色就还原
/// </summary>
public class ItemTriggerFader : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        ItemFader[] faders = other.GetComponentsInChildren<ItemFader>();
        if (faders.Length > 0)
        {
            foreach (var i in faders)
            {
                i.FadeOut();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ItemFader[] faders = other.GetComponentsInChildren<ItemFader>();
        if (faders.Length > 0)
        {
            foreach (var i in faders)
            {
                i.FadeIn();
            }
        }
    }

}
