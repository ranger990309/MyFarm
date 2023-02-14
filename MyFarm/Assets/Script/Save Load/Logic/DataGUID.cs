
using UnityEngine;

[ExecuteAlways]//这个作用是让代码一直运行
public class DataGUID : MonoBehaviour
{
    public string guid;//这是个16位字符串
    private void Awake() {
        if(guid == string.Empty){
            guid = System.Guid.NewGuid().ToString();//一开始先生成一个GUID
        }
    }
}
