using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using NodeCanvas.DialogueTrees;


public class DialougeUIController : MonoBehaviour
{
    [Serializable]
    public class SubtitleSpeeds
    {
        public float verySlow = 0.15f;
        public float slow = 0.1f;
        public float normal = 0.05f;
        public float fast = 0.03f;
        public float veryFast = 0.01f;
    }

    [Serializable]
    public class SubtitleDelays
    {
        [Header("Punctuation Delay Modifiers")]
        //public const float characterDelay = 1f;
        [Range(0f, 5f), Tooltip("This modifier increases the base duration of character animation by a given percentage.")]
        public float sentenceDelayModifier = 4f;
        [Range(0f, 5f), Tooltip("This modifier increases the base duration of character animation by a given percentage.")]
        public float commaDelayModifier = 2.5f;
        //[Range(0f, 1f), Tooltip("Animate this character at given percent of default animation speed")]
        //public float finalDelay = 1.2f;

        [Header("Expressive Pauses")]
        public float shortPause = 0.5f;
        public float mediumPause = 1f;
        public float longPause = 2f;
        public float veryLongPause = 4f;
    }

    public TMP_Text actorName;

    [Header("Subtitle Elements")]
    public GameObject subtitleView;
    public TMP_Text subtitleText;
    public bool allowAnimationSkip = true;
    public KeyCode skipAnimationKey = KeyCode.Space;

    [Header("Subtitle Pacing")]
    [SerializeField] SubtitleSpeeds subtitleSpeeds = new SubtitleSpeeds();
    [SerializeField] SubtitleDelays subtitleDelays = new SubtitleDelays();
    float currentSubtitleSpeed;
    bool pauseTriggered = false;

    [Header("Option Elements")]
    public GameObject optionsView;
    public Transform optionsContainer;
    public GameObject optionButtonPrefab;
    Dictionary<int, Button> cachedButtons = new Dictionary<int, Button>();


    private void Awake()
    {
        subtitleView.SetActive(false);
        optionsView.SetActive(false);
    }

    void OnEnable()
    {
        DialogueTree.OnSubtitlesRequest += OnSubtitlesRequest;
        DialogueTree.OnMultipleChoiceRequest += OnMultipleChoiceRequest;
        ExpressionTagParser.OnSpeedExpressed += OnSpeedExpressed;
        ExpressionTagParser.OnPauseExpressed += OnPauseExpressed;
    }

    void OnDisable()
    {
        DialogueTree.OnSubtitlesRequest -= OnSubtitlesRequest;
        DialogueTree.OnMultipleChoiceRequest -= OnMultipleChoiceRequest;
        ExpressionTagParser.OnSpeedExpressed -= OnSpeedExpressed;
        ExpressionTagParser.OnPauseExpressed -= OnPauseExpressed;
    }

    private void OnSubtitlesRequest(SubtitlesRequestInfo info)
    {
        actorName.text = info.actor.name;

        subtitleText.text = "";
        subtitleView.SetActive(true);

        // Start co-routine to write out subtitles with effects.
        StartCoroutine(AnimateSubtitles(info));
    }

    IEnumerator AnimateSubtitles(SubtitlesRequestInfo info)
    {
        currentSubtitleSpeed = subtitleSpeeds.normal;

        bool animationWasSkipped = false;
        string dialogueText = info.statement.text;


        // If animation skipping is enabled, monitor for skip input.
        if (allowAnimationSkip)
        {
            StartCoroutine(MonitorSkipAnimationInput(() => { animationWasSkipped = true; }));
        }

        for (int i = 0; i < dialogueText.Length; i++)
        {
            // If user has input skip animation action, display full subtitle and break animation loop.
            if (animationWasSkipped)
            {
                subtitleText.text = dialogueText;
                yield return null;
                break;
            }

            // If character opens expression tag (ex. "<speed=1>", pass to parser and wait for new position. Loop in case of subsequent tags.
            while (dialogueText[i] == Constants.ExpressionTagOpen)
            {
                // Parser returns position in text following tag close.
                i = ExpressionTagParser.Parse(dialogueText, i);

                // If parser returns position beyond range of subtitle text, jump out of animation loops.
                if (i >= dialogueText.Length)
                {
                    goto AnimationComplete;
                }
            }

            // Hang animation while pause is active.
            while (pauseTriggered && !animationWasSkipped)
            {
                yield return null;
            }

            // Add next character to displayed text trigger appropriate delay.
            if (!animationWasSkipped)
            {
                subtitleText.text += dialogueText[i];
                yield return new WaitForSeconds(GetCharacterDelay(dialogueText[i]));
            }
        }

    AnimationComplete:

        // Wait for Space press to continue.
        while (!Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            yield return null;
        }

        subtitleView.SetActive(false);

        // Execute subtitle request callback to continue dialogue tree.
        info.Continue();
    }

    IEnumerator MonitorSkipAnimationInput(Action Do)
    {
        // This yield prevents skipping action on same frame as a previous statements continue action when bindings are the same.
        yield return null;

        while (!Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            yield return null;
        }
        Do();
    }

    IEnumerator PauseSubtitleAnimation(float duration)
    {
        pauseTriggered = true;

        yield return new WaitForSeconds(duration);

        pauseTriggered = false;
    }

    private void OnMultipleChoiceRequest(MultipleChoiceRequestInfo info)
    {
        float buttonHeight = optionButtonPrefab.GetComponent<RectTransform>().rect.height;
        actorName.text = info.actor.name;

        foreach (KeyValuePair<IStatement, int> option in info.options)
        {
            int optionIndex = option.Value;

            // If a button DOES NOT already exists for this index, set up new option button.
            if (!cachedButtons.TryGetValue(optionIndex, out Button optionButton))
            {
                optionButton = Instantiate(optionButtonPrefab).GetComponent<Button>();
                optionButton.transform.SetParent(optionsContainer, false);

                // Cache new button.
                cachedButtons.Add(optionIndex, optionButton);
            }

            // Assign request info to button text.
            optionButton.GetComponentInChildren<TMP_Text>().text = option.Key.text; ;

            // Add new listener to button's onClick event.
            optionButton.onClick.AddListener( () => { OnOptionSelected(info.SelectOption, optionIndex); } );

            // Set option elements active.
            optionButton.gameObject.SetActive(true);
            optionsView.SetActive(true);
        }
    }

    // Method passed to be passed in as callback when an option is selected.
    void OnOptionSelected(Action<int> selectOption, int optionIndex)
    {
        // Continue dialogue tree according to selected option.
        selectOption(optionIndex);

        // Remove listeners from cached buttons and set inactive.
        foreach (KeyValuePair<int, Button> button in cachedButtons)
        {
            button.Value.onClick.RemoveAllListeners();
            button.Value.gameObject.SetActive(false);
        }

        optionsView.SetActive(false);
    }

    private void OnSpeedExpressed(int value)
    {
        // Convert expression value to pre-defined subtitle speed.
        currentSubtitleSpeed = value switch
        {
            -2 => subtitleSpeeds.verySlow,
            -1 => subtitleSpeeds.slow,
            1 => subtitleSpeeds.fast,
            2 => subtitleSpeeds.veryFast,
            _ => subtitleSpeeds.normal
        };
    }

    private void OnPauseExpressed(int value)
    {
        // Convert expression value to pre-defined pause duration.
        float duration = value switch
        {
            1 => subtitleDelays.shortPause,
            2 => subtitleDelays.mediumPause,
            3 => subtitleDelays.longPause,
            4 => subtitleDelays.veryLongPause,
            _ => 0f
        };

        StartCoroutine(PauseSubtitleAnimation(duration));
    }

    float GetCharacterDelay(char c)
    {
        // Get character-specific delay by slowing the current subtitle speed by a given percentage of itself.
        if (c == ',')
        {
            return currentSubtitleSpeed + (currentSubtitleSpeed * subtitleDelays.commaDelayModifier);
        }
        else if (c == '.' || c == '?' || c == '!')
        {
            return currentSubtitleSpeed + (currentSubtitleSpeed * subtitleDelays.sentenceDelayModifier);
        }
        else
        {
            return currentSubtitleSpeed;
        }
    }
}
