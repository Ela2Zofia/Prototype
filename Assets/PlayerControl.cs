using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private const float MAX_VELOCITY = 8f;
    private const float MAX_SLIDING_SPEED = 20f;

    // allowed user input
    [Range(0.1f, 100.0f)] public float senstivity = 2.0f;
    public bool invertX = false;
    public bool invertY = false;
    public bool toggleCrouch = false;
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode runKey = KeyCode.LeftShift;

    private Rigidbody rb;
    private CapsuleCollider capcol;

    // esc to pause the game
    private bool gamePause = false;

    // velocity local sapce
    Vector3 lookVelocity;

    // forces
    private float forward;
    private float horizontal;

    // speed control
    private float speedForce = 1000.0f;
    private float max_velocity = MAX_VELOCITY;

    // jump & double jump boost control
    private float jumpForce = 550f;
    private int jumpCount = 2;
    private float doubelJumpCoefficient = 0.3f;
    private bool jump = false;

    // get the last collided object so player can't keep refreshing jump
    private GameObject lastCollided;

    // friction control
    private bool grounded = true;
    private float frictionCoefficient = 1.1f;



    // wall run
    private bool isWallRunning = false;
    private bool wallRunReady = true;
    private Vector3 collisionSurfaceNorm;
    private float wallRunCD = 0.4f;
    private float exitTime;

    // crouch
    private bool isCrouching = false;
    private bool slide = false;
    private float slideForce = 400f;
    private float slideStoppingForce = 1000f;
    private float crouchMultiplier = 1f;
    private float defaulfHeight;

    // run
    private bool isRunning = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        capcol = GetComponent<CapsuleCollider>();
        defaulfHeight = capcol.height;
        // keep the cursor at the centre of the screen and invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!gamePause) {
            HorizontalMouseLook();
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
        exitTime = Time.time;

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

    // Handles horizontal look input
    void HorizontalMouseLook()
    {
        float horizontal = senstivity * Input.GetAxis("Mouse X");
        
        if (invertX)
        {
            horizontal = -horizontal;
        }


        transform.eulerAngles = new Vector3(0f,  transform.eulerAngles.y+horizontal, 0f);

    }


    // Handles all the inputs from keyboard
    void Movement()
    {
        float airMultiplier = 1f;

        Crouch();
        if (!isCrouching)
        {   
            Run();
        } 
        Jump();
        
        // if in the air, limit player's control over movement
        if (!grounded && !isWallRunning)
        {
            airMultiplier = 0.5f;
        }

        // check if the current velocity in local space exceeds max velocity, if so, do not allow input force anymore
        lookVelocity = transform.InverseTransformDirection(rb.velocity);
        
        if ((lookVelocity.x > max_velocity && horizontal > 0)||(lookVelocity.x < -max_velocity && horizontal < 0))
        {
            horizontal = 0.0000001f;
            
        }
        if ((lookVelocity.z > max_velocity && forward > 0)||(lookVelocity.z < -max_velocity && forward < 0))
        {
            forward = 0.0000001f;
        }
        rb.AddForce((forward * transform.forward+horizontal * transform.right).normalized * speedForce * Time.deltaTime*airMultiplier*crouchMultiplier);
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
            rb.AddForce(Vector3.up * jumpForce);
            
            if (isWallRunning)
            {
                rb.AddForce(collisionSurfaceNorm*500f);
            }
            jump = false;
            jumpCount--;
        }
    }

    void DoubleJumpBoost() 
    {
        if (forward != 0)
        {
            rb.AddForce(transform.forward * Mathf.Sign(forward) * jumpForce * doubelJumpCoefficient);
        }
        if (horizontal != 0)
        {
            rb.AddForce(transform.right * Mathf.Sign(horizontal) * jumpForce * doubelJumpCoefficient*0.5f);
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
            max_velocity = MAX_VELOCITY * 0.5f;
            crouchMultiplier = 0.5f;
            if (rb.velocity.magnitude > 1f)
            {
                if (grounded && slide)
                {
                    rb.AddForce(transform.forward * slideForce);
                    slide = false;
                }
            }
            // gain speed on a slope
            float slopeAngle = Mathf.Abs(Vector3.Angle(collisionSurfaceNorm, Vector3.up));
            Debug.Log(slopeAngle);
            if (slopeAngle < 60 && slopeAngle > 5 && grounded && rb.velocity.magnitude < MAX_SLIDING_SPEED)
            {
                rb.AddForce(Vector3.down * 200f * slopeAngle*Time.deltaTime);
            }
            
        }
        else
        {
            capcol.height = defaulfHeight;
            max_velocity = MAX_VELOCITY;
            crouchMultiplier = 1f;
            
        }
    }
    
    void Run()
    {
        if (isRunning)
        {
            max_velocity = MAX_VELOCITY * 2f;
        }
        else
        {
            max_velocity = MAX_VELOCITY;
        }
    }

    void StartWallRun()
    {
        if (grounded)
        {
            isWallRunning = false;
        }

       
        rb.AddForce(Vector3.up * 9.5f);
        
    }
    void StopWallRun()
    {
        ResetJump();
    }

    // simulate friction and drag since the default system's feel is not that great
    void FrictionControl()  
    {
        // slowdown sliding
        if (isCrouching && rb.velocity.magnitude > max_velocity && grounded)
        {
            rb.AddForce(slideStoppingForce * Time.deltaTime * -rb.velocity.normalized);
            return;
        }
        

        // simulation stoping friction
        if (Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2) != 0)
        {
            if((forward == 0 || (lookVelocity.z > 0 && forward < 0) || (lookVelocity.z < 0 && forward > 0)) && (grounded||isWallRunning))
            {
                rb.AddForce(transform.forward * -lookVelocity.z * speedForce * Time.deltaTime * frictionCoefficient);
            }
            else if((forward == 0 || (lookVelocity.z > 0 && forward < 0) || (lookVelocity.z < 0 && forward > 0)) && !grounded)
            {
                rb.AddForce(transform.forward * -lookVelocity.z * speedForce * Time.deltaTime * frictionCoefficient* 0.01f);
            }
            
            if ((horizontal == 0 || (lookVelocity.x > 0 && horizontal < 0) || (lookVelocity.x < 0 && horizontal > 0)) && (grounded||isWallRunning))
            {
                rb.AddForce(transform.right * -lookVelocity.x * speedForce * Time.deltaTime * frictionCoefficient);
            }
            
            else if((horizontal == 0 || (lookVelocity.x > 0 && horizontal < 0) || (lookVelocity.x < 0 && horizontal > 0)) && !grounded)
            {
                rb.AddForce(transform.right * -lookVelocity.x * speedForce * Time.deltaTime * frictionCoefficient*0.01f);
            }
           
        }
        // limit diagonla velocity by giving it an opposite force witha magnitude relative to its speed (moving friction)
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > max_velocity && grounded)
        {
            rb.AddForce(-rb.velocity * speedForce * 0.35f * Time.deltaTime);
        }
    }
   


}
