using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]private Animator animator;
    [SerializeField]private CharacterController ccon;
    PlayerInput playerInput;
    InputAction move;
    InputAction moveSprint;
    private Vector2 currentInput;
    private float currentSprint;
    #region animation variables
    private int walkingHash;
    private int runningHash;
    #endregion
    
    #region movement variables


    [SerializeField]private float moveSpeed;
    [SerializeField]private float sprintSpeed;
    [SerializeField]private float rotationSmoothTime;
    [SerializeField]private float acceleration;
    [SerializeField]private float jumpHeight;
    [SerializeField]private float customGravity;
    [SerializeField]private float jumpCD;
    [SerializeField]private float fallTimeout;
    [SerializeField]private bool grounded;//remove serializefield later
    [SerializeField]private float groundedRadius;
    [SerializeField]private LayerMask groundedLayerMask;

    private GameObject mainCamera;
    #endregion
    
    
    #region private movement Variables

    private float jumptimeoutDelta;
    private float falltimeoutDelta;
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    #endregion
    
    
    private void Awake()
    {
        playerInput = new PlayerInput();
        move = playerInput.movement.KB;
        moveSprint = playerInput.movement.Sprint;
        move.performed += ctx => HandleMovement(ctx.ReadValue<Vector2>());
        moveSprint.performed += ctx => HandleSprint(ctx.ReadValue<float>());
        walkingHash = Animator.StringToHash("walk");
        runningHash = Animator.StringToHash("run");



        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }


    private void Start()
    {

    }
    private void OnEnable()
    {
        move.Enable();
        moveSprint.Enable();
    }
    private void OnDisable()
    {
        move.Disable();
        moveSprint.Disable();

    }

    private void HandleMovement(Vector2 playerInput)
    {
        //Debug.Log("Player Movement : " + playerInput);
        currentInput = playerInput;
        //handle animations everytime player input is changed
        if(playerInput.normalized.magnitude > 0.5f)
        {
            animator.SetBool(walkingHash,true);
        }
        else if(playerInput.normalized.magnitude <0.1f)
        {
            animator.SetBool(walkingHash,false);
        }
    }
    private void HandleSprint(float sprint)
    {
        Debug.Log("sprinting");
        currentSprint = sprint;
        //handle sprint animation here
        if(currentSprint > 0.2f) 
        animator.SetBool(runningHash, true);
        else if(currentSprint <0.2f)
        animator.SetBool(runningHash, false);
    }

    private void JumpAndGravity()
        {
            if (grounded)
            {
                falltimeoutDelta = fallTimeout;
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }
                /*
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }
                */

                // jump timeout
                if (jumptimeoutDelta >= 0.0f)
                {
                    jumptimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                jumptimeoutDelta = jumpCD;

                // fall timeout
                if (falltimeoutDelta >= 0.0f)
                {
                    falltimeoutDelta -= Time.deltaTime;
                }

                /*
                // if we are not grounded, do not jump
                _input.jump = false;
                */
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += customGravity * Time.deltaTime;
            }
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - 0.1f,
                transform.position.z);
            grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundedLayerMask,
                QueryTriggerInteraction.Ignore);
        }
        private void LateUpdate()
        {
            //JumpAndGravity();
            //GroundedCheck();
            Move();
        }
        private void Move()
        {
            //float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            float targetSpeed = ((currentInput.magnitude>0.2f) && (currentSprint>0.2f))?sprintSpeed:moveSpeed;
            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            //if (_input.move == Vector2.zero) targetSpeed = 0.0f;
            if(currentInput.magnitude<0.1f)
            {
                targetSpeed = 0.0f;
            }    
            //if(currentInput.magnitude > 0.2f)
            Debug.Log("entered here " + currentInput.magnitude);
            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(ccon.velocity.x, 0.0f, ccon.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * acceleration);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * acceleration);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(currentInput.x, 0.0f, currentInput.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (currentInput != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    rotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            ccon.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

}
