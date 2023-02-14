using System.IO.Pipes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class LightControl : MonoBehaviour
{
    public LightPattenList_SO lightData;
    private Light2D currentLight;
    private LightDetails currentLightDetails;

    private void Awake() {
        currentLight = GetComponent<Light2D>();
    }

    /// <summary>
    /// 实际切换灯光
    /// </summary>
    /// <param name="season"></param>
    /// <param name="lightShift"></param>
    /// <param name="timeDifference"></param>
    public void ChangeLightShift(Season season,LightShift lightShift,float timeDifference){
        //得到这个季节这个晚上早上的灯光应有颜色
        currentLightDetails = lightData.GetLightDetails(season, lightShift);
        if(timeDifference < Settings.lightChangeDuration){//早上5点到5点25分慢慢改变颜色
            ////颜色差值/时间插值
            var colorOffset = (currentLightDetails.lightColor - currentLight.color) / Settings.lightChangeDuration * timeDifference;
            currentLight.color += colorOffset;
            //转换颜色
            DOTween.To(() => currentLight.color, c => currentLight.color = c, currentLightDetails.lightColor, Settings.lightChangeDuration - timeDifference);
            //慢慢改变亮度
            DOTween.To(() => currentLight.intensity, i => currentLight.intensity = i, currentLightDetails.lightAmount, Settings.lightChangeDuration - timeDifference);
        }
        if(timeDifference > Settings.lightChangeDuration){//过了5点25分,一下子赋值
            currentLight.color = currentLightDetails.lightColor;
            currentLight.intensity = currentLightDetails.lightAmount;
        }
    }
}
