using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina")]
    public float maxStamina = 100f;
    public float regenPerSecond = 25f;
    public float regenDelay = 1f;        // pause after using stamina before it refills

    [Header("UI")]
    public Image fillImage;              // the Fill image of the stamina bar

    private float current;
    private float lastUseTime = -999f;

    public float Normalized => current / maxStamina;

    void Start()
    {
        current = maxStamina;
        UpdateUI();
    }

    void Update()
    {
        if (Time.time - lastUseTime >= regenDelay && current < maxStamina)
        {
            current = Mathf.Min(maxStamina, current + regenPerSecond * Time.deltaTime);
        }
        UpdateUI();
    }

    // Returns true and spends stamina if there is enough; returns false otherwise
    public bool TryUse(float cost)
    {
        if (current < cost) return false;
        current -= cost;
        lastUseTime = Time.time;
        return true;
    }

    void UpdateUI()
    {
        if (fillImage != null) fillImage.fillAmount = Normalized;
    }
}