using System.Data.Common;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private const float WALKING_SPEED = 4f;
    private const float RUNNING_SPEED = 6f;
    private const float MAX_SLIDING_SPEED = 9.5f;
    private const float WALL_RUN_SPEED = 8.3f;
    private const float MAX_SPEED = 12.5f;


    public bool toggleCrouch = false;
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode runKey = KeyCode.LeftShift;

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
    private float doubelJumpCoefficient = 0.6f;
    private bool jump = false;

    // get the last collided object so player can't keep refreshing jump
    private GameObject lastCollided;
    
    // crouch
    private bool isCrouching = false;
    private float crouchMultiplier = 1f;
    private float defaulfHeight;
    // slide
    private bool slide = false;
    private bool isSliding = false;
    private float slideForce = 2f;
    private float slideMultiplier = 1f;

    // run
    private bool isRunning = false;

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
        lookVelocity = transform.InverseTransformDirection(rb.velocity);
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
            airMultiplierHorizontal = 0.2f;
        }
        if (isRunning)
        {
            runHorizontalLimiter = 0.3f;
        }
        
        rb.AddForce(forward * transform.forward * speedForce * airMultiplierForward, mode);
        rb.AddForce(horizontal * transform.right * speedForce * airMultiplierHorizontal * runHorizontalLimiter, mode);
        

    }

    // simulate friction and drag since the default system's feel is not that great
    void FrictionControl()
    {
        if (grounded)
        {
            if (isSliding && rb.velocity.magnitude <= MAX_SPEED)
            {
                rb.AddForce(stoppingForce * -rb.velocity.normalized * slideMultiplier, mode);
                return;
            }
            if (rb.velocity.magnitude > max_velocity)
            {
               rb.AddForce(stoppingForce * -rb.velocity.normalized * (rb.velocity.magnitude - max_velocity + 1), mode);
            }
            
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
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (jumpCount == 1)
            {
                DoubleJumpBoost();
            }
            rb.AddForce(Vector3.up * jumpForce, mode);
            
            if (isWallRunning)
            {
                rb.AddForce(collisionSurfaceNorm * 5f, mode);
                rb.AddForce(transform.forward * 5f, mode);
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
            rb.AddForce((Vector3.down + transform.forward / Mathf.Tan(slopeAngle * Mathf.Deg2Rad)).normalized * 0.5f, mode);
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

 
   


}
