using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager> {
    [Header("音乐数据库")]
    public SoundDetailsList_SO soundDetailsData;
    public SceneSoundList_SO sceneSoundData;
    [Header("Audio Source")]
    public AudioSource ambientSource;
    public AudioSource gameSource;
    private Coroutine soundRoutine;
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    [Header("Snapshots")]
    public AudioMixerSnapshot normalSnapShot;
    public AudioMixerSnapshot ambientSnapShot;
    public AudioMixerSnapshot muteSnapShot;
    private float musicTransitionSecond = 6f;
    //5秒到15秒换音乐
    public float MusicStartSecond => UnityEngine.Random.Range(5f, 15f);

    private void OnEnable() {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.PlaySoundEvent += OnPlaySoundEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable() {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.PlaySoundEvent -= OnPlaySoundEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void OnEndGameEvent()
    {
        if(soundRoutine != null){
            StopCoroutine(soundRoutine);
        }
        muteSnapShot.TransitionTo(1f);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="soundName"></param>
    private void OnPlaySoundEvent(SoundName soundName)
    {
        var soundDetails = soundDetailsData.GetSoundDetails(soundName);//通过名字得到soundDetails.
        if(soundDetails != null){
            EventHandler.CallInitSoundEffect(soundDetails);//生成音效,soundDetails传到audioSource并播放
        }
    }

    private void OnAfterSceneLoadedEvent()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        SceneSoundItem sceneSound = sceneSoundData.GetSceneSoundItem(currentScene);
        if(sceneSound == null){
            return;
        }

        SoundDetails ambient = soundDetailsData.GetSoundDetails(sceneSound.ambient);
        SoundDetails music = soundDetailsData.GetSoundDetails(sceneSound.music);

        if(soundRoutine != null){
            StopCoroutine(soundRoutine);  
        }

        soundRoutine = StartCoroutine(PlaySoundRoutine(music, ambient));
    }

    private IEnumerator PlaySoundRoutine(SoundDetails music,SoundDetails ambient){
        if(music != null && ambient != null){
            PlayAmbientClip(ambient, 1f);//先播放环境音效 1秒快速切换
            yield return new WaitForSeconds(MusicStartSecond);//等5-15秒
            PlayMusicClip(music, musicTransitionSecond);//后播放背景音乐
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="soundDetails"></param>
    private void PlayMusicClip(SoundDetails soundDetails,float transitionTime){
        audioMixer.SetFloat("MusicVolume", ConvectSoundVolume(soundDetails.soundVolume));
        gameSource.clip = soundDetails.soundClip;
        if(gameSource.isActiveAndEnabled){
            gameSource.Play();
        }
        //过了8秒就将音量等设置切换到normalSnapShot状态
        normalSnapShot.TransitionTo(transitionTime);
    }

    /// <summary>
    /// 播放环境音效
    /// </summary>
    /// <param name="soundDetails"></param>
    private void PlayAmbientClip(SoundDetails soundDetails,float TransitionTime){
        audioMixer.SetFloat("AmbientVolume", ConvectSoundVolume(soundDetails.soundVolume));
        ambientSource.clip = soundDetails.soundClip;
        if(ambientSource.isActiveAndEnabled){
            ambientSource.Play();
        }
        //过了transitionTime秒就将音量等设置切换到normalSnapShot状态
        ambientSnapShot.TransitionTo(TransitionTime);
    }

    private float ConvectSoundVolume(float amount){
        //-80~20
        return (amount * 100 - 80);
    }

    public void SetMasterVolume(float value){
        audioMixer.SetFloat("MasterVolume", (value * 100 - 80));
    }
}