using TMPro;
using UnityEngine;

public class ShowMissedUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI missedText;

    private void Start()
    {
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        GameManager.Instance.OnMissedChanged += UpdateMissedUI;
    }

    private void UpdateMissedUI(int missedHit)
    {
        if (missedText)
        {
            missedText.SetText($"{missedHit}");
        }
    }
}
