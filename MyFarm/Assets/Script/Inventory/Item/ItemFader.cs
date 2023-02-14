using DG.Tweening;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class ItemFader : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 树木由半透明变回不透明(人出去的时候)
    /// </summary>
    public void FadeIn()
    {
        Color TargetColor = new Color(1, 1, 1, 1);
        spriteRenderer.DOColor(TargetColor, Settings.fadeDuration);
    }

    /// <summary>
    /// 树木变半透明(人进来的时候)
    /// </summary>
    public void FadeOut()
    {
        Color TargetColor = new Color(1, 1, 1, Settings.TargetAlpha);
        spriteRenderer.DOColor(TargetColor, Settings.fadeDuration);
    }
}
