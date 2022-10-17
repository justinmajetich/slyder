using System;
using UnityEngine;
using NodeCanvas.DialogueTrees;

public class CharacterAudioController : MonoBehaviour
{
    [Serializable]
    public class FootstepClips
    {
        public AudioClip[] clips = new AudioClip[10];
    }

    [Serializable]
    public class TalkClips
    {
        public AudioClip[] clips;
    }

    AudioSource source;
    bool isActiveActor = false;

    [Header("Footstep Audio")]
    public FootstepClips footstepAudio = new FootstepClips();

    [Header("Talk Audio")]
    [SerializeField]
    string resourcePath = string.Empty;
    public FootstepClips talkAudio = new FootstepClips();

    private void OnEnable()
    {
        CharacterSpriteController.OnPlayerDidStep += PlayFootstep;
        DialogueTree.OnSubtitlesRequest += IsActiveActor;
        SubtitleAnimator.OnTalk += PlayTalkClip;
    }

    private void OnDisable()
    {
        CharacterSpriteController.OnPlayerDidStep -= PlayFootstep;
        DialogueTree.OnSubtitlesRequest -= IsActiveActor;
        SubtitleAnimator.OnTalk -= PlayTalkClip;
    }

    void Start()
    {
        source = GetComponent<AudioSource>();

        talkAudio.clips = Resources.LoadAll<AudioClip>(resourcePath);
    }

    void IsActiveActor(SubtitlesRequestInfo info)
    {
        isActiveActor = info.actor.name == GetComponentInParent<IDialogueActor>().name;
    }

    void PlayFootstep()
    {
        source.clip = LoadRandomClip(footstepAudio.clips);
        source.Play();
    }

    void PlayTalkClip(float speed, char character)
    {
        if (isActiveActor && !source.isPlaying && char.IsLetterOrDigit(character))
        {
            source.clip = LoadRandomClip(talkAudio.clips);
            source.Play();
        }
    }

    AudioClip LoadRandomClip(AudioClip[] clips)
    {
        int size = clips.Length;

        if (size <= 0)
        {
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, size - 1);

        while (size > 1 && source.clip == clips[randomIndex])
        {
            randomIndex = UnityEngine.Random.Range(0, size - 1);
        }

        return clips[randomIndex];
    }


}
