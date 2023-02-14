
using System.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.AStar;
using UnityEngine.SceneManagement;
using System;
using MFarm.Save;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class NPCMovement : MonoBehaviour,ISaveable
{
    public ScheduleDetaList_SO scheduleData;
    private SortedSet<ScheduleDetails> scheduleSet;
    private ScheduleDetails currentSchedule;
    //临时储存信息
    [SerializeField]private string currentScene;//NPC现处场景
    private string targetScene;//NPC目标场景
    private Vector3Int currentGridPosition;//NPC现处网格位置
    private Vector3Int targetGridPosition;//NPC目标网格位置
    private Vector3Int nextGridPosition;//
    private Vector3 nextWorldPostion;
    public string StartScene{ set => currentScene = value; }//这东西可以设置currentScene的值

    [Header("移动属性")]
    public float normalSpeed = 2f;
    public float minSpeed = 1;
    public float maxSpeed = 3;
    private Vector2 dir;
    public bool isMoving;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D coll;
    private Animator anim;
    private Grid grid;
    /// <summary>
    /// 栈:从起点到终点的步数
    /// </summary>
    private Stack<MovementStep> movementSteps;
    private Coroutine npcMoveRoutine;
    private bool isInitialised = false;//是否已经初始化过
    private bool npcMove;
    private bool sceneLoaded;//场景是否已经加载完成
    public bool interactable;//是否可以交流
    public bool isFirstLoad;
    private Season currentSeason;//现在季节

    //动画计时器
    private float animationBreakTime=2;//动画冷却时间为2s
    private bool canPlayStopAnimation;
    private AnimationClip stopAnimationClip;//动画片段
    public AnimationClip blankAnimationClip;//???????????
    private AnimatorOverrideController animOverride;//?????????????
    private TimeSpan GameTime => TimeManager.Instance.GameTime;//拿到了游戏时间

    public string GUID => GetComponent<DataGUID>().guid;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        movementSteps = new Stack<MovementStep>();

        animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController = animOverride;
        scheduleSet = new SortedSet<ScheduleDetails>();

        foreach(var schedule in scheduleData.scheduleList){
            scheduleSet.Add(schedule);
        }
    }

    private void OnEnable() {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.GameMinteEvent += OnGameMinuteEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable() {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.GameMinteEvent -= OnGameMinuteEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        isInitialised = false;
        isFirstLoad = true;
    }

    private void OnEndGameEvent()
    {
        sceneLoaded = false;
        npcMove = false;
        if(npcMoveRoutine != null){
            StopCoroutine(npcMoveRoutine);
        }
    }

    private void Start() {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }

    private void Update() {
        if(sceneLoaded){
            SwitchAnimation();
        }

        //计时器,animationBreakTime不断减少,小于0就可以播放动画了
        animationBreakTime -= Time.deltaTime;
        //Debug.Log(animationBreakTime);
        canPlayStopAnimation = (animationBreakTime <= 0);

    }

    private void FixedUpdate(){
        if (sceneLoaded){
            Movement();
        }
    }

    private void OnGameMinuteEvent(int minute, int hour,int day,Season season)
    {
        int time = (hour * 100) + minute;
        currentSeason = season;
        ScheduleDetails matchSchedule = null;
        foreach(var schedule in scheduleSet){
            if(schedule.Time == time){
                if(schedule.day != day && schedule.day != 0){
                    continue;
                }
                if(schedule.season != season){
                    continue;
                }
                matchSchedule = schedule;
            }else if(schedule.Time > time){
                break;
            }
        }
        if(matchSchedule != null){
            BuildPath(matchSchedule);
        }
    }

    private void OnBeforeSceneUnloadEvent()
    {
        sceneLoaded = false;
    }

    private void OnAfterSceneLoadedEvent()
    {
        grid = FindObjectOfType<Grid>();
        CheckVisiable();

        if(!isInitialised){
            InitNPC();
            isInitialised = true;
        }

        sceneLoaded = true;

        if (!isFirstLoad)//如果不是第一次加载游戏,那就是点存档进来的
        {
            currentGridPosition = grid.WorldToCell(transform.position);
            var schedule = new ScheduleDetails(0, 0, 0, 0, currentSeason, targetScene, (Vector2Int)targetGridPosition, stopAnimationClip, interactable);
            BuildPath(schedule);
            isFirstLoad = true;
        }
    }

    /// <summary>
    /// 若所处的场景事NPC本该出现的场景就显示NPC
    /// </summary>
    private void CheckVisiable(){
        if(currentScene == SceneManager.GetActiveScene().name){
            SetActiveInScene();
        }else{
            SetInactiveInScene();
        }
    }

    private void InitNPC(){
        targetScene = currentScene;

        //将NPC置于当前坐标的网格中心
        currentGridPosition = grid.WorldToCell(transform.position);
        transform.position = new Vector3(currentGridPosition.x + Settings.gridCellSize / 2f, currentGridPosition.y + Settings.gridCellSize / 2f, 0);

        targetGridPosition = currentGridPosition;//人物不动(终点就是现在脚下的坐标)
    }

    /// <summary>
    /// 主要移动方法
    /// </summary>
    private void Movement(){
        if (!npcMove)
        {
            if (movementSteps.Count > 0)
            {
                MovementStep step = movementSteps.Pop();//获得栈第一步
                //将NPC所在的场景设置为第一步的场景
                currentScene = step.sceneName;
                //显示NPC
                CheckVisiable();
                //拿到下一步网格坐标
                nextGridPosition = (Vector3Int)step.gridCoordinate;
                //拿到时间戳,将会在几时几分几秒到达step这个网格
                TimeSpan stepTime = new TimeSpan(step.hour, step.minute, step.second);
                //真实移动
                MoveToGridPosition(nextGridPosition, stepTime);
            }
            else if(!isMoving && canPlayStopAnimation){
                //Debug.Log(canPlayStopAnimation);
                StartCoroutine(SetStopAnimation());//面朝玩家
            }
        }
    }

    private void MoveToGridPosition(Vector3Int gridPos,TimeSpan stepTime){
        npcMoveRoutine = StartCoroutine(MoveRoutine(gridPos, stepTime));
    }

    //移动到下一步
    private IEnumerator MoveRoutine(Vector3Int gridPos,TimeSpan stepTime){
        npcMove = true;
        nextWorldPostion = GetWorldPosition(gridPos);
        //还有时间来移动
        if(stepTime > GameTime){
            //到达下一步所需要的时间差
            float timetoMove = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
            //到达下一步所需要的实际距离
            float distance = Vector3.Distance(transform.position, nextWorldPostion);
            //获得速度
            float speed = Mathf.Max(minSpeed, (distance / timetoMove / Settings.secondThreshold));

            if(speed <= maxSpeed){
                //NPC位置距离下一步的距离大于一个像素就认为还没到,要继续移动
                while (Vector3.Distance(transform.position, nextWorldPostion) > Settings.pixelSize)
                {
                    //方向
                    dir = (nextWorldPostion - transform.position).normalized;
                    //移动的距离
                    Vector2 posOffset = new Vector2(dir.x * speed * Time.fixedDeltaTime, dir.y * speed * Time.fixedDeltaTime);
                    //一次又一次移动一点距离,多了就到下一步了
                    rb.MovePosition(rb.position + posOffset);
                    //等待下一次FixedUpdate的执行就执行这里面的内容
                    yield return new WaitForFixedUpdate();
                }
            }
        }

        //如果时间已经到了就瞬移
        rb.position = nextWorldPostion;
        currentGridPosition = gridPos;
        nextGridPosition = currentGridPosition;
        npcMove = false;
    }

    /// <summary>
    /// 根据chedule构建路径
    /// </summary>
    /// <param name="schedule"></param>
    public void BuildPath(ScheduleDetails schedule){
        movementSteps.Clear();
        currentSchedule = schedule;
        targetScene = schedule.targetScene;
        targetGridPosition = (Vector3Int)schedule.targetGridPosition;
        stopAnimationClip = schedule.clipAtStop;
        this.interactable = schedule.interactable;

        //如果NPC要去的场景和现在所在的场景是同一个的话
        if(schedule.targetScene == currentScene){
            AStar.Instance.BuildPath(schedule.targetScene, (Vector2Int)currentGridPosition, schedule.targetGridPosition, movementSteps);
        }else if(schedule.targetScene != currentScene){//如果要去另一个场景
            //得到跨场景路线
            SceneRoute sceneRoute = NPCManager.Instance.GetSceneRoute(currentScene, schedule.targetScene);
            //Debug.Log(sceneRoute.scenePathList[0].sceneName +" "+sceneRoute.scenePathList[0].fromGridCell.ToString()+" "+sceneRoute.scenePathList[0].gotoGridCell.ToString());
            if (sceneRoute != null)
            {
                for (int i = 0; i < sceneRoute.scenePathList.Count; i++)
                {
                    Vector2Int fromPos, gotoPos;
                    ScenePath path = sceneRoute.scenePathList[i];

                    //若NPC的起始坐标是无限,那就重置为现在所处坐标
                    if (path.fromGridCell.x >= Settings.maxGridSize)
                    {
                        fromPos = (Vector2Int)currentGridPosition;
                        // Debug.Log("1"+currentGridPosition);
                    }
                    else
                    {//若NPC的起始坐标是明确的地图上某个点,那就某个点喽
                        fromPos = path.fromGridCell;
                        // Debug.Log("2"+path.fromGridCell);
                    }
                    
                    //若NPC的目标坐标是无限,那就重置为日程的目标位置
                    if (path.gotoGridCell.x >= Settings.maxGridSize)
                    {
                        gotoPos = schedule.targetGridPosition;
                        // Debug.Log("3"+schedule.targetGridPosition);
                    }
                    else
                    {//
                        gotoPos = path.gotoGridCell;
                        // Debug.Log("4"+path.gotoGridCell);
                    }
                    AStar.Instance.BuildPath(path.sceneName, fromPos, gotoPos, movementSteps);
                    // Debug.Log("---------------");
                    // foreach(MovementStep step in movementSteps){
                    //     Debug.Log(step.gridCoordinate);
                    // }
                    // Debug.Log("---------------");
                }
            }
        }
        
        if(movementSteps.Count > 1){
            //更新每一步对应的时间戳
            UpdateTimeOnPath();
        }
    }

    /// <summary>
    /// 每走一步都有其自己的时间,算几分几秒走到哪一步,是几分几秒到终点
    /// </summary>
    private void UpdateTimeOnPath(){
        MovementStep previousSetp = null;
        TimeSpan currentGameTime = GameTime;//游戏时间
        //每走一步都有其自己的时间,算几分几秒走到哪一步
        foreach(MovementStep step in movementSteps){
            if(previousSetp == null){
                //previousSetp代表下一步
                previousSetp = step;
            }

            step.hour = currentGameTime.Hours;
            step.minute = currentGameTime.Minutes;
            step.second = currentGameTime.Seconds;
            
            //每移动一格所需要的时间
            TimeSpan gridMovementStepTime;

            if(MoveInDiagonal(step,previousSetp)){
                //时间=距离/速度,算出走一步需要多少时间
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / normalSpeed / Settings.secondThreshold));
            }else{
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellSize / normalSpeed / Settings.secondThreshold));
            }

            //不断加上去,算出起点到终点需要多少时间,是几分几秒到终点
            //累加获得下一步的时间戳
            currentGameTime = currentGameTime.Add(gridMovementStepTime);
            //循环下一步
            previousSetp = step;

        }
    }

    /// <summary>
    /// 判断是否走斜方向
    /// </summary>
    /// <param name="currentStep"></param>
    /// <param name="previousStep"></param>
    /// <returns></returns>
    private bool MoveInDiagonal(MovementStep currentStep,MovementStep previousStep){
        return (currentStep.gridCoordinate.x != previousStep.gridCoordinate.x) && (currentStep.gridCoordinate.y != previousStep.gridCoordinate.y);
    }

    /// <summary>
    /// 网格坐标 返回 世界坐标中心
    /// </summary>
    /// <param name="网格坐标">网格坐标</param>
    /// <returns></returns>
    private Vector3 GetWorldPosition(Vector3Int gridPos){
        Vector3 worldPos = grid.CellToWorld(gridPos);
        return new Vector3(worldPos.x + Settings.gridCellSize / 2f, worldPos.y + Settings.gridCellSize / 2);
    }

    /// <summary>
    /// 动画
    /// </summary>
    private void SwitchAnimation(){
        //NPC的位置还没到目标位置,那移动开关就开
        isMoving = (transform.position != GetWorldPosition(targetGridPosition));

        anim.SetBool("isMoving", isMoving);
        if(isMoving){
            anim.SetBool("Exit", true);
            anim.SetFloat("DirX", dir.x);
            anim.SetFloat("DirY", dir.y);
        }else{
            anim.SetBool("Exit", false);
        }
    }

    /// <summary>
    /// 面朝玩家
    /// </summary>
    /// <returns></returns>
    private IEnumerator SetStopAnimation(){
        //强制面对镜头
        //anim.SetFloat("DirX", 0);
        //anim.SetFloat("DirY", -1);

        //还原计时器
        animationBreakTime = Settings.animationBreakTime;
        //播放动画
        if(stopAnimationClip != null){
            animOverride[blankAnimationClip] = stopAnimationClip;//???????????????
            anim.SetBool("EventAnimation", true);
            yield return null;
            anim.SetBool("EventAnimation", false);
        }else{
            animOverride[stopAnimationClip] = blankAnimationClip;//???????????????
            anim.SetBool("EventAnimation", false);
        }
    }

    #region 设置NPC的显示
    private void SetActiveInScene(){
        spriteRenderer.enabled = true;
        coll.enabled = true;
        //影子关闭
        transform.GetChild(0).gameObject.SetActive(false);
    }

    private void SetInactiveInScene(){
        spriteRenderer.enabled = false;
        coll.enabled = false;
        //影子关闭
        transform.GetChild(0).gameObject.SetActive(false);
    }
    #endregion

    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.charcacterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.charcacterPosDict.Add("targetGridPosition", new SerializableVector3(targetGridPosition));//XXX:这里的string不应该是人名吗?
        saveData.charcacterPosDict.Add("currentPosition", new SerializableVector3(transform.position));//XXX:这里的string不应该是人名吗?
        saveData.dataSceneName = currentScene;
        saveData.targetScene = this.targetScene;
        if(stopAnimationClip != null){
            saveData.animationInstanceID = stopAnimationClip.GetInstanceID();//储存动画片段,比如NPC这时在伸懒腰
        }
        saveData.interactable = this.interactable;
        saveData.timeDict = new Dictionary<string, int>();
        saveData.timeDict.Add("currentSeason", (int)currentSeason);
        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        isInitialised = true;
        isFirstLoad = false;
        currentScene = saveData.dataSceneName;
        targetScene = saveData.targetScene;

        Vector3 pos = saveData.charcacterPosDict["currentPosition"].ToVector3();
        Vector3Int gridPos = (Vector3Int)saveData.charcacterPosDict["targetGridPosition"].ToVector2Int();

        transform.position = pos;
        targetGridPosition = gridPos;

        if(saveData.animationInstanceID != 0){
            this.stopAnimationClip = Resources.InstanceIDToObject(saveData.animationInstanceID) as AnimationClip;
        }

        this.interactable = saveData.interactable;
        this.currentSeason = (Season)saveData.timeDict["currentSeason"];
    }
}
