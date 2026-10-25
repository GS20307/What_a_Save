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
    public float wallJumpUpForce = 9f;     // Vertical launch power
    public float wallJumpOutForce = 12f;   // How hard it kicks you toward the opposite wall
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
    private bool isWallRunning;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camScript = GetComponent<camera>();
    }

    void Update()
    {
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
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, whatIsWall);
    }

    bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    void StateMachine()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Start Wall Run: Touching wall + holding W + airborne
        if ((wallLeft || wallRight) && verticalInput > 0f && AboveGround())
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
            
            // Allow launching/jumping off the wall even if W isn't held, as long as airborne
            else if ((wallLeft || wallRight) && AboveGround() && Input.GetKeyDown(KeyCode.Space))
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

        rb.AddForce(wallForward * wallRunSpeed, ForceMode.Force);

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

        // Launch up and away toward the opposite wall
        Vector3 jumpDirection = (transform.up * wallJumpUpForce) + (wallNormal * wallJumpOutForce);
        rb.AddForce(jumpDirection, ForceMode.Impulse);

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

            // Correct tilt rules matching your preference
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