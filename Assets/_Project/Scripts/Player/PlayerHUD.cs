
using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lowHealthColor = Color.red;

    private void OnEnable()
    {
        playerHealth.OnHealthChanged += UpdateHealth;
    }

    private void OnDisable()
    {
        playerHealth.OnHealthChanged -= UpdateHealth;
    }

    private void Start()
    {
        messageText.text = "";
        UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void UpdateHealth(int current, int max)
    {
        healthText.text = $"HP: {current}";
        healthText.color = current <= max * 0.3f
            ? lowHealthColor
            : normalColor;
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
    }
}
