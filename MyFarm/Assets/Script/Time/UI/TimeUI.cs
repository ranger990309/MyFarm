using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class TimeUI : MonoBehaviour
{
    /// <summary>
    /// 白天黑夜大图标RectTransform
    /// </summary>
    public RectTransform datNightImage;
    /// <summary>
    /// 时间大图标RectTransform
    /// </summary>
    public RectTransform clockParent;
    /// <summary>
    /// 季节小图标Image
    /// </summary>
    public Image seasonImage;
    /// <summary>
    /// 几月几日右上角Text
    /// </summary>
    public TextMeshProUGUI dataText;
    /// <summary>
    /// 几点右上角Text
    /// </summary>
    public TextMeshProUGUI timeText;
    /// <summary>
    /// 4季小图标Sprite[]
    /// </summary>
    public Sprite[] seasonSprites;
    /// <summary>
    /// 6个时间块List(List GameObject)
    /// </summary>
    /// <typeparam name="GameObject"></typeparam>
    /// <returns></returns>
    private List<GameObject> clockBlocks = new List<GameObject>();

    private void Awake()
    {
        //将6个时间块加入clockBlocks,并全部关闭时间块.
        for (int i = 0; i < clockParent.childCount; i++)
        {
            clockBlocks.Add(clockParent.GetChild(i).gameObject);
            clockParent.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        EventHandler.GameMinteEvent += OnGameMinteEvent;
        EventHandler.GameDataEvent += OnGameDataEvent;
    }
    private void OnDisable()
    {
        EventHandler.GameMinteEvent -= OnGameMinteEvent;
        EventHandler.GameDataEvent -= OnGameDataEvent;
    }
    private void OnGameDataEvent(int hour, int day, int month, int year, Season season)
    {
        dataText.text = year + "年" + month.ToString("00") + "月" + day.ToString("00") + "日";
        seasonImage.sprite = seasonSprites[(int)season];
        SwitchHourImage(hour);//每过4小时就有一个时间块亮起,
        DayNightImageRotate(hour);//右上角那圆盘每6小时转90度
    }

    private void OnGameMinteEvent(int minute, int hour,int day,Season season)
    {
        timeText.text = hour.ToString("00") + ":" + minute.ToString("00");
    }

    /// <summary>
    /// 每过4小时就有一个时间块亮起,
    /// </summary>
    /// <param name="hour"></param>
    private void SwitchHourImage(int hour)
    {
        int index = hour / 4;
        if (index == 0)
        {
            foreach (var item in clockBlocks)
            {
                item.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < clockBlocks.Count; i++)
            {
                if (i < index)
                {
                    clockBlocks[i].SetActive(true);
                }
                else
                {
                    clockBlocks[i].SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 右上角那圆盘每6小时转90度
    /// </summary>
    /// <param name="hour"></param>
    private void DayNightImageRotate(int hour)
    {
        var target = new Vector3(0, 0, hour * 15 - 90);
        //
        datNightImage.DORotate(target, 1f, RotateMode.Fast);
    }

}
