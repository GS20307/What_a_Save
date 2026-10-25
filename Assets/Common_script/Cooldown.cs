using UnityEngine;
using UnityEngine.UI;

public class AbilityCooldown : MonoBehaviour
{
    public float cooldownDuration = 2f;

    [Header("UI")]
    public Image fillImage;              // the Fill image of the cooldown bar

    private float readyAtTime;

    public bool IsReady => Time.time >= readyAtTime;

    void Update()
    {
        if (fillImage != null)
        {
            float remaining = Mathf.Max(0f, readyAtTime - Time.time);
            fillImage.fillAmount = 1f - (remaining / cooldownDuration); // fills up as it recharges
        }
    }

    // Returns true and starts the cooldown if the ability is ready
    public bool TryStart()
    {
        if (!IsReady) return false;
        readyAtTime = Time.time + cooldownDuration;
        return true;
    }
}