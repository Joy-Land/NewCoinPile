using System.Collections;
using System.Collections.Generic;
using Framework;
using Unity.VisualScripting;
using UnityEngine;

namespace Manager
{
    public class SoundFXManager : SingletonBaseMono<SoundFXManager>
    {
        [SerializeField] private AudioSource soundFXObject;
        
        public void PlaySoundFXClip(AudioClip audioClip, Vector3 spawnPosition, float volume)
        {
            AudioSource audioSource = Instantiate(soundFXObject, spawnPosition, Quaternion.identity);
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.Play();
            float clipLength = audioSource.clip.length;
            Destroy(audioSource.gameObject, clipLength);
        }
    }
}
