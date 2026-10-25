using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class basic_movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 5f;
    public float movementMultiplier = 10f;
    public float groundDrag = 6f;

    [Header("Jump Settings")]
    public float jumpCooldown = 0.05f;
    public LayerMask groundMask;

    // Private state (all declared here, at class level)
    private Rigidbody rb;
    private WallRun wallRunScript;
    private bool isGrounded;
    private bool isTouchingWall;
    private Vector3 wallNormal;
    private float landTimer;
    private bool hasLeftGround = true;
    private float lastGroundContactTime = -1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.sleepThreshold = 0f;
        wallRunScript = GetComponent<WallRun>();
        landTimer = jumpCooldown;
    }

    void Update()
    {
        isGrounded = Time.time - lastGroundContactTime < 0.1f;

        if (!isGrounded)
        {
            hasLeftGround = true;
            landTimer = 0f;
        }
        else if (hasLeftGround)
        {
            landTimer += Time.deltaTime;
        }

        if (wallRunScript != null && wallRunScript.isWallRunning)
        {
            rb.linearDamping = 0f;
        }
        else if (isGrounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && landTimer >= jumpCooldown && isGrounded && hasLeftGround && (wallRunScript == null || !wallRunScript.isWallRunning))
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        if (wallRunScript != null && wallRunScript.isWallRunning)
        {
            return;
        }

        MovePlayer();

        if (wallRunScript == null || wallRunScript.wallJumpTimer <= 0f)
        {
            SpeedControl();
        }

        isTouchingWall = false;
    }

    void MovePlayer()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.forward * z + transform.right * x).normalized;

        if (isTouchingWall)
        {
            float dot = Vector3.Dot(moveDir, wallNormal);
            if (dot < 0)
            {
                moveDir -= wallNormal * dot;
            }
        }

        float airMultiplier = isGrounded ? 1f : 0.4f;
        rb.AddForce(moveDir * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Force);
    }

    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint c in collision.contacts)
        {
            bool onGroundLayer = (groundMask.value & (1 << c.otherCollider.gameObject.layer)) != 0;
            if (onGroundLayer && c.normal.y > 0.7f)
            {
                lastGroundContactTime = Time.time;
                break;
            }
        }

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > -0.3f && contact.normal.y < 0.3f)
            {
                isTouchingWall = true;
                wallNormal = contact.normal;
                break;
            }
        }
    }

    void Jump()
    {
        landTimer = 0f;
        hasLeftGround = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
}