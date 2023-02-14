using System;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer
{
    int sceneIndex = -1;//默认为-1,代表没有任何场景
    GUIContent[] sceneNames;
    readonly string[] scenePathSplit = { "/", ".unity" };
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (EditorBuildSettings.scenes.Length == 0) return;//没有场景就跳过
        if (sceneIndex == -1)
        {
            GetSceneNameArray(property);
        }
        int oldIndex = sceneIndex;
        sceneIndex = EditorGUI.Popup(position, label, sceneIndex, sceneNames);//打印选项
        if (oldIndex != sceneIndex)
        {
            property.stringValue = sceneNames[sceneIndex].text;
        }
    }
    /// <summary>
    /// ???????????
    /// </summary>
    /// <param name="property"></param>
    public void GetSceneNameArray(SerializedProperty property)
    {
        var scenes = EditorBuildSettings.scenes;
        //初始化数组
        sceneNames = new GUIContent[scenes.Length];

        //获得场景名字
        for (int i = 0; i < sceneNames.Length; i++)
        {
            string path = scenes[i].path;
            //去除路径的/和空格等
            string[] splitPath = path.Split(scenePathSplit, System.StringSplitOptions.RemoveEmptyEntries);
            string sceneName = "";
            if (splitPath.Length > 0)
            {
                sceneName = splitPath[splitPath.Length - 1];//成功获取名字
            }
            else
            {
                sceneName = "(Deleted Scene)";
            }
            sceneNames[i] = new GUIContent(sceneName);
        }
        //如果没有场景
        if (sceneNames.Length == 0)
        {
            sceneNames = new[] { new GUIContent("Check your Build Setting") };
        }
        //如果附着的变量有值
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            bool nameFound = false;

            for (int i = 0; i < sceneNames.Length; i++)
            {
                //附着的变量的场景名字和场景库中的某个场景名字对应上了,选项自动选择到它
                if (sceneNames[i].text == property.stringValue)
                {
                    sceneIndex = i;
                    nameFound = true;
                    break;
                }
            }
            //如果附着的变量有场景名字,但在场景库中找不到该名字,选项自动滑到第一项
            if (nameFound == false)
            {
                sceneIndex = 0;
            }

        }
        else
        {//如果附着的变量没有值,选项自动滑到第一项
            sceneIndex = 0;
        }

        property.stringValue = sceneNames[sceneIndex].text;
    }
}
#endif
