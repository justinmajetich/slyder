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
    [System.Serializable]
    public class SubtitleDelays
    {
        public float characterDelay = 0.05f;
        public float sentenceDelay = 0.5f;
        public float commaDelay = 0.1f;
        public float finalDelay = 1.2f;
    }

    public TMP_Text actorName;

    [Header("Subtitle Elements")]
    public GameObject subtitleView;
    public TMP_Text subtitleText;
    public bool allowAnimationSkip = true;
    public KeyCode skipAnimationKey = KeyCode.Space;

    [Header("Subtitle Delays")]
    [SerializeField] SubtitleDelays delays = new SubtitleDelays();

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
        //DialogueTree.OnDialogueStarted += OnDialogueStarted;
        //DialogueTree.OnDialoguePaused += OnDialoguePaused;
        //DialogueTree.OnDialogueFinished += OnDialogueFinished;
        DialogueTree.OnSubtitlesRequest += OnSubtitlesRequest;
        DialogueTree.OnMultipleChoiceRequest += OnMultipleChoiceRequest;
    }

    void OnDisable()
    {
        //DialogueTree.OnDialogueStarted -= OnDialogueStarted;
        //DialogueTree.OnDialoguePaused -= OnDialoguePaused;
        //DialogueTree.OnDialogueFinished -= OnDialogueFinished;
        DialogueTree.OnSubtitlesRequest -= OnSubtitlesRequest;
        DialogueTree.OnMultipleChoiceRequest -= OnMultipleChoiceRequest;
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
        bool animationWasSkipped = false;
        string dialogueText = info.statement.text;
        string textBuffer = "";

        // If animation skipping is enabled, monitor for skip input.
        if (allowAnimationSkip)
        {
            StartCoroutine(MonitorSkipAnimationInput(() => { animationWasSkipped = true; }));
        }

        for (int i = 0; i < dialogueText.Length; i++)
        {
            if (animationWasSkipped)
            {
                subtitleText.text = dialogueText;
                yield return null;
                break;
            }

            // Add next character from subtitle text to buffer.
            char c = dialogueText[i];
            textBuffer += c;

            yield return new WaitForSeconds(delays.characterDelay);

            // Assign updated text buffer contents to text display.
            subtitleText.text = textBuffer;
        }

        // Wait for Space press to continue.
        while (!Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            yield return null;
        }

        subtitleView.SetActive(false);

        // Execute subtitle request callback to continue dialogue tree.
        info.Continue();
    }

    IEnumerator MonitorSkipAnimationInput(System.Action Do)
    {
        // This yield prevents skipping action on same frame as a previous statements continue action when bindings are the same.
        yield return null;

        while (!Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            yield return null;
        }
        Do();
    }

    private void OnMultipleChoiceRequest(MultipleChoiceRequestInfo info)
    {
        float buttonHeight = optionButtonPrefab.GetComponent<RectTransform>().rect.height;
        //int buttonIndex = 0;

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
}
