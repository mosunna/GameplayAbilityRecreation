using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecallUIController : MonoBehaviour
{
    [Header("References")]
    public TracerAbilities ability;
    public Image icon;
    public TextMeshProUGUI cooldownText;

    [Header("Colors")]
    public Color readyColor = Color.white;
    public Color activeColor = Color.red;

    void Update()
    {
        if (ability.IsRecalling)
        {
            icon.color = activeColor;
        }
        else
        {
            icon.color = readyColor;
        }

        if (ability.RecallCooldownRemaining > 0f)
        {
            cooldownText.text = Mathf.Ceil(ability.RecallCooldownRemaining).ToString();
        }
        else
        {
            cooldownText.text = "";
        }
    }
}
