using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoostersUIManager : MonoBehaviour
{
    [Header("References")]
    public DvaBoosters ability;
    public Image icon;
    public TextMeshProUGUI cooldownText;

    [Header("Colors")]
    public Color readyColor = Color.white;
    public Color activeColor = Color.red;

    void Update()
    {
        if (ability.IsBoosting)
        {
            icon.color = activeColor;
        }
        else
        {
            icon.color = readyColor;
        }

        if (ability.CooldownRemaining > 0f)
        {
            cooldownText.text = Mathf.Ceil(ability.CooldownRemaining).ToString();
        }
        else
        {
            cooldownText.text = "";
        }
    }
}
