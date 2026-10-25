
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Movement Compatibility")]
    public Behaviour movementScript;

    private Rigidbody rb;
    private bool isDashing;
    private bool canDash = true;
    public float dashStaminaCost = 30f;
    private PlayerStamina stamina;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        stamina = GetComponent<PlayerStamina>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isDashing)
        {
            if (stamina == null || stamina.TryUse(dashStaminaCost))
                StartCoroutine(PerformDash());
        }
    }

    IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        // Temporarily stop normal movement.
        if (movementScript != null)
            movementScript.enabled = false;

        Vector3 direction = transform.forward;
        direction.y = 0f;
        direction.Normalize();

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            rb.linearVelocity = new Vector3(
                direction.x * dashSpeed,
                rb.linearVelocity.y,
                direction.z * dashSpeed
            );

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;

        // Restore normal movement.
        if (movementScript != null)
            movementScript.enabled = true;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }
}
