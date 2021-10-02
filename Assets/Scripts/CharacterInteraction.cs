using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.DialogueTrees;


public class CharacterInteraction : MonoBehaviour
{
    DialogueActor self;

    private void Start()
    {
        self = GetComponent<DialogueActor>();
    }

    void Update()
    {
        int layerMask = 1 << LayerMask.NameToLayer("NPC");

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Player: Got E Key Down");

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.25f, layerMask);

            Debug.Log("Player: I have " + hits.Length + " hits!");

            foreach (Collider2D hit in hits)
            {
                Debug.Log("Player: Hit name is " + hit.gameObject.name);

                if (hit.CompareTag("NPC"))
                {
                    Debug.Log("Player: I'm interacting with an NPC");

                    DialogueTreeController dialogue = hit.gameObject.GetComponent<DialogueTreeController>();

                    if (dialogue)
                    {
                        dialogue.StartDialogue(self);
                    }
                }
            }
        }
    }
}
