using System.Data.Common;
using System.IO.MemoryMappedFiles;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private const float WALKING_SPEED = 4f;
    private const float RUNNING_SPEED = 6f;
    private const float MAX_SLIDING_SPEED = 9.5f;
    private const float MAX_SPEED = 12.5f;
    private const float WALL_RUN_TIME = 2f;
    private const float WALL_RUN_SPEED = 8.3f;

    [Range(60f, 120f)] public float FieldOfView = 103f;
    public bool toggleCrouch = false;
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode runKey = KeyCode.LeftShift;
    public Transform cam;
    public Camera MainCam;
    Rigidbody rb;
    CapsuleCollider capcol;


    private ForceMode mode = ForceMode.VelocityChange;

    // esc to pause the game
    private bool gamePause = false;

    // velocity local sapce
    Vector3 lookVelocity;

    // forces
    private float forward;
    private float horizontal;

    // speed control
    private float speedForce = 0.5f;
    private float max_velocity = WALKING_SPEED;
    
    // friction control
    private bool grounded = false;
    private float frictionCoefficient = 1f;
    private float stoppingForce = 0.5f;
    
    // jump & double jump boost control
    private float jumpForce = 7f;
    private int jumpCount = 2;
    private float doubelJumpCoefficient = 0.4f;
    private bool jump = false;

    // get the last collided object so player can't keep refreshing jump
    private int lastCollided;
    
    // crouch
    private bool isCrouching = false;
    private float defaulfHeight;
    // slide
    private bool slide = false;
    private bool isSliding = false;
    private float slideForce = 2f;
    private float slideMultiplier = 1f;

    // run
    private bool isRunning = false;

    // wall run
    private Vector3 collisionSurfaceNorm;
    private bool isWallRunning = false;
    private float wallRunCD = 1f;
    private float startTime;
    private float exitTime = 0f;
    private int wallPosition = 0; // 1 is left, -1 is right, 0 is no wall
    private float camTiltAngle = 15f;
    private float currentCamAngle = 0;

    // FoV
    private float fovMultiplier = 1.13f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capcol = GetComponent<CapsuleCollider>();
    }

    void Start()
    {
        MainCam.fieldOfView = FieldOfView;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        defaulfHeight = capcol.height;
        // keep the cursor at the centre of the screen and invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        if (!gamePause) { 
            WallRunCamTilt();
            MovementInputHandle();
            FoVControl();
        }
        cursorLock();
    }

    void FixedUpdate()
    {
        Movement();
        FrictionControl();
    }

    void OnCollisionEnter(Collision collision)
    {
        collisionSurfaceNorm = collision.GetContact(0).normal;
        if (collision.collider.tag == "Ground" || Mathf.Abs(Vector3.Dot(collisionSurfaceNorm, Vector3.right)) < 0.1f)
        {
            ResetJump();
            grounded = true;
        }
        // check a bunch of requirement for initaiting a wall run: surface normal angle, if grounded, if input, if the same wall is ran again
        if (Mathf.Abs(Vector3.Dot(collisionSurfaceNorm, Vector3.up)) < 0.5f && !grounded && (horizontal != 0 || forward != 0) 
            && ((collision.gameObject.GetInstanceID() != lastCollided) || Time.time - exitTime > wallRunCD))
        {
            startTime = Time.time;
            isWallRunning = true;
        }
        else
        {
            exitTime = Time.time;
            isWallRunning = false;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (Mathf.Abs(Vector3.Dot(collision.GetContact(0).normal, Vector3.right)) < 0.1f)
        {
            grounded = true;
        }
        
    }

    void OnCollisionExit(Collision collision)
    {
        lastCollided = collision.gameObject.GetInstanceID();
        if (collision.collider.tag == "Ground")
        {
            grounded = false;
        }
        if (isWallRunning)
        {
            isWallRunning = false;
        }
    }


    /*
     * Custom functions below
     */

    // controls pausing game and locking cursor
    void cursorLock()
    {
        if (Input.GetKeyDown("escape"))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;

                Cursor.visible = true;
                Time.timeScale = 0;
                gamePause = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;
                gamePause = false;
            }
        }
    }

    // handle keyboard or controller input
    void MovementInputHandle()
    {
        forward = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && jumpCount > 0)
        {
            jump = true;
        }

        // crouch
        if (toggleCrouch)
        {
            if (Input.GetKeyDown(crouchKey) && isCrouching == false)
            {   
                isCrouching = true;
                slide = true;
            }
            else if (Input.GetKeyDown(crouchKey) && isCrouching == true)
            {
                isCrouching = false;
            }
        }
        else
        {
            if (Input.GetKeyDown(crouchKey))
            {
                isCrouching = true;
                slide = true;
            }
            if (Input.GetKeyUp(crouchKey))
            {
                isCrouching = false;
            }
        }

        if (Input.GetKeyDown(runKey))
        {
            
            isRunning = true;
        }
        if (Input.GetKeyUp(runKey))
        {
            isRunning = false;
        }

    }

    // Handles all the inputs from keyboard
    void Movement()
    {
        float airMultiplierHorizontal = 1f;
        float airMultiplierForward = 1f;
        float runHorizontalLimiter = 1f;
        lookVelocity = transform.InverseTransformDirection(rb.velocity);

        if (!isWallRunning)
        {
            wallPosition = 0;
            rb.useGravity = true;
            Crouch();
            if (!isCrouching)
            {   
               Run();
            } 
            Jump();
        
            // if in the air, limit player's control over movement
            if (!grounded)
            {
                airMultiplierForward = 0.1f;
                airMultiplierHorizontal = 0.2f;
            }
            // limit horizontal movement when running for realism
            if (isRunning)
            {
                runHorizontalLimiter = 0.3f;
            }
            
            rb.AddForce(forward * transform.forward * speedForce * airMultiplierForward, mode);
            rb.AddForce(horizontal * transform.right * speedForce * airMultiplierHorizontal * runHorizontalLimiter, mode);
        }
        else
        {
            rb.useGravity = false;
            StartWallRun();
            Jump();
            StopWallRun();
        }
        
        

    }

    void StartWallRun()
    {
        if (grounded)
        {
            isWallRunning = false;
        }
        ResetJump();

        rb.velocity = Vector3.zero;

        // check if the wall we hit is on the right with cross product & limit camera look angle if wall running
        if (Vector3.Dot(transform.forward, Vector3.Cross(Vector3.down, collisionSurfaceNorm)) > 0.6f)
        {
            rb.velocity = Vector3.Cross(Vector3.down, collisionSurfaceNorm).normalized * WALL_RUN_SPEED;
            wallPosition = 1;
        }
        // check if the wall we hit is on the left with cross product & limit camera look angle if wall running
        else if (Vector3.Dot(transform.forward, Vector3.Cross(Vector3.up, collisionSurfaceNorm)) > 0.6f)
        {
            rb.velocity = Vector3.Cross(Vector3.up, collisionSurfaceNorm).normalized * WALL_RUN_SPEED;
            wallPosition = -1;
        }
        else
        {
            isWallRunning = false;
            exitTime = Time.time;
        }
        // add a force to stick the player to the wall
        rb.AddForce(-collisionSurfaceNorm * 0.1f);
    }

    void StopWallRun()
    {
        if (Time.time - startTime > WALL_RUN_TIME)
        {
            isWallRunning = false;
            rb.useGravity = true;
            rb.AddForce(collisionSurfaceNorm * 0.01f);
            exitTime = Time.time;
        }
    }

    void WallRunCamTilt()
    {
        float change = 0;
        
        if (wallPosition == -1) // right
        {
            change = (camTiltAngle - currentCamAngle) * Time.deltaTime * 4f;
        }
        else if (wallPosition == 1) // left
        {
            change = (-camTiltAngle - currentCamAngle) * Time.deltaTime * 4f;
        }
        else
        {         
            change = -currentCamAngle * Time.deltaTime * 4f;

        }
        currentCamAngle += change;
        cam.localRotation = Quaternion.Euler(cam.localEulerAngles.x, 0f, currentCamAngle);
    }

    void FoVControl()
    {
        if (isSliding || isWallRunning)
        {
            MainCam.fieldOfView += (FieldOfView * fovMultiplier - MainCam.fieldOfView) * Time.deltaTime * 3f;
        }
        else if (grounded)
        {
            MainCam.fieldOfView -= (MainCam.fieldOfView - FieldOfView) * Time.deltaTime * 3f;
        }
    }

    // simulate friction and drag since the default system's feel is not that great
    void FrictionControl()
    {
        if (grounded)
        {
            // sliding friction
            if (isSliding && rb.velocity.magnitude < MAX_SPEED)
            {
                rb.AddForce(stoppingForce * -rb.velocity.normalized * slideMultiplier, mode);
                return;
            }
            // walking & running friction
            if (rb.velocity.magnitude > max_velocity)
            {
               rb.AddForce(stoppingForce * -rb.velocity.normalized * (rb.velocity.magnitude - max_velocity + 1), mode);
            }
            // stopping friction
            if (Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2) != 0)
            {
                if ((forward == 0 || (lookVelocity.z > 0 && forward < 0) || (lookVelocity.z < 0 && forward > 0)) && grounded)
                {
                    rb.AddForce(transform.forward * -lookVelocity.z * speedForce * frictionCoefficient, mode);
                }
                if ((horizontal == 0 || (lookVelocity.x > 0 && horizontal < 0) || (lookVelocity.x < 0 && horizontal > 0)) && grounded)
                {
                    rb.AddForce(transform.right * -lookVelocity.x * speedForce * frictionCoefficient, mode);
                }
            }
        }
    }
    void Jump()
    {
        if (jump && jumpCount > 0)
        {
            if (isWallRunning)
            {
                isWallRunning = false;
                rb.useGravity = true;
                rb.AddForce(collisionSurfaceNorm * 1f, mode);
                rb.AddForce(transform.forward * 1f, mode);
            }
            else
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                if (jumpCount == 1)
                {
                    DoubleJumpBoost();
                }
                
            }
            rb.AddForce(Vector3.up * jumpForce, mode);
            jump = false;
            jumpCount--;
            
        }
    }

    void DoubleJumpBoost() 
    {
        // Titanfall style opposite boost
        if ((forward > 0 && lookVelocity.z < 0) || (forward < 0 && lookVelocity.z > 0))
        {
            rb.velocity = Vector3.zero;
            rb.AddForce(transform.forward * Mathf.Sign(forward) * jumpForce * doubelJumpCoefficient, mode);
        }
        
        if (horizontal != 0)
        {
            rb.AddForce(transform.right * Mathf.Sign(horizontal) * jumpForce * doubelJumpCoefficient * 0.3f, mode);
        }
    }

    void ResetJump()
    {
        jumpCount = 2;
    }

    void Crouch()
    {
        
        if (isCrouching == true)
        {
            capcol.height = defaulfHeight * 0.5f;
            max_velocity = WALKING_SPEED * 0.5f;
            Slide(); 
        }
        else
        {
            capcol.height = defaulfHeight;
            max_velocity = WALKING_SPEED;
            isSliding = false;
            slideMultiplier = 1f;
        }
    }
    
    void Slide()
    {
        if (isSliding)
        {
            horizontal = 0;
        }

        if (rb.velocity.magnitude <= WALKING_SPEED * 0.5f)
        {
            isSliding = false;
            slideMultiplier = 1f;
        }



        if (isRunning)
        {
            if (grounded && slide)
            {
                rb.AddForce(transform.forward * slideForce, mode);
                slide = false;
                isSliding = true;
                slideMultiplier = 1.1f;
                max_velocity = MAX_SLIDING_SPEED;
            }
        }

        // gain speed on a slope
        float slopeAngle = Mathf.Abs(Vector3.Angle(collisionSurfaceNorm, Vector3.up));
        
        if (slopeAngle <= 60 && slopeAngle > 5 && grounded && isSliding && rb.velocity.magnitude <= MAX_SLIDING_SPEED)
        {   
            rb.AddForce((Vector3.down + transform.forward / Mathf.Tan(slopeAngle * Mathf.Deg2Rad)).normalized * 0.3f, mode);
        }
    }

    void Run()
    {
        if (isRunning)
        {
            max_velocity = RUNNING_SPEED;
        }
        else
        {
            max_velocity = WALKING_SPEED;
        }
    }

}
