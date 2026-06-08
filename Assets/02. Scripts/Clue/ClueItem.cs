using UnityEngine;

public class ClueItem : InteractableObject
{
    [SerializeField] private ClueData clueData;

    public override void Interact()
    {
        if (clueData == null)
        {
            Debug.LogWarning($"[ClueItem] {gameObject.name}에 ClueData가 연결되어 있지 않습니다.");
            return;
        }
        ClueManager.Instance.AddClue(clueData);
        Debug.Log($"[ClueItem] 단서 수집: {clueData.clueName}");
        gameObject.SetActive(false);
    }
}
