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
    public float wallJumpUpForce = 6f;     // Vertical launch power
    public float wallJumpOutForce = 6f;    // How hard it kicks you away from the wall
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

        rb.AddForce(wallForward * wallRunSpeed * wallRunForceMultiplier, ForceMode.Force);

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude > wallRunSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * wallRunSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }

        if (!(wallLeft && Input.GetKey(KeyCode.A)) && !(wallRight && Input.GetKey(KeyCode.D)))
        {
            rb.AddForce(-wallNormal * 50f, ForceMode.Force);
        }
    }

    void WallJump()
    {
        bool isRightWallActive = wallRight && (!wallLeft || rightWallHit.distance < leftWallHit.distance);
        Vector3 wallNormal = isRightWallActive ? rightWallHit.normal : leftWallHit.normal;

        // Replace velocity entirely: same launch every time, ignores mass and previous speed
        rb.linearVelocity = (Vector3.up * wallJumpUpForce) + (wallNormal * wallJumpOutForce);

        wallJumpTimer = 0.15f;
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
                targetTilt = camTilt;
            }
            else if (isLeftWallActive)
            {
                targetTilt = -camTilt;
            }
        }

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);

        if (camScript != null)
        {
            camScript.currentTilt = currentTilt;
        }
    }
}