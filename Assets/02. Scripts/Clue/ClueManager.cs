using System;
using System.Collections.Generic;
using UnityEngine;

public class ClueManager : MonoBehaviour
{
    public static ClueManager Instance { get; private set; }

    public event Action<ClueData> OnClueAdded;

    private readonly List<ClueData> collectedClues = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddClue(ClueData clue)
    {
        if (collectedClues.Contains(clue)) return;
        collectedClues.Add(clue);
        OnClueAdded?.Invoke(clue);
    }

    public bool HasClue(string clueId) => collectedClues.Exists(c => c.clueId == clueId);

    public IReadOnlyList<ClueData> GetAllClues() => collectedClues;
}
