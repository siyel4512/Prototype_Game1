using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class ClueEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image background;

    private static readonly Color NormalColor   = new Color(1f, 1f, 1f, 0.08f);
    private static readonly Color SelectedColor = new Color(0.3f, 0.65f, 1f, 0.45f);

    public void Setup(string title, Action onClick)
    {
        label.text = title;
        GetComponent<Button>().onClick.AddListener(() => onClick.Invoke());
        SetSelected(false);
    }

    public void SetSelected(bool on)
    {
        if (background != null)
            background.color = on ? SelectedColor : NormalColor;
    }
}
