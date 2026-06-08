using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class ClueUIPanel : MonoBehaviour
{
    public static bool IsInventoryOpen { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject panel;

    [Header("왼쪽 - 목록")]
    [SerializeField] private Transform clueListContainer;
    [SerializeField] private GameObject clueEntryPrefab;

    [Header("오른쪽 - 상세")]
    [SerializeField] private GameObject detailRoot;
    [SerializeField] private TextMeshProUGUI detailTitle;
    [SerializeField] private TextMeshProUGUI detailContent;
    [SerializeField] private GameObject emptyHint;

    private readonly List<ClueEntryUI> entries = new();
    private int selectedIndex = -1;

    private void Start()
    {
        if (ClueManager.Instance == null) return;
        ClueManager.Instance.OnClueAdded += OnClueAdded;
        panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (ClueManager.Instance != null)
            ClueManager.Instance.OnClueAdded -= OnClueAdded;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            TogglePanel();
            return;
        }

        if (!IsInventoryOpen) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ClosePanel();
            return;
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            Navigate(1);
        else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            Navigate(-1);

        if (Keyboard.current.enterKey.wasPressedThisFrame && selectedIndex >= 0)
            SelectEntry(selectedIndex);
    }

    private void TogglePanel()
    {
        if (IsInventoryOpen) ClosePanel();
        else OpenPanel();
    }

    private void OpenPanel()
    {
        IsInventoryOpen = true;
        panel.SetActive(true);
        RebuildList();
        if (entries.Count > 0)
            SelectEntry(0);
        else
            ShowEmptyHint();
    }

    private void ClosePanel()
    {
        IsInventoryOpen = false;
        panel.SetActive(false);
    }

    private void Navigate(int dir)
    {
        if (entries.Count == 0) return;
        SelectEntry(Mathf.Clamp(selectedIndex + dir, 0, entries.Count - 1));
    }

    public void SelectEntry(int index)
    {
        if (index < 0 || index >= entries.Count) return;

        if (selectedIndex >= 0 && selectedIndex < entries.Count)
            entries[selectedIndex].SetSelected(false);

        selectedIndex = index;
        entries[selectedIndex].SetSelected(true);

        var clue = ClueManager.Instance.GetAllClues()[index];
        ShowDetail(clue);
    }

    private void ShowDetail(ClueData clue)
    {
        if (emptyHint != null) emptyHint.SetActive(false);
        if (detailRoot != null) detailRoot.SetActive(true);
        if (detailTitle != null) detailTitle.text = clue.clueName;
        if (detailContent != null)
            detailContent.text = string.IsNullOrEmpty(clue.description) ? "(내용 없음)" : clue.description;
    }

    private void ShowEmptyHint()
    {
        if (detailRoot != null) detailRoot.SetActive(false);
        if (emptyHint != null) emptyHint.SetActive(true);
    }

    private void OnClueAdded(ClueData _)
    {
        if (IsInventoryOpen) RebuildList();
    }

    private void RebuildList()
    {
        foreach (var e in entries)
            if (e != null) Destroy(e.gameObject);
        entries.Clear();
        selectedIndex = -1;

        var clues = ClueManager.Instance.GetAllClues();
        for (int i = 0; i < clues.Count; i++)
        {
            int captured = i;
            var go = Instantiate(clueEntryPrefab, clueListContainer);
            var entry = go.GetComponent<ClueEntryUI>();
            entry.Setup(clues[i].clueName, () => SelectEntry(captured));
            entries.Add(entry);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(clueListContainer as RectTransform);
    }
}
