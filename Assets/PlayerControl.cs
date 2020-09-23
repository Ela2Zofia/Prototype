using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private const float WALKING_SPEED = 8f;
    private const float RUNNING_SPEED = 14;
    private const float MAX_SLIDING_SPEED = 25f;
    private const float WALL_RUN_SPEED = 25f;


    public bool toggleCrouch = false;
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode runKey = KeyCode.LeftShift;

    Rigidbody rb;
    CapsuleCollider capcol;


    private ForceMode mode = ForceMode.Force;

    // esc to pause the game
    private bool gamePause = false;

    // velocity local sapce
    Vector3 lookVelocity;

    // forces
    private float forward;
    private float horizontal;

    // speed control
    private float speedForce = 30.0f;
    private float max_velocity = WALKING_SPEED;
    
    // friction control
    private bool grounded = true;
    private float frictionCoefficient = 1.2f;
    private float stoppingForce = 40f;
    
    // jump & double jump boost control
    private float jumpForce = 800f;
    private int jumpCount = 2;
    private float doubelJumpCoefficient = 0.6f;
    private bool jump = false;

    // get the last collided object so player can't keep refreshing jump
    private GameObject lastCollided;
    
    // crouch
    private bool isCrouching = false;
    private bool slide = false;
    private float slideForce = 500f;
    private float crouchMultiplier = 1f;
    private float defaulfHeight;

    // run
    private bool isRunning = false;
    private float runMultiplier = 1f;

    // wall run
    private bool isWallRunning = false;
    private bool wallRunReady = true;
    private Vector3 collisionSurfaceNorm;
    private float wallRunCD = 0.4f;
    private float exitTime;

    

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capcol = GetComponent<CapsuleCollider>();
    }

    void Start()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        defaulfHeight = capcol.height;
        // keep the cursor at the centre of the screen and invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        if (!gamePause) { 
            MovementInputHandle();
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
        lastCollided = collision.gameObject;
        if (collision.collider.tag == "Ground")
        {
            grounded = false;
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
        Crouch();
        if (!isCrouching)
        {   
            Run();
        } 
        Jump();
        
        // if in the air, limit player's control over movement
        if (!grounded && !isWallRunning)
        {
            airMultiplierForward = 0.1f;
            airMultiplierHorizontal = 0.5f;
        }
        if (isRunning)
        {
            runHorizontalLimiter = 0.4f;
        }

        // check if the current velocity in local space exceeds max velocity, if so, do not allow input force anymore
        lookVelocity = transform.InverseTransformDirection(rb.velocity);
        rb.AddForce(horizontal * transform.right * speedForce * airMultiplierHorizontal * crouchMultiplier*runHorizontalLimiter, mode);
        rb.AddForce(forward * transform.forward * speedForce * airMultiplierForward * crouchMultiplier * runMultiplier, mode);

    }


    void Jump()
    {
        if (jump && jumpCount > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (jumpCount == 1)
            {
                DoubleJumpBoost();
            }
            rb.AddForce(Vector3.up * jumpForce, mode);
            
            if (isWallRunning)
            {
                rb.AddForce(collisionSurfaceNorm*500f, mode);
            }
            jump = false;
            jumpCount--;
        }
    }

    void DoubleJumpBoost() 
    {
        if ((forward > 0 && lookVelocity.z < 0) || (forward < 0 && lookVelocity.z > 0))
        {
            rb.velocity = Vector3.zero;
            rb.AddForce(transform.forward * Mathf.Sign(forward) * jumpForce * doubelJumpCoefficient, mode);
        }
        
        if (horizontal != 0)
        {
            rb.AddForce(transform.right * Mathf.Sign(horizontal) * jumpForce * doubelJumpCoefficient * 0.8f, mode);
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
            crouchMultiplier = 0.5f;
            if (rb.velocity.magnitude > 10f && rb.velocity.magnitude < MAX_SLIDING_SPEED)
            {
                if (grounded && slide)
                {
                    rb.AddForce(transform.forward * slideForce, mode);
                    slide = false;
                }
            }
            // gain speed on a slope
            float slopeAngle = Mathf.Abs(Vector3.Angle(collisionSurfaceNorm, Vector3.up));
            if (slopeAngle <= 60 && slopeAngle > 5 && grounded && rb.velocity.magnitude < MAX_SLIDING_SPEED)
            {
                rb.AddForce((Vector3.down + Vector3.down/Mathf.Tan(slopeAngle * Mathf.Deg2Rad)) * 10f);
            }
            
        }
        else
        {
            capcol.height = defaulfHeight;
            max_velocity = WALKING_SPEED;
            crouchMultiplier = 1f;
            
        }
    }
    
    void Run()
    {
        if (isRunning)
        {
            max_velocity = RUNNING_SPEED;
            runMultiplier = 0.8f*RUNNING_SPEED/(lookVelocity.magnitude+1f);
        }
        else
        {
            max_velocity = WALKING_SPEED;
            runMultiplier = 1f;
        }
    }

    void StartWallRun()
    {
        if (grounded)
        {
            isWallRunning = false;
        }

       
        rb.AddForce(Vector3.up * 18f);
        
    }
    void StopWallRun()
    {
        ResetJump();
    }

    // simulate friction and drag since the default system's feel is not that great
    void FrictionControl()  
    {
        // slowdown
        if (rb.velocity.magnitude > max_velocity && grounded)
        {
            rb.AddForce(stoppingForce * -rb.velocity.normalized, mode);
        }
        
        // simulation stoping friction
        if (Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2) != 0)
        {
            if((forward == 0 || (lookVelocity.z > 0 && forward < 0) || (lookVelocity.z < 0 && forward > 0)) && grounded)
            {
                rb.AddForce(transform.forward * -lookVelocity.z * speedForce * frictionCoefficient, mode);
            }
            else if((forward == 0 || (lookVelocity.z > 0 && forward < 0) || (lookVelocity.z < 0 && forward > 0)) && !grounded)
            {
                rb.AddForce(transform.forward * -lookVelocity.z * speedForce * frictionCoefficient* 0.01f, mode);
            }
            
            if ((horizontal == 0 || (lookVelocity.x > 0 && horizontal < 0) || (lookVelocity.x < 0 && horizontal > 0)) && grounded)
            {
                rb.AddForce(transform.right * -lookVelocity.x * speedForce * frictionCoefficient, mode);
            }
            
            else if((horizontal == 0 || (lookVelocity.x > 0 && horizontal < 0) || (lookVelocity.x < 0 && horizontal > 0)) && !grounded)
            {
                rb.AddForce(transform.right * -lookVelocity.x * speedForce * frictionCoefficient*0.01f, mode);
            }
           
        }
        
    }
   


}
