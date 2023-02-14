using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Transition
{
    public class Teleport : MonoBehaviour
    {
        [SceneName]
        public string sceneToGo;//要传送的场景
        public Vector3 positionToGo;//要传送的地点
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                EventHandler.CallTransitionEvent(sceneToGo, positionToGo);
            }
        }
    }
}
