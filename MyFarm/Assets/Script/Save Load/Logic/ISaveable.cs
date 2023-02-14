namespace MFarm.Save
{
    public interface ISaveable //接口来的
    {
        string GUID { get; }
        void RegisterSaveable(){
            SaveLoadManager.Instance.RegisterSaveable(this);
        }
        GameSaveData GenerateSaveData();//生成一份存储信息
        void RestoreData(GameSaveData saveData);//将存档里存的东西拿出来
    }
}
