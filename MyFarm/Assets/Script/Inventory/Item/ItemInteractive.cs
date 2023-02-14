using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//稻草晃的方法
public class ItemInteractive : MonoBehaviour
{
    private bool isAnimating;
    private WaitForSeconds pause = new WaitForSeconds(0.04f);//????????????
    private void OnTriggerEnter2D(Collider2D other) {
        if(!isAnimating){
            if(other.transform.position.x < transform.position.x){
                //Player在左侧就向右边晃
                StartCoroutine(RotateRight());
            }else{
                //Player在右侧就向左边晃
                StartCoroutine(RotateLeft());
            }
            EventHandler.CallPlaySoundEvent(SoundName.Rustle);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(!isAnimating){
            if(other.transform.position.x > transform.position.x){
                //Player在左侧就向右边晃
                StartCoroutine(RotateRight());
            }else{
                //Player在右侧就向左边晃
                StartCoroutine(RotateLeft());
            }
            EventHandler.CallPlaySoundEvent(SoundName.Rustle);
        }
    }

    private IEnumerator RotateLeft(){//向左晃
        isAnimating = true;

        for (int i = 0; i < 4; i++)
        {
            transform.GetChild(0).Rotate(0, 0, 2);
            yield return pause;
        }
        for (int i = 0; i < 5; i++)
        {
            transform.GetChild(0).Rotate(0, 0, -2);
            yield return pause;
        }
        transform.GetChild(0).Rotate(0, 0, 2);
        yield return pause;

        isAnimating = false;
    }

    private IEnumerator RotateRight(){//向右晃
        isAnimating = true;

        for (int i = 0; i < 4; i++)
        {
            transform.GetChild(0).Rotate(0, 0, -2);
            yield return pause;
        }
        for (int i = 0; i < 5; i++)
        {
            transform.GetChild(0).Rotate(0, 0, 2);
            yield return pause;
        }
        transform.GetChild(0).Rotate(0, 0, -2);
        yield return pause;

        isAnimating = false;
    }
}
