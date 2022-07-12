using System.Collections;
using UnityEngine;

public class LeafAnimation : MonoBehaviour
{
    Animator leafAnimator;

    void Start()
    {
        leafAnimator = GetComponent<Animator>();
        StartCoroutine(DropLeaf());
    }

    IEnumerator DropLeaf()
    {
        yield return new WaitForSeconds(Random.Range(0f, 5f));
        leafAnimator.SetTrigger("dropLeaf");
    }

    public void OnAnimationComplete()
    {
        StartCoroutine(DropLeaf());
    }
}
