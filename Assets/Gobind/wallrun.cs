using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class wallrun : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;
    private camera camScript;

    [Header("Wall Running")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunSpeed = 7f;          // Lowered default speed
    public float wallRunForceMultiplier = 10f; // Lowered default force
    public float wallJumpForce = 7f;
    public float wallCheckDistance = 0.8f;
    public float minJumpHeight = 1.5f;

    [Header("Camera Tilt")]
    public float camTilt = 15f;      
    public float tiltSpeed = 10f;    
    private float currentTilt = 0f;

    private bool wallLeft;
    private bool wallRight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    public bool isWallRunning;

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

        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround())
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
        }
    }

    void StartWallRun()
    {
        isWallRunning = true;
        rb.useGravity = false; 
    }

    void WallRunningMovement()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((transform.forward - wallForward).magnitude > (transform.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // Apply movement force
        rb.AddForce(wallForward * wallRunSpeed * wallRunForceMultiplier, ForceMode.Force);

        // STRICT SPEED CAP: Keeps you from accelerating infinitely into a rocket ship
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude > wallRunSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * wallRunSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }

        // Keep player pressed slightly against the wall
        if (!(wallLeft && Input.GetKey(KeyCode.A)) && !(wallRight && Input.GetKey(KeyCode.D)))
            rb.AddForce(-wallNormal * 100f, ForceMode.Force);
    }

    void WallJump()
    {
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 jumpDirection = transform.up + wallNormal * 1.5f;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(jumpDirection * wallJumpForce, ForceMode.Impulse);

        StopWallRun();
    }

    void StopWallRun()
    {
        isWallRunning = false;
        rb.useGravity = true; 
    }

    void HandleCameraTilt()
    {
        float targetTilt = 0f;

        if (isWallRunning)
        {
            if (wallRight) targetTilt = camTilt;     
            if (wallLeft) targetTilt = -camTilt;    
        }

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);

        if (camScript != null)
        {
            camScript.currentTilt = currentTilt;
        }
    }
}