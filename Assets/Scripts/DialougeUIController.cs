using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NodeCanvas.DialogueTrees;
using System;


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

    [Header("General UI Elements")]
    public GameObject parentUIElement;
    public Image actorPortrait;
    public Sprite defaultPortraitSprite;
    public TMP_Text actorName;

    [Header("Subtitle Elements")]
    public GameObject subtitleView;
    public TMP_Text subtitleText;
    public bool allowAnimationSkip = true;
    public KeyCode skipAnimationKey = KeyCode.Space;

    [Header("Subtitle Settings")]
    [SerializeField] SubtitleDelays delays = new SubtitleDelays();

    [Header("Option Elements")]
    public GameObject optionsView;
    public Transform optionsContainer;
    public GameObject optionButtonPrefab;
    public float buttonSpacing = 20f;
    Dictionary<int, Button> cachedButtons = new Dictionary<int, Button>();


    void OnEnable()
    {
        DialogueTree.OnDialogueStarted += OnDialogueStarted;
        DialogueTree.OnDialoguePaused += OnDialoguePaused;
        DialogueTree.OnDialogueFinished += OnDialogueFinished;
        DialogueTree.OnSubtitlesRequest += OnSubtitlesRequest;
        DialogueTree.OnMultipleChoiceRequest += OnMultipleChoiceRequest;
    }

    void OnDisable()
    {
        DialogueTree.OnDialogueStarted -= OnDialogueStarted;
        DialogueTree.OnDialoguePaused -= OnDialoguePaused;
        DialogueTree.OnDialogueFinished -= OnDialogueFinished;
        DialogueTree.OnSubtitlesRequest -= OnSubtitlesRequest;
        DialogueTree.OnMultipleChoiceRequest -= OnMultipleChoiceRequest;
    }

    void Start()
    {
        parentUIElement.SetActive(false);
        optionsView.SetActive(false);
    }

    private void OnDialogueStarted(DialogueTree obj)
    {
        parentUIElement.SetActive(true);
    }

    private void OnDialoguePaused(DialogueTree obj)
    {
        throw new NotImplementedException();
    }

    private void OnSubtitlesRequest(SubtitlesRequestInfo obj)
    {
        subtitleView.SetActive(true);

        // Start co-routine to write out subtitles with effects.
        StartCoroutine(AnimateSubtitles(obj));
    }

    IEnumerator AnimateSubtitles(SubtitlesRequestInfo info)
    {
        bool skipAnimation = false;
        string dialogueText = info.statement.text;
        string textBuffer = "";

        // Assign actor variables.
        actorName.text = info.actor.name;
        actorPortrait.sprite = info.actor.portraitSprite ? info.actor.portraitSprite : defaultPortraitSprite;

        if (allowAnimationSkip)
        {
            StartCoroutine(MonitorSkipAnimationInput(() => { skipAnimation = true; }));
        }

        for (int i = 0; i < dialogueText.Length; i++)
        {
            if (skipAnimation)
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
        while (!Input.GetKeyDown(KeyCode.Space))
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

        while (!Input.GetKeyDown(skipAnimationKey))
        {
            yield return null;
        }
        Do();
    }

    private void OnMultipleChoiceRequest(MultipleChoiceRequestInfo info)
    {
        optionsView.SetActive(true);

        float buttonHeight = optionButtonPrefab.GetComponent<RectTransform>().rect.height;
        int buttonIndex = 0;

        foreach (KeyValuePair<IStatement, int> option in info.options)
        {
            Debug.Log(option.Value.ToString() + ": " + option.Key.text);

            // If a button DOES NOT already exists for this index, set up new option button.
            if (!cachedButtons.TryGetValue(buttonIndex, out Button optionButton))
            {
                optionButton = Instantiate(optionButtonPrefab).GetComponent<Button>();
                optionButton.transform.SetParent(optionsContainer, false);
                optionButton.transform.localPosition = new Vector2(0f, (buttonSpacing + buttonHeight) * buttonIndex);

                // Add new button to cache.
                cachedButtons.Add(buttonIndex, optionButton);
            }

            // Assign request info to button text.
            TMP_Text buttonText = optionButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = option.Key.text;

            // Clear existing listeners from button's onClick event and add new.
            optionButton.onClick.RemoveAllListeners();
            optionButton.onClick.AddListener( () => { OnOptionSelected(info.SelectOption, option.Value); } );
            Debug.Log("buttonIndex: " + buttonIndex);

            optionButton.gameObject.SetActive(true);

            buttonIndex++;
        }
    }

    void OnOptionSelected(Action<int> selectOption, int optionIndex)
    {
        selectOption(optionIndex);

        // Set all cached buttons inactive.
        foreach (var button in cachedButtons)
        {
            button.Value.gameObject.SetActive(false);
        }
    }

    private void OnDialogueFinished(DialogueTree obj)
    {
        parentUIElement.SetActive(false);
    }

    void Update()
    {

    }
}
