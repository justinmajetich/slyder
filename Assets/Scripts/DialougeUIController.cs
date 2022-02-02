using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using NodeCanvas.DialogueTrees;


public class DialougeUIController : MonoBehaviour
{
    public TMP_Text actorName;

    [Header("Subtitle Elements")]
    public GameObject subtitleView;
    public TMP_Text subtitleText;
    public SubtitleAnimator animator;
    SubtitlesRequestInfo activeSubtitleInfo;

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
        SubtitleAnimator.OnAnimationComplete += OnSubtitleAnimationComplete;
    }

    void OnDisable()
    {
        DialogueTree.OnSubtitlesRequest -= OnSubtitlesRequest;
        DialogueTree.OnMultipleChoiceRequest -= OnMultipleChoiceRequest;
        SubtitleAnimator.OnAnimationComplete -= OnSubtitleAnimationComplete;
    }

    private void OnSubtitlesRequest(SubtitlesRequestInfo info)
    {
        actorName.text = info.actor.name;

        subtitleText.text = "";
        subtitleView.SetActive(true);

        // Pass subtitle text to animator.
        animator.Animate(info.statement.text);

        activeSubtitleInfo = info;
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

    void OnSubtitleAnimationComplete()
    {
        StartCoroutine(WaitForInputToContinue());
    }

    IEnumerator WaitForInputToContinue()
    {
        // Wait for Space press to continue.
        while (!Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            yield return null;
        }

        subtitleView.SetActive(false);

        // Execute subtitle request callback to continue dialogue tree.
        activeSubtitleInfo.Continue();
    }
}
