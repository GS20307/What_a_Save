using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WallRun : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;
    private camera camScript;

    [Header("Layer Masks")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;

    [Header("Wall Running & Launch Settings")]
    public float wallRunSpeed = 12f;
    public float wallRunForceMultiplier = 10f;
    public float wallJumpUpForce = 9f;     // Vertical launch power
    public float wallJumpOutForce = 12f;   // How hard it kicks you away from the wall
    public float wallCheckDistance = 0.9f;
    public float minJumpHeight = 1.0f;

    [Header("Camera Tilt Settings")]
    public float camTilt = 15f;
    public float tiltSpeed = 15f;
    private float currentTilt = 0f;

    private bool wallLeft;
    private bool wallRight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    public bool isWallRunning;

    // Counts down after a wall jump; basic_movement reads this to skip its speed clamp
    public float wallJumpTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camScript = GetComponent<camera>();
    }

    void Update()
    {
        if (wallJumpTimer > 0f) wallJumpTimer -= Time.deltaTime;

        CheckForWall();
        StateMachine();
        HandleCameraTilt();
    }

    void FixedUpdate()
    {
        if (isWallRunning)
        {
            WallRunningMovement();
        }
    }

    void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    void StateMachine()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Start Wall Run: Touching wall + holding W + airborne (and not just wall-jumped)
        if ((wallLeft || wallRight) && verticalInput > 0f && AboveGround() && wallJumpTimer <= 0f)
        {
            if (!isWallRunning)
                StartWallRun();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                WallJump();
            }
        }
        else
        {
            if (isWallRunning)
                StopWallRun();

            // Allow launching off the wall even if W isn't held, as long as airborne
            else if ((wallLeft || wallRight) && AboveGround() && wallJumpTimer <= 0f && Input.GetKeyDown(KeyCode.Space))
            {
                WallJump();
            }
        }
    }

    void StartWallRun()
    {
        isWallRunning = true;
        rb.useGravity = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, 0f), rb.linearVelocity.z);
    }

    void WallRunningMovement()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        bool isRightWallActive = wallRight && (!wallLeft || rightWallHit.distance < leftWallHit.distance);
        Vector3 wallNormal = isRightWallActive ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((transform.forward - wallForward).magnitude > (transform.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        // Apply movement force
        rb.AddForce(wallForward * wallRunSpeed * wallRunForceMultiplier, ForceMode.Force);

        // Strict speed cap
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude > wallRunSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * wallRunSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }

        // Keep player pressed slightly against the wall
        if (!(wallLeft && Input.GetKey(KeyCode.A)) && !(wallRight && Input.GetKey(KeyCode.D)))
        {
            rb.AddForce(-wallNormal * 50f, ForceMode.Force);
        }
    }

    void WallJump()
    {
        // Determine which wall we are launching off of
        bool isRightWallActive = wallRight && (!wallLeft || rightWallHit.distance < leftWallHit.distance);
        Vector3 wallNormal = isRightWallActive ? rightWallHit.normal : leftWallHit.normal;

        // Clear velocity for a clean, snappy launch
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Launch up and away from the wall
        Vector3 jumpDirection = (transform.up * wallJumpUpForce) + (wallNormal * wallJumpOutForce);
        rb.AddForce(jumpDirection, ForceMode.Impulse);

        wallJumpTimer = 0.3f;
        StopWallRun();
    }

    void StopWallRun()
    {
        if (!isWallRunning) return;
        isWallRunning = false;
        rb.useGravity = true;
    }

    void HandleCameraTilt()
    {
        float targetTilt = 0f;

        if (isWallRunning)
        {
            bool isRightWallActive = wallRight && (!wallLeft || rightWallHit.distance < leftWallHit.distance);
            bool isLeftWallActive = wallLeft && (!wallRight || leftWallHit.distance < rightWallHit.distance);

            if (isRightWallActive)
            {
                targetTilt = camTilt;  // Wall on the RIGHT -> Tilt RIGHT
            }
            else if (isLeftWallActive)
            {
                targetTilt = -camTilt; // Wall on the LEFT -> Tilt LEFT
            }
        }

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);

        if (camScript != null)
        {
            camScript.currentTilt = currentTilt;
        }
    }
}