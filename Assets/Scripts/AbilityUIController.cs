using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUIController : MonoBehaviour
{
    [Header("References")]
    public SombraTranslocator ability;
    public Image icon;
    public TextMeshProUGUI cooldownText;

    [Header("Colors")]
    public Color readyColor = Color.white;
    public Color activeColor = Color.red;

    void Update()
    {
        if (ability.OnCooldown)
        {
            icon.color = activeColor;
            cooldownText.text = ability.CooldownRemaining > 0f
                ? Mathf.Ceil(ability.CooldownRemaining).ToString()
                : "";
        }
        else
        {
            icon.color = readyColor;
            cooldownText.text = "";
        }
    }
}
