using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactPrompt = "E - 조사";

    public abstract void Interact();

    public string GetInteractPrompt() => interactPrompt;
}
