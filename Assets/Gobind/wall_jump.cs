using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WallDash : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;

    [Header("Wall Jump / Dash Settings")]
    public LayerMask whatIsWall;
    public float wallCheckDistance = 0.8f;
    public float wallJumpUpForce = 8f;     // Vertical launch power
    public float wallJumpOutForce = 12f;   // How hard it kicks you away from the wall

    private bool wallLeft;
    private bool wallRight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        CheckForWalls();

        // Press Space while touching a wall to dash/jump off it to the opposite side
        if (Input.GetKeyDown(KeyCode.Space) && (wallLeft || wallRight))
        {
            ExecuteWallJump();
        }
    }

    void CheckForWalls()
    {
        // Cast rays left and right to detect nearby walls
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, whatIsWall);
    }

    void ExecuteWallJump()
    {
        // Figure out which wall we are currently touching
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        // Clear existing vertical velocity so every wall jump feels crisp and identical
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Calculate bounce direction: straight up + pushed away from the wall surface
        Vector3 jumpDirection = (transform.up * wallJumpUpForce) + (wallNormal * wallJumpOutForce);

        // Apply instant impulse force to fling the player toward the opposite wall
        rb.AddForce(jumpDirection, ForceMode.Impulse);
    }
}