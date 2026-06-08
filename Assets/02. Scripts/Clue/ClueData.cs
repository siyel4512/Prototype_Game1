using UnityEngine;

[CreateAssetMenu(fileName = "NewClue", menuName = "Game/Clue Data")]
public class ClueData : ScriptableObject
{
    public string clueId;
    public string clueName;
    [TextArea] public string description;
    public Sprite clueImage;
}
