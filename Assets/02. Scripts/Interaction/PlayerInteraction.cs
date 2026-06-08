using System;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactRadius = 1.5f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float holdDuration = 1f;
    [SerializeField] private float drainDuration = 0.3f;

    public event Action<IInteractable> OnTargetChanged;
    public event Action<float> OnInteractProgress;

    private InputSystem_Actions inputActions;
    private IInteractable currentTarget;
    private float holdProgress;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        if (ClueUIPanel.IsInventoryOpen)
        {
            if (holdProgress > 0f)
            {
                holdProgress = 0f;
                OnInteractProgress?.Invoke(0f);
            }
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRadius, interactableLayer);
        IInteractable detected = hit != null ? hit.GetComponent<IInteractable>() : null;

        if (detected != currentTarget)
        {
            currentTarget = detected;
            OnTargetChanged?.Invoke(currentTarget);
            holdProgress = 0f;
            OnInteractProgress?.Invoke(0f);
            if (currentTarget != null)
                Debug.Log($"[Interaction] 범위 진입: {hit.gameObject.name}");
            else
                Debug.Log("[Interaction] 범위 이탈");
        }

        if (currentTarget != null)
        {
            if (inputActions.Player.Interact.IsPressed())
            {
                holdProgress = Mathf.MoveTowards(holdProgress, 1f, Time.deltaTime / holdDuration);
                if (holdProgress >= 1f)
                {
                    var mb = currentTarget as MonoBehaviour;
                    Debug.Log($"[Interaction] 수집 완료: {(mb != null ? mb.gameObject.name : "unknown")}");
                    currentTarget.Interact();
                    holdProgress = 0f;
                }
            }
            else
            {
                holdProgress = Mathf.MoveTowards(holdProgress, 0f, Time.deltaTime / drainDuration);
            }
            OnInteractProgress?.Invoke(holdProgress);
        }
        else if (holdProgress > 0f)
        {
            holdProgress = 0f;
            OnInteractProgress?.Invoke(0f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
