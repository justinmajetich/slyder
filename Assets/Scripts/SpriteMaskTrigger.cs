using UnityEngine;

public class SpriteMaskTrigger : MonoBehaviour
{
    SpriteMask mask;

    void Start()
    {
        mask = GetComponent<SpriteMask>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "PlayerPivotPoint")
        {
            mask.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "PlayerPivotPoint")
        {
            mask.enabled = false;
        }
    }
}
