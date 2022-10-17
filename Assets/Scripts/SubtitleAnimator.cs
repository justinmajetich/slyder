using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SubtitleAnimator : MonoBehaviour
{
    public static event Action OnAnimationComplete;
    public static event Action<float, char> OnTalk;

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
        [Range(0f, 5f), Tooltip("This modifier increases the base duration of character animation by a given percentage.")]
        public float sentenceDelayModifier = 4f;
        [Range(0f, 5f), Tooltip("This modifier increases the base duration of character animation by a given percentage.")]
        public float commaDelayModifier = 2.5f;
        [Range(0f, 1f), Tooltip("This modifier decreases the base duration of character animation by a given percentage.")]
        public float spaceDelayModifier = 0.5f;
        //[Range(0f, 1f), Tooltip("Animate this character at given percent of default animation speed")]
        //public float finalDelay = 1.2f;

        [Header("Expressive Pauses")]
        public float shortPause = 0.5f;
        public float mediumPause = 1f;
        public float longPause = 2f;
        public float veryLongPause = 4f;
    }

    [Header("UI Text Element")]
    public TMP_Text subtitleText;

    [Header("Pacing")]
    [SerializeField] SubtitleSpeeds speeds = new SubtitleSpeeds();
    [SerializeField] SubtitleDelays delays = new SubtitleDelays();
    float animationSpeed;
    bool pauseTriggered = false;

    [Space]
    public bool allowAnimationSkip = true;


    void OnEnable()
    {
        ExpressionTagParser.OnSpeedExpressed += OnSpeedExpressed;
        ExpressionTagParser.OnPauseExpressed += OnPauseExpressed;
    }

    void OnDisable()
    {
        ExpressionTagParser.OnSpeedExpressed -= OnSpeedExpressed;
        ExpressionTagParser.OnPauseExpressed -= OnPauseExpressed;
    }

    /// <summary>
    /// Animates a string into a text mesh object character by character. Animation pacing is dynamic based on settings and embedded expression tags.
    /// </summary>
    /// <param name="subtitle">String to animate.</param>
    public void Animate(string subtitle)
    {
        // Pass subtitle text and completion callback to animation coroutine.
        StartCoroutine(AnimateSubtitle(subtitle));
    }
    
    IEnumerator AnimateSubtitle(string subtitle)
    {
        bool animationWasSkipped = false;

        animationSpeed = speeds.normal;

        // If animation skipping is enabled, monitor for skip input.
        if (allowAnimationSkip)
        {
            StartCoroutine(MonitorSkipAnimationInput(() => { animationWasSkipped = true; }));
        }

        for (int i = 0; i < subtitle.Length; i++)
        {
            // If user has input skip animation action, display full subtitle and break animation loop.
            if (animationWasSkipped)
            {
                subtitleText.text = subtitle;
                yield return null;
                break;
            }

            // Check for expression tag and parse. Loop in case of subsequent tags.
            while (subtitle[i] == Constants.ExpressionTagOpen)
            {
                // Parser returns a copy of the subtitle with current tag removed.
                subtitle = ExpressionTagParser.Parse(subtitle, i);

                // If parsed string is now shorter than current position.
                if (i >= subtitle.Length)
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
                // Check if character begins new word.
                if (i != 0 && subtitle[i - 1] == ' ')
                {
                    // Runs ahead to check if current word will fit on current line.
                    if (!WordWillFitLine(subtitle, i))
                    {
                        subtitleText.text += '\n';
                    }
                }

                subtitleText.text += subtitle[i];

                OnTalk?.Invoke(animationSpeed, subtitle[i]);

                yield return new WaitForSeconds(GetCharacterDelay(subtitle[i]));
            }
        }

    AnimationComplete:
        OnAnimationComplete?.Invoke();
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


    void OnSpeedExpressed(int value)
    {
        // Convert expression value to pre-defined subtitle speed.
        animationSpeed = value switch
        {
            -2 => speeds.verySlow,
            -1 => speeds.slow,
            1 => speeds.fast,
            2 => speeds.veryFast,
            _ => speeds.normal
        };
    }

    void OnPauseExpressed(int value)
    {
        // Convert expression value to pre-defined pause duration.
        float duration = value switch
        {
            1 => delays.shortPause,
            2 => delays.mediumPause,
            3 => delays.longPause,
            4 => delays.veryLongPause,
            _ => 0f
        };

        StartCoroutine(PauseSubtitleAnimation(duration));
    }

    float GetCharacterDelay(char c)
    {
        // Get character-specific delay by slowing the current subtitle speed by a given percentage of itself.
        if (c == ',')
        {
            return animationSpeed + (animationSpeed * delays.commaDelayModifier);
        }
        else if (c == '.' || c == '?' || c == '!')
        {
            return animationSpeed + (animationSpeed * delays.sentenceDelayModifier);
        }
        else if (c == ' ')
        {
            return animationSpeed - (animationSpeed * delays.spaceDelayModifier);
        }
        else
        {
            return animationSpeed;
        }
    }

    /// <summary>
    /// Determines if the next word with fit on the current line of the text mesh object.
    /// </summary>
    /// <param name="subtitle">The complete string being animated into the text mesh object.</param>
    /// <param name="i">Current position of animator in subtitle string.</param>
    /// <returns>True if next word will fit; False if not.</returns>
    bool WordWillFitLine(string subtitle, int i)
    {
        // Store current contents of text mesh object.
        string temp = subtitleText.text;

        // Get line number of new word based on first character, forcing mesh update to retrieve accurate character data.
        subtitleText.text += subtitle[i];
        subtitleText.ForceMeshUpdate();

        int initialLineNumber = subtitleText.textInfo.characterInfo[i].lineNumber;

        i++; // Advance to next position in string.

        // Add remainder of current word (including closing punctuation and/or space) to text mesh object.
        while (i < subtitle.Length && subtitle[i] != '<')
        {
            char c = subtitle[i++];

            subtitleText.text += c;

            if (c == ' ') // If character is space, word has ended.
            {
                break;
            }
        }

        subtitleText.ForceMeshUpdate(); // Update mesh data again to reflect additional characters.

        // Compare line number of word's first character to that of its last. If different, word does NOT fit line.
        bool wordFitsLine = initialLineNumber == subtitleText.textInfo.characterInfo[i - 1].lineNumber;

        // Reset text mesh content to pre-function state.
        subtitleText.text = temp;

        return wordFitsLine;
    }
}
