using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GameObject menuCanvas;
    public GameObject menuPrefab;
    public Button settingsBtn;//右上角按钮
    public GameObject pausePanel;
    public Slider volumeSlider;

    private void Awake() {
        settingsBtn.onClick.AddListener(TogglePausePanel);
        volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);//将slide和游戏声音绑定
    }

    private void OnEnable() {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }

    private void OnDisable() {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }
    private void Start() {
        menuCanvas = GameObject.FindWithTag("MenuCanvas");
        Instantiate(menuPrefab, menuCanvas.transform);
    }

    private void OnAfterSceneLoadedEvent()
    {
        if(menuCanvas.transform.childCount >0){
            Destroy(menuCanvas.transform.GetChild(0).gameObject);//开始游戏后删掉菜单面板
        }
    }

    private void TogglePausePanel(){
        bool isOpen = pausePanel.activeInHierarchy;//检测调声音大小哪个面板是否已经打开

        if(isOpen){
            pausePanel.SetActive(false);
            //timeScale为1.0时，时间是正常速度
            Time.timeScale = 1;
        }else{
            System.GC.Collect();//强制垃圾回收
            pausePanel.SetActive(true);
            //timeScale为0时，所有基于帧率的功能都将被暂停。，FixedUpdate函数不再执行。
            Time.timeScale = 0;
        }
    }

    public void ReturnMenuCanvas(){//返回主菜单
        Time.timeScale = 1;//要变成1要不协程不能运行
        StartCoroutine(BackToMenu());
    }

    private IEnumerator BackToMenu(){//返回主菜单
        pausePanel.SetActive(false);
        EventHandler.CallEndGameEvent();
        yield return new WaitForSeconds(1f);
        Instantiate(menuPrefab, menuCanvas.transform);
    }

}
