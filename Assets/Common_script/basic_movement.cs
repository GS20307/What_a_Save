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

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isTouchingWall;
    private Vector3 wallNormal;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevents the player capsule from tipping over
    }

    void Update()
    {
        // Check if the player's feet are touching the Ground layer
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Handle drag so movement feels snappy
        if (isGrounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0f;

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
        SpeedControl();
        isTouchingWall = false; // Reset wall contact flag each frame
    }

    void MovePlayer()
    {
        // Get WASD inputs
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.forward * z + transform.right * x).normalized;

        // If touching a wall, strip out any input pushing directly into it
        if (isTouchingWall)
        {
            float dot = Vector3.Dot(moveDir, wallNormal);
            if (dot < 0)
            {
                moveDir -= wallNormal * dot; // Removes the component pushing into the wall
            }
        }

        // Apply movement force
        if (isGrounded)
        {
            rb.AddForce(moveDir * moveSpeed * movementMultiplier, ForceMode.Force);
        }
        else
        {
            // Slight air control
            rb.AddForce(moveDir * moveSpeed * movementMultiplier * 0.4f, ForceMode.Force);
        }
    }

    void SpeedControl()
    {
        // Limit horizontal speed to prevent runaway acceleration against walls
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Check if the contact normal is vertical (a wall)
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
        // Reset vertical velocity before jumping for consistent height
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
}