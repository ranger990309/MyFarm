using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MFarm.Save;

namespace MFarm.Transition
{
    public class TransitionManager : Singleton<TransitionManager>,ISaveable
    {
        [SceneName]
        public string startSceneName = string.Empty;

        private CanvasGroup fadeCanvasGroup;//渐入渐出

        private bool isFade;

        public string GUID => GetComponent<DataGUID>().guid;

        private void OnEnable()
        {
            EventHandler.TransitionEvent += OnTransitionEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.TransitionEvent -= OnTransitionEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void OnEndGameEvent()
        {
            UnloadScene();
        }

        private void OnStartNewGameEvent(int obj)
        {
            StartCoroutine(LoadSaveDataScene(startSceneName));
        }

        private void OnTransitionEvent(string sceneToGo, Vector3 positionToGo)
        {
            if (!isFade)
            {
                StartCoroutine(Transition(sceneToGo, positionToGo));
            }
        }

        protected override void Awake(){
            base.Awake();
            SceneManager.LoadScene("UI", LoadSceneMode.Additive);//在游戏的一开始先加载UI场景
        }

        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();

            fadeCanvasGroup = FindObjectOfType<CanvasGroup>(); 
        }

        /// <summary>
        /// 场景切换(到目标位置)
        /// </summary>
        /// <param name="目标场景"></param>
        /// <param name="目标位置"></param>
        /// <returns></returns>
        private IEnumerator Transition(string sceneName, Vector3 targetPosition)
        {
            EventHandler.CallBeforeSceneUnloadEvent();
            yield return Fade(1);
            //将现有运行的场景直接开除场景库
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            yield return LoadSceneSetActive(sceneName);//加载场景(sceneName)并设置为激活
            EventHandler.CallMoveToPosition(targetPosition);//记载新场景后移动人物到新坐标
            EventHandler.CallAfterSceneLoadedEvent();
            yield return Fade(0);
        }

        /// <summary>
        /// 加载场景并设置为激活
        /// </summary>
        /// <param name="场景名"></param>
        /// <returns></returns>
        private IEnumerator LoadSceneSetActive(string sceneName)
        {
            //在后台异步加载(加一个)场景(sceneName)(在原有场景的情况下)。
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            //因为上一句代码在所有场景的最后加了个sceneName,那sceneName就在场景库的最后了,这句的意思得到场景库中的sceneName场景,
            Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            SceneManager.SetActiveScene(newScene);//设newScene场景状态为活动
        }

        /// <summary>
        /// 画面渐入渐出
        /// </summary>
        /// <param name="targetAlpha"></param>
        /// <returns></returns>
        private IEnumerator Fade(float targetAlpha)
        {
            isFade = true;
            fadeCanvasGroup.blocksRaycasts = true;
            //渐变速度
            float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / Settings.fadeDurationImage;
            //比较两个(现有透明度和目标透明度)浮点的数值,判断其是否相近,若不相近,则执行渐变
            while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
            {
                fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                yield return null;
            }
            fadeCanvasGroup.blocksRaycasts = false;
            isFade = false;

        }

        /// <summary>
        /// 加载存档游戏场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private IEnumerator LoadSaveDataScene(string sceneName){
            yield return Fade(1f);//场景先变黑

            if (SceneManager.GetActiveScene().name != "PersistentScene")//在游戏过程中,加载另外游戏进度
            { 
                EventHandler.CallBeforeSceneUnloadEvent();//先保存本场景数据
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);//然后将当前的场景卸载掉
            }

            yield return LoadSceneSetActive(sceneName);
            EventHandler.CallAfterSceneLoadedEvent();
            yield return Fade(0);
        }

        private IEnumerator UnloadScene(){
            EventHandler.CallBeforeSceneUnloadEvent();
            yield return Fade(1);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);//将激活的场景卸载掉
            yield return Fade(0);
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.dataSceneName = SceneManager.GetActiveScene().name;
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            //加载游戏进度场景
            StartCoroutine(LoadSaveDataScene(saveData.dataSceneName));
        }
    }
}
