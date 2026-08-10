using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlinkUIController : MonoBehaviour
{
    [Header("References")]
    public TracerAbilities ability;
    public Image icon;
    public TextMeshProUGUI chargesText;

    [Header("Colors")]
    public Color readyColor = Color.white;
    public Color activeColor = Color.red;

    void Update()
    {
        icon.color = ability.IsBlinking ? activeColor : readyColor;
        chargesText.text = ability.BlinkCharges.ToString();
    }
}
