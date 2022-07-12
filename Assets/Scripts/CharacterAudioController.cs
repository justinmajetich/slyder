using System;
using UnityEngine;

public class CharacterAudioController : MonoBehaviour
{
    [Serializable]
    public class FootstepClips
    {
        public AudioClip[] clips = new AudioClip[10];
    }

    AudioSource source;

    [Header("Footstep Audio")]
    public FootstepClips footstepAudio = new FootstepClips();

    private void OnEnable()
    {
        CharacterSpriteController.OnPlayerDidStep += PlayFootstep;
    }

    private void OnDisable()
    {
        CharacterSpriteController.OnPlayerDidStep -= PlayFootstep;
    }

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    void PlayFootstep()
    {
        source.clip = LoadRandomFootstep();
        source.Play();
    }

    AudioClip LoadRandomFootstep()
    {
        int randomIndex = UnityEngine.Random.Range(0, footstepAudio.clips.Length - 1);

        while (source.clip == footstepAudio.clips[randomIndex])
        {
            randomIndex = UnityEngine.Random.Range(0, footstepAudio.clips.Length - 1);
        }

        return footstepAudio.clips[randomIndex];
    }
}
