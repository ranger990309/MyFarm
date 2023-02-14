using System;
using System.Security.Authentication.ExtendedProtection;
using Cinemachine;
using UnityEngine;

public class SwitchBounds : MonoBehaviour
{
    //切换场景后调用(旧场景删除新场景通过加载时)
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += SwitchConfinerShape;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= SwitchConfinerShape;
    }

    private void SwitchConfinerShape()
    {
        //摄像机视野限制
        PolygonCollider2D confinerShape = GameObject.FindGameObjectWithTag("BoundsConfiner").GetComponent<PolygonCollider2D>();
        CinemachineConfiner confiner = GetComponent<CinemachineConfiner>();
        //摄像机视野限制
        confiner.m_BoundingShape2D = confinerShape;
        confiner.InvalidatePathCache();
    }
}
