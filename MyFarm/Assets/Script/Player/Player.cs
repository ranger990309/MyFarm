using Debug = UnityEngine.Debug;
using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Save;

public class Player : MonoBehaviour,ISaveable
{
    private Rigidbody2D rb;

    public float speed;

    private float inputX;

    private float inputY;

    private Vector2 movementInput;

    private Animator[] animators;

    private bool isMoving;

    private bool inputDisable;
    //动画实用工具
    private float mouseX;
    private float mouseY;
    private bool useTool;

    public string GUID => GetComponent<DataGUID>().guid;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>();
        inputDisable = false;//一开始不能让Player动
    }

    private void Start() {
        ISaveable saveable = this;
        saveable.RegisterSaveable();//将这一份ISaveable代码加入到List<ISaveable>中
    }

    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaderEvent;//你点击运行事件直接就启动了
        EventHandler.MoveToPosition += OnMoveToPosition;
        EventHandler.MouseClickedEvent += OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaderEvent;
        EventHandler.MoveToPosition -= OnMoveToPosition;
        EventHandler.MouseClickedEvent -= OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void OnEndGameEvent()
    {
        inputDisable = true;
    }

    private void OnStartNewGameEvent(int obj)
    {
        inputDisable = false;//新游戏一开始是不能控制人物的
        transform.position = Settings.playerStartPos;
    }

    private void OnUpdateGameStateEvent(GameState gameState)
    {
        switch(gameState){
            case GameState.GamePlay:
                inputDisable = false;//游戏进行时可以移动
                break;
            case GameState.Pause:
                inputDisable = true;//游戏暂停时不可移动
                break;
        }
    }

    private void OnMouseClickedEvent(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {

        if(useTool) return;

        //执行动画
        if(itemDetails.itemType != ItemType.seed && itemDetails.itemType != ItemType.Commodity && itemDetails.itemType != ItemType.Furniture){
            //鼠标距离脚底x轴距离
            mouseX = mouseWorldPos.x - transform.position.x;
            mouseY = mouseWorldPos.y - (transform.position.y +0.85f);
            //此处我怀疑如果鼠标偏x轴一点锄头就会水平锄地
            if(Mathf.Abs(mouseX) > Mathf.Abs(mouseY)){
                mouseY = 0;
            }else{
                mouseY = 0;
            }
            StartCoroutine(UseToolRoutine(mouseWorldPos, itemDetails));
        }else{
            //在播放了(砍树)动画后实行的(砍树)数据的变现
            EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        }
    }

    /// <summary>
    /// 动画执行,而后执行实际效果
    /// </summary>
    /// <param name="mouseWorldPos"></param>
    /// <param name="itemDetails"></param>
    /// <returns></returns>
    private IEnumerator UseToolRoutine(Vector3 mouseWorldPos,ItemDetails itemDetails){
        useTool = true;
        inputDisable = true;
        yield return null;
        foreach(var anim in animators){
            anim.SetTrigger("useTool");
            //人物的面朝方向
            anim.SetFloat("InputX", mouseX);
            anim.SetFloat("InputY", mouseY);
        }
        yield return new WaitForSeconds(0.45f);
        //在播放了(砍树)动画后实行的(砍树)数据的变现
        EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        yield return new WaitForSeconds(0.25f);
        //等待动画结束
        useTool = false;
        inputDisable = false;
    }

    private void OnAfterSceneLoaderEvent()
    { 
        inputDisable = false;
    }

    private void OnBeforeSceneUnloadEvent()
    {

        inputDisable = true;
    }

    private void OnMoveToPosition(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            EventHandler.CallAfterSceneLoadedEvent();//搞定
        }

        if (!inputDisable)
        {
            PlayerInput();
        }
        else
        {
            isMoving = false;
        }
        SwitchAnimation();
    }

    private void FixedUpdate()
    {
        if (!inputDisable)
        {
            Movement();
        }
    }

    /// <summary>
    /// 玩家键盘wasd输入值
    /// </summary>
    private void PlayerInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");
        if (inputX != 0 && inputY != 0)
        {
            inputX = inputX * Mathf.Sqrt(1 - (inputX * inputX) / 2);
            inputY = inputY * Mathf.Sqrt(1 - (inputY * inputY) / 2);
        }

        //走路状态速度
        if (Input.GetKey(KeyCode.LeftShift))
        {
            inputX = inputX * 0.5f;
            inputY = inputY * 0.5f;
        }

        movementInput = new Vector2(inputX, inputY);
        //只要键盘有按下isMoving就为ture
        isMoving = (movementInput != Vector2.zero);
    }

    /// <summary>
    /// 根据键盘输入值算出移动速度
    /// </summary>
    private void Movement()
    {
        rb.MovePosition(rb.position + movementInput * speed * Time.deltaTime);
    }

    /// <summary>
    /// 切换动画InputX和InputY赋值
    /// </summary>
    private void SwitchAnimation()
    {
        foreach (var anim in animators)
        {
            anim.SetBool("isMoving", isMoving);
            anim.SetFloat("mouseX", mouseX);
            anim.SetFloat("mouseY", mouseY);
            if (isMoving)
            {
                anim.SetFloat("InputX", inputX);
                anim.SetFloat("InputY", inputY);
            }
        }
    }

    public GameSaveData GenerateSaveData()//生成一个GameSaveData存档信息
    {
        GameSaveData saveData = new GameSaveData();
        saveData.charcacterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.charcacterPosDict.Add(this.name, new SerializableVector3(transform.position));//加入Player名字和地点

        return saveData;
    }

    public void RestoreData(GameSaveData saveData)//将存档里存的东西拿出来
    {
        var targetPosition = saveData.charcacterPosDict[this.name].ToVector3();
        transform.position = targetPosition;
    }
}
