using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 下面这些代码都是为了将一堆特效存起来在一堆对象池,随时可用
/// </summary>
public class PoolManager : MonoBehaviour
{
    public List<GameObject> poolPrefabs;
    /// <summary>
    /// 对象池列表(一堆对象池)
    /// </summary>
    /// <returns></returns>
    private List<ObjectPool<GameObject>> poolEffectList = new List<ObjectPool<GameObject>>();//对象池列表(一堆对象池)

    private Queue<GameObject> soundQueue = new Queue<GameObject>();//队列(先进先出) 对象池

    private void OnEnable() {
        EventHandler.ParticleEffectEvent += OnParticleEffectEvent;
        EventHandler.InitSoundEffect += InitSoundEffect;
    }

    private void OnDisable() {
        EventHandler.ParticleEffectEvent -= OnParticleEffectEvent;
        EventHandler.InitSoundEffect -= InitSoundEffect;
    }

    private void Start() {
        CreatePool();
    }

    /// <summary>
    /// 生成对象池
    /// </summary>
    private void CreatePool(){
        foreach(GameObject item in poolPrefabs){
            Transform parent = new GameObject(item.name).transform;
            parent.SetParent(transform);//将那两个特效的放在PoolManager下

            //新建对象池
            var newPool = new ObjectPool<GameObject>(
                () => Instantiate(item, parent),
                e => { e.SetActive(true); },
                e => { e.SetActive(false); },
                e => { Destroy(e); }
            );

            //将新建的对象池加入对象池列表中
            poolEffectList.Add(newPool);
        }
    }

    /// <summary>
    /// 播放特效
    /// </summary>
    /// <param name="effectType"></param>
    /// <param name="pos"></param>
    private void OnParticleEffectEvent(ParticaleEffectType effectType, Vector3 pos)
    {
        //根据特效补全
        ObjectPool<GameObject> objPool = effectType switch
        {
            ParticaleEffectType.LeavesFalling01 => poolEffectList[0],
            ParticaleEffectType.LeavesFalling02 => poolEffectList[1],
            ParticaleEffectType.Rock=>poolEffectList[2],
            ParticaleEffectType.ReapableScenery=>poolEffectList[3],
            _ => null,
        };
        //取出特效的GameObject
        GameObject obj = objPool.Get();
        obj.transform.position = pos;
        StartCoroutine(ReleaseRoutine(objPool, obj));//等1.5s,这样动画就播放完了,再收回obj
    }

    private IEnumerator ReleaseRoutine(ObjectPool<GameObject> pool,GameObject obj){
        yield return new WaitForSeconds(1.5f);
        pool.Release(obj);
    }

    // /// <summary>
    // /// 生成音效
    // /// </summary>
    // /// <param name="soundDetails"></param>
    // private void InitSoundEffect(SoundDetails soundDetails){
    //     ObjectPool<GameObject> pool = poolEffectList[4];//得到第五个Sound(gamobject)的对象池
    //     var obj = pool.Get();//得到Sound(gamobject)

    //     obj.GetComponent<Sound>().SetSound(soundDetails);//得到代码Sound里的audio Source设置为新音效片段(soundDetails)
    //     StartCoroutine(DisableSound(pool, obj, soundDetails));//音效播放完后要搞回去
    // }

    // /// <summary>
    // /// 音效播放完后要搞回去
    // /// </summary>
    // /// <param name="pool"></param>
    // /// <param name="obj"></param>
    // /// <param name="soundDetails"></param>
    // /// <returns></returns>
    // private IEnumerator DisableSound(ObjectPool<GameObject> pool,GameObject obj,SoundDetails soundDetails){
    //     yield return new WaitForSeconds(soundDetails.soundClip.length);//音效播放完成后
    //     pool.Release(obj);//从对象池释放Sound(gamobject)
    // }

    /// <summary>
    /// 创建了对象池 初始化对象池
    /// </summary>
    private void CreateSoundPool(){
        var parent = new GameObject(poolPrefabs[4].name).transform;//得到Sound(gameobject)的位置
        parent.SetParent(transform);//将特效Sound的放在PoolManager下

        for (int i = 0; i < 20;i++){
            GameObject newObj = Instantiate(poolPrefabs[4], parent);
            newObj.SetActive(false);
            soundQueue.Enqueue(newObj);//压到队列当中
        }
    }

    /// <summary>
    /// 取得音效
    /// </summary>
    /// <returns></returns>
    private GameObject GetPoolObject(){
        if(soundQueue.Count < 2){//发现队列里子弹不多了就再卡20个进去
            CreateSoundPool();
        }
        return soundQueue.Dequeue();//把队列的第一个拿出来
    }

    /// <summary>
    /// 生成音效
    /// </summary>
    /// <param name="soundDetails"></param>
    private void InitSoundEffect(SoundDetails soundDetails){
        var obj = GetPoolObject();//得到第一个Sound(gameobject)
        obj.GetComponent<Sound>().SetSound(soundDetails);//将Sound(gameobject)身上的AudioSource的声源设置为soundDetails;
        obj.SetActive(true);
        StartCoroutine(DisableSound(obj, soundDetails.soundClip.length));// 等音效播放完就关闭和压回去对象池里
    }

    /// <summary>
    /// 等音效播放完就关闭和压回去对象池里
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator DisableSound(GameObject obj,float duration){
        yield return new WaitForSeconds(duration);//等音效播放完
        obj.SetActive(false);
        soundQueue.Enqueue(obj);
    }
}
