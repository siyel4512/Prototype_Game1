using System;
using UnityEngine;

public class ClueCondition : MonoBehaviour, IPuzzleCondition
{
    [SerializeField] private int requiredCount = 1;

    public bool IsMet { get; private set; }
    public event Action OnConditionMet;

    private void Start()
    {
        if (ClueManager.Instance != null)
            ClueManager.Instance.OnClueAdded += OnClueAdded;
    }

    private void OnDestroy()
    {
        if (ClueManager.Instance != null)
            ClueManager.Instance.OnClueAdded -= OnClueAdded;
    }

    private void OnClueAdded(ClueData _)
    {
        if (IsMet) return;
        if (ClueManager.Instance.GetAllClues().Count >= requiredCount)
        {
            IsMet = true;
            OnConditionMet?.Invoke();
        }
    }
}
