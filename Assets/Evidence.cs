using UnityEngine;

public class Evidence : MonoBehaviour
{
    public string evidenceName = "USB Drive";
    [TextArea] public string description = "USB found on the desk.";

    [Header("Puzzle & Lock Settings")]
    [Tooltip("1 = Code Comparison, 2 = Cryptography, 3 = Logic Gates")]
    public int puzzleType = 1;
    public GameObject linkedLock; // 
}