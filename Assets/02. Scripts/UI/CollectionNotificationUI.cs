using System.Collections;
using UnityEngine;
using TMPro;

public class CollectionNotificationUI : MonoBehaviour
{
    [SerializeField] private GameObject notificationRoot;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float displayDuration = 2f;

    private Coroutine hideCoroutine;

    private void Start()
    {
        if (ClueManager.Instance != null)
            ClueManager.Instance.OnClueAdded += ShowNotification;

        notificationRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (ClueManager.Instance != null)
            ClueManager.Instance.OnClueAdded -= ShowNotification;
    }

    private void ShowNotification(ClueData clue)
    {
        notificationText.text = $"단서 획득: {clue.clueName}";
        notificationRoot.SetActive(true);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        notificationRoot.SetActive(false);
    }
}
