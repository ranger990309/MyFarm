using System.Security.AccessControl;
using System;
using UnityEngine;

public class Settings
{
    public const float fadeDuration=0.35f;
    public const float TargetAlpha=0.45f;
    //时间相关
    public const float secondThreshold=0.01f;//数值越小时间越快??????????
    public const int secondHold=59;
    public const int minuteHold=59;
    public const int hourHold=23;
    public const int dayHold=30;
    public const int seasonHold=3;
    public const float fadeDurationImage=0.8f;
    public const int reapAmount = 2;//限制割草数量

    //NPC网格移动
    public const float gridCellSize = 1;//网格长度
    public const float gridCellDiagonalSize = 1.41f;//网格斜长度
    public const float pixelSize = 0.05f;//1像素的距离
    public const float animationBreakTime = 2f;//NPC停止等2s播放动画
    public const int maxGridSize = 9999;
    //灯光
    public const float lightChangeDuration = 25f;//转换时间
    public static TimeSpan morningTime = new TimeSpan(5, 0, 0);//早上5点
    public static TimeSpan nightTime = new TimeSpan(19, 0, 0);//晚上7点

    public static Vector3 playerStartPos = new Vector3(8.5f, -15.5f, 0);
    public const int playerStartMoney = 100;
}
