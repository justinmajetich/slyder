using UnityEngine;
using NodeCanvas.DialogueTrees;


public class ExpressiveDialogueActor : DialogueActor
{
    [Header("Dialogue UI")]
    public Transform leftUIAnchor;
    public Transform rightUIAnchor;
    public DialogueOrientation dialogueOrientation = DialogueOrientation.None;
}

public enum DialogueOrientation
{
    Left = -1,
    Right = 1,
    None = 0
}