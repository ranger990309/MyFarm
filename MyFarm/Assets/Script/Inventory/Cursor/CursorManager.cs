using System.Security.Cryptography;
using MFarm.CropPlant;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MFarm.Map;
using MFarm.Inventory;

public class CursorManager : MonoBehaviour
{
    public Sprite normal, tool, seed, item;
    private Sprite currentSprite;//储存当前鼠标图片
    private Image cursorImage;
    private RectTransform cursorCanvas;
    //建造鼠标跟随
    private Image buildImage;
    //鼠标检测
    private Camera mainCamera;
    private Grid currentGrid;
    private Vector3 mouseWorldPos;
    private Vector3Int mouseGridPos;
    /// <summary>
    /// 鼠标是否可用
    /// </summary>
    private bool cursorEnable;
    /// <summary>
    /// 鼠标是否是可点区域
    /// </summary>
    private bool cursorPositionValid;
    /// <summary>
    /// 在背包中被选中的物品
    /// </summary>
    private ItemDetails currentItem;
    private Transform PlayerTransform => FindObjectOfType<Player>().transform;

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;

    }
    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }

    private void OnBeforeSceneUnloadEvent()
    {
        cursorEnable = false;
    }
    private void OnAfterSceneLoadedEvent()
    {
        currentGrid = FindObjectOfType<Grid>();
    }

    private void Start()
    {
        cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
        cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();
        //拿到建筑图标
        buildImage = cursorCanvas.GetChild(1).GetComponent<Image>();
        buildImage.gameObject.SetActive(false);

        currentSprite = normal;
        SetCursorImage(normal);

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (cursorCanvas == null) return;
        //鼠标图片跟随鼠标一起移动
        cursorImage.transform.position = Input.mousePosition;
        //其他类型鼠标是否与UI(背包)有互动,有则变回去
        if (!InteractWithUI()&&cursorEnable)
        {
            SetCursorImage(currentSprite);
            //鼠标放在土地上则取得瓦片信息(鼠标移到背包上就不行)
            CheckCursorVaild();
            //点击鼠标左键丢东西
            CheckPlayerInput();
        }
        else
        {//如果鼠标移动到UI上
            SetCursorImage(normal);
            //buildImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 看看玩家都按了什么键
    /// </summary>
    private void CheckPlayerInput(){
        //点击了鼠标左键,传入背包被选中的物品
        if(Input.GetMouseButtonDown(0) && cursorPositionValid){
            EventHandler.CallMouseClickedEvent(mouseWorldPos, currentItem);
        }
    }

    /// <summary>
    /// 设置鼠标图片
    /// </summary>
    /// <param name="sprite"></param>
    private void SetCursorImage(Sprite sprite)
    {
        cursorImage.sprite = sprite;
        cursorImage.color = new Color(1, 1, 1, 1);
    }

    /// <summary>
    /// 鼠标在可用的情况下,颜色不透明
    /// </summary>
    private void SetCursorvalid(){
        cursorPositionValid = true;
        cursorImage.color = new Color(1, 1, 1, 1);//鼠标不透明
        buildImage.color = new Color(1, 1, 1, 0.5f);//建造鼠标椅子半透明
    }

    /// <summary>
    /// 鼠标在不可用的情况下,颜色半透明
    /// </summary>
    private void SetCursorInValid(){
        cursorPositionValid = false;
        cursorImage.color = new Color(1,0.2f,0.5f,0.5f);
        buildImage.color = new Color(1, 0, 0, 0.5f);//建造鼠标椅子变颜色
    }

    /// <summary>
    /// 根据选择切换图标
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <param name="isSelected"></param>
    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        if (!isSelected)
        {
            currentItem = null;
            cursorEnable = false;
            currentSprite = normal;
            buildImage.gameObject.SetActive(false);
        }
        else //物品被选中才切换图片
        {
            currentItem = itemDetails;

            currentSprite = itemDetails.itemType switch
            {
                ItemType.seed => seed,
                ItemType.Commodity => item,
                ItemType.ChopTool => tool,
                ItemType.HoeTool => tool,
                ItemType.WaterTool => tool,
                ItemType.breakTool => tool,
                ItemType.ReapTool => tool,
                ItemType.Furniture => tool,
                ItemType.CollectTool => tool,
                _ => normal
            };
            cursorEnable = true;

            //如果是鼠标点到背包的建造蓝图,那鼠标移到世界那边的鼠标图标就会变成一个椅子
            if(itemDetails.itemType == ItemType.Furniture){
                buildImage.gameObject.SetActive(true);
                buildImage.sprite = itemDetails.itemOnWorldSprite;
                buildImage.SetNativeSize();
            }else{
                buildImage.gameObject.SetActive(false);
            }
        }

    }

    /// <summary>
    /// 鼠标放在土地上则取得瓦片信息(鼠标移到背包上就不行)
    /// </summary>
    private void CheckCursorVaild(){
        //屏幕坐标转世界坐标(以相机为原点)从(0,0)(1920,1080)转换到(0,0)(1,1)
        mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
        mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);

        //将玩家的世界坐标转换为网格坐标,这一步是为丢物品做准备
        var playerGridPos = currentGrid.WorldToCell(PlayerTransform.position);

        //建造图片跟随移动
        buildImage.rectTransform.position = Input.mousePosition;

        //判断鼠标位置和玩家位置的距离是否大于木头的可丢距离,为丢物品做准备
        if(Mathf.Abs(mouseGridPos.x-playerGridPos.x)>currentItem.itemUseRadius||Mathf.Abs(mouseGridPos.y-playerGridPos.y)>currentItem.itemUseRadius){
            SetCursorvalid();
            return;
        }

        /// <summary>
        /// 鼠标所在位置获取此处瓦片10大信息
        /// </summary>
        /// <returns></returns>
        TileDetails currentTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(mouseGridPos);

        //如果这个下栏是可丢弃商品,鼠标颜色不透明,要是商品不可丢弃的话就半透明
        if(currentTile != null){
            //得到这块地皮的种子信息,为收获做准备
            CropDetails currentCrop = CropManager.Instance.GetCropDetails(currentTile.seedItemID);
            //拿到这块瓦片上的作物
            Crop crop = GridMapManager.Instance.GetCropObject(mouseWorldPos);
            //WORKFLOW:补充所有物品类型的判断
            switch(currentItem.itemType){
                case ItemType.seed:
                    if(currentTile.daysSinceDug>-1 && currentTile.seedItemID ==-1){
                        SetCursorvalid();
                    }else{
                        SetCursorInValid();
                    }
                    break;
                case ItemType.Commodity:
                    if(currentTile.canDropItem && currentItem.canDropped){
                        SetCursorvalid();
                    }else{
                        SetCursorInValid();
                    }
                    break;
                case ItemType.HoeTool:
                    if(currentTile.canDig){
                        SetCursorvalid();
                    }else{
                        SetCursorInValid();
                    }
                    break;
                case ItemType.WaterTool:
                    if(currentTile.daysSinceDug>-1 && currentTile.daysSinceWatered==-1){
                        SetCursorvalid();
                    }else{
                        SetCursorInValid();
                    }
                    break;
                case ItemType.breakTool:
                    //break;
                case ItemType.ChopTool:
                    if(crop != null){
                        if(crop.CanHarvest && crop.cropDetails.CheckToolAvailable(currentItem.itemID)) SetCursorvalid(); else SetCursorInValid();
                    }else{
                        SetCursorInValid();
                    }
                    break;
                case ItemType.CollectTool:
                    if(currentCrop != null){
                        if(currentCrop.CheckToolAvailable(currentItem.itemID)){
                            if(currentTile.growthDays >= currentCrop.TotalGrowthDays){
                                SetCursorvalid();
                            }else{
                                SetCursorInValid();
                            }
                        }
                    }else{
                        SetCursorInValid();
                    }
                    break;
                case ItemType.ReapTool:
                    if(GridMapManager.Instance.HaveReapableItemInRadius(mouseWorldPos,currentItem)){
                        SetCursorvalid();
                    }else{
                        SetCursorInValid();
                    }
                    break;
                case ItemType.Furniture:
                    buildImage.gameObject.SetActive(true);
                    var bluePrintDatails = InventoryManager.Instance.bluePrintData.GetBluePrintDatails(currentItem.itemID);
                    //是建筑可用地且背包有足够建材就显示
                    if(currentTile.canPlaceFurniture && InventoryManager.Instance.CheckStock(currentItem.itemID) && !HaveFurnitureInRadius(bluePrintDatails)){
                        SetCursorvalid();
                    }else{
                        SetCursorInValid();
                    }
                    break;
                    
            }
        }else{
            SetCursorInValid();
        }
    }

    /// <summary>
    /// 检查周围是否有建筑物,周围有建造物那就不能建造在这了
    /// </summary>
    /// <param name="bluePrintDatails"></param>
    /// <returns></returns>
    private bool HaveFurnitureInRadius(BluePrintDatails bluePrintDatails){
        var buildItem = bluePrintDatails.buildPrefab;
        Vector2 point = mouseWorldPos;
        var size = buildItem.GetComponent<BoxCollider2D>().size;

        var otherColl = Physics2D.OverlapBox(point, size, 0);
        if(otherColl != null){
            return otherColl.GetComponent<Furniture>();
        }
        return false;
    }

    /// <summary>
    /// 其他类型鼠标是否与UI(背包)有互动,有则变回去
    /// </summary>
    /// <returns></returns>
    public bool InteractWithUI()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }
        return false;
    }

}
