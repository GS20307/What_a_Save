
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerSlide : MonoBehaviour
{
    [Header("Slide Settings")]
    public float slideSpeed = 12f;
    public float slideDuration = 1.5f;
    public float slideCooldown = 0.5f;
    public float slideDeceleration = 3f;

    [Header("Input Settings")]
    public KeyCode slideKey = KeyCode.LeftControl;
    public float minSpeedToSlide = 3f;

    [Header("Collider Settings")]
    public float slideHeight = 1f;

    [Header("Camera Settings")]
    public Transform playerCamera;
    public float cameraDrop = 0.5f;
    public float slideCameraAngle = 30f;
    public float cameraSmoothSpeed = 8f;

    [Header("Movement Compatibility")]
    public basic_movement movementScript;
    public AbilityCooldown slideCooldownBar;

    private Rigidbody rb;
    private CapsuleCollider capsule;

    private float originalHeight;
    private Vector3 originalCenter;

    private Vector3 cameraOriginalPosition;
    private Quaternion cameraOriginalRotation;
    private bool cameraInitialized;

    private Vector3 slideDirection;
    private float currentSlideSpeed;

    private bool isSliding;
    private bool canSlide = true;
    private Coroutine slideCoroutine;

    public bool IsSliding => isSliding;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        originalHeight = capsule.height;
        originalCenter = capsule.center;

        if (movementScript == null)
            movementScript = GetComponent<basic_movement>();

        CacheCameraTransform();
    }

    void CacheCameraTransform()
    {
        if (playerCamera == null)
            return;

        cameraOriginalPosition = playerCamera.localPosition;
        cameraOriginalRotation = playerCamera.localRotation;
        cameraInitialized = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(slideKey) && canSlide && !isSliding)
            TryStartSlide();
    }

    void FixedUpdate()
    {
        if (!isSliding)
            return;

        Vector3 velocity = rb.linearVelocity;

        rb.linearVelocity = new Vector3(
            slideDirection.x * currentSlideSpeed,
            velocity.y,
            slideDirection.z * currentSlideSpeed
        );

        currentSlideSpeed = Mathf.MoveTowards(
            currentSlideSpeed,
            0f,
            slideDeceleration * Time.fixedDeltaTime
        );
    }

    void LateUpdate()
    {
        if (playerCamera == null || !cameraInitialized)
            return;

        Vector3 targetPosition = cameraOriginalPosition;
        Quaternion targetRotation = cameraOriginalRotation;

        if (isSliding)
        {
            // Lower the camera during the slide.
            targetPosition.y -= cameraDrop;

            // Tilt the camera upward relative to its normal view.
            targetRotation = cameraOriginalRotation *
                Quaternion.Euler(-slideCameraAngle, 0f, 0f);
        }

        // Smoothly blend to the target camera position and rotation.
        float smooth = 1f - Mathf.Exp(
            -cameraSmoothSpeed * Time.deltaTime
        );

        playerCamera.localPosition = Vector3.Lerp(
            playerCamera.localPosition,
            targetPosition,
            smooth
        );

        playerCamera.localRotation = Quaternion.Slerp(
            playerCamera.localRotation,
            targetRotation,
            smooth
        );
    }

    void TryStartSlide()
    {
        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        if (horizontalVelocity.magnitude < minSpeedToSlide)
            return;

        if (horizontalVelocity.magnitude < minSpeedToSlide)
            return;

        if (slideCooldownBar != null && !slideCooldownBar.TryStart())
            return;

        slideDirection = horizontalVelocity.normalized;

        // Start at the greater of the current speed or configured slide speed.
        currentSlideSpeed = Mathf.Max(
            horizontalVelocity.magnitude,
            slideSpeed
        );

        slideCoroutine = StartCoroutine(PerformSlide());
    }

    IEnumerator PerformSlide()
    {
        isSliding = true;
        canSlide = false;

        if (movementScript != null)
            movementScript.enabled = false;

        // Lower the collider while keeping its bottom near the ground.
        float newHeight = Mathf.Clamp(
            slideHeight,
            capsule.radius * 2f,
            originalHeight
        );

        capsule.height = newHeight;
        capsule.center = originalCenter -
            Vector3.up * ((originalHeight - newHeight) / 2f);

        float elapsed = 0f;

        while (elapsed < slideDuration && currentSlideSpeed > 0.1f)
        {
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        EndSlide();

        yield return new WaitForSeconds(slideCooldown);
        canSlide = true;
        slideCoroutine = null;
    }

    void EndSlide()
    {
        isSliding = false;

        capsule.height = originalHeight;
        capsule.center = originalCenter;

        if (movementScript != null)
            movementScript.enabled = true;
    }

    void OnDisable()
    {
        if (capsule != null)
        {
            capsule.height = originalHeight;
            capsule.center = originalCenter;
        }

        if (movementScript != null)
            movementScript.enabled = true;

        isSliding = false;
        canSlide = true;
        slideCoroutine = null;
    }
}
