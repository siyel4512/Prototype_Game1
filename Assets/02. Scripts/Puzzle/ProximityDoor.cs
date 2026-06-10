using System.Collections;
using UnityEngine;

public class ProximityDoor : MonoBehaviour
{
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] [Range(0f, 1f)] private float colliderTriggerPoint = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        StopAllCoroutines();
        StartCoroutine(OpenRoutine());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        StopAllCoroutines();
        StartCoroutine(CloseRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        doorAnimator.SetTrigger("Open");
        yield return null;
        yield return new WaitForSeconds(doorAnimator.GetCurrentAnimatorStateInfo(0).length * colliderTriggerPoint);
        blockingCollider.enabled = false;
    }

    private IEnumerator CloseRoutine()
    {
        doorAnimator.SetTrigger("Close");
        yield return null;
        yield return new WaitForSeconds(doorAnimator.GetCurrentAnimatorStateInfo(0).length * colliderTriggerPoint);
        blockingCollider.enabled = true;
    }
}
