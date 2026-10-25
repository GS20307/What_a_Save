using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WallDash : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;

    [Header("Layer Masks")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;

    [Header("Wall Jump / Dash Settings")]
    public float wallCheckDistance = 0.8f;
    public float wallJumpUpForce = 5f;     // Vertical launch power
    public float wallJumpOutForce = 6f;    // How hard it kicks you away from the wall
    public float minJumpHeight = 1.0f;     // Must be this high above the ground to wall jump
    public float dashCooldown = 0.3f;      // Prevents spamming / double triggers

    private bool wallLeft;
    private bool wallRight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private float nextDashTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // WallRun already has a wall jump. If both run, one Space press launches twice.
        if (GetComponent<WallRun>() != null)
        {
            enabled = false;
        }
    }

    void Update()
    {
        CheckForWalls();

        if (Input.GetKeyDown(KeyCode.Space)
            && (wallLeft || wallRight)
            && AboveGround()
            && Time.time >= nextDashTime)
        {
            ExecuteWallJump();
        }
    }

    void CheckForWalls()
    {
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    void ExecuteWallJump()
    {
        // Use the closer wall if both sides are touching
        bool useRight = wallRight && (!wallLeft || rightWallHit.distance < leftWallHit.distance);
        Vector3 wallNormal = useRight ? rightWallHit.normal : leftWallHit.normal;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Vector3 jumpDirection = (transform.up * wallJumpUpForce) + (wallNormal * wallJumpOutForce);
        rb.AddForce(jumpDirection, ForceMode.Impulse);

        nextDashTime = Time.time + dashCooldown;
    }
}