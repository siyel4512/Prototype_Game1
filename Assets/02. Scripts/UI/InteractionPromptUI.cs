using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image progressBarFill;

    private PlayerInteraction playerInteraction;

    private void Start()
    {
        playerInteraction = FindAnyObjectByType<PlayerInteraction>();
        if (playerInteraction != null)
        {
            playerInteraction.OnTargetChanged += UpdatePrompt;
            playerInteraction.OnInteractProgress += UpdateProgress;
        }
        if (progressBarFill != null)
            progressBarFill.fillAmount = 0f;
        promptRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (playerInteraction != null)
        {
            playerInteraction.OnTargetChanged -= UpdatePrompt;
            playerInteraction.OnInteractProgress -= UpdateProgress;
        }
    }

    private void UpdatePrompt(IInteractable target)
    {
        bool hasTarget = target != null;
        promptRoot.SetActive(hasTarget);
        if (hasTarget)
            promptText.text = target.GetInteractPrompt();
        if (!hasTarget && progressBarFill != null)
            progressBarFill.fillAmount = 0f;
    }

    private void UpdateProgress(float progress)
    {
        if (progressBarFill != null)
            progressBarFill.fillAmount = progress;
    }
}
