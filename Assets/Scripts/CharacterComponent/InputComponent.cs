using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterComponent
{
    public class InputComponent : MonoBehaviour
    {
        public Animator animator;
        public InputActionAsset actionAsset;
        public CharacterController characterController;
        
        private InputAction moveAction;
        private InputAction runAction;
        private GameObject mainCamera;
        
        private const float RotationSmoothTime = 0.12f;
        private const float SpeedChangeRate = 10f;
        
        private float rotationVelocity;
        private float targetRotation;
        private float movementSpeed;
        private float animationBlend;
        
        private float verticalVelocity;
        private float terminalVelocity = 53.0f;
        
        // timeout deltatime
        private float jumpTimeoutDelta;
        private float fallTimeoutDelta;
        
        private int movementSpeedAnimationID;
        private int jumpAnimationID;
        private int groundAnimationID;
        private int fallingAnimationID;
        
        private bool isGrounded;
        private bool isJumping;
        
        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.1f;
        
        [Space(10)]
        [Tooltip("")]
        [Range(0.0f, 10f)]
        public float WalkSpeed = 5f;
        
        [Tooltip("")]
        [Range(0.0f, 20f)]
        public float RunSpeed = 10f;
        
        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;
        
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;
        
        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 0.8f;
        
        private void Awake()
        {
            // get a reference to our main camera
            if (mainCamera == null)
            {
                mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }
        
        private void Start()
        {
            // find the "move" action, and keep the reference to it, for use in Update
            moveAction = actionAsset.FindActionMap("gameplay").FindAction("move");

            // for the "run" action, we add a callback method for when it is performed
            runAction = actionAsset.FindActionMap("gameplay").FindAction("run");
            
            // for the "jump" action, we add a callback method for when it is performed
            actionAsset.FindActionMap("gameplay").FindAction("jump").performed += OnJump;
            
            movementSpeedAnimationID = Animator.StringToHash("MovementSpeed");
            jumpAnimationID = Animator.StringToHash("Jump");
            groundAnimationID = Animator.StringToHash("IsGround");
        }
        
        private void FixedUpdate()
        {
            JumpAndGravity();
            
            GroundedCheck();

            OnMove();
        }
        
        private void JumpAndGravity()
        {
            if (isGrounded)
            {
                // reset the fall timeout timer
                fallTimeoutDelta = FallTimeout;

                // update animator if using character
                animator.SetBool(jumpAnimationID, false);
                animator.SetBool(fallingAnimationID, false);

                // stop our velocity dropping infinitely when grounded
                if (verticalVelocity < 0.0f)
                {
                    verticalVelocity = -2f;
                }

                // Jump
                if (isJumping && jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    animator.SetBool(jumpAnimationID, true);
                }

                // jump timeout
                if (jumpTimeoutDelta >= 0.0f)
                {
                    jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (fallTimeoutDelta >= 0.0f)
                {
                    fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    animator.SetBool(fallingAnimationID, true);
                }

                // if we are not grounded, do not jump
                isJumping = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (verticalVelocity < terminalVelocity)
            {
                verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            var spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            isGrounded = Physics.CheckSphere(spherePosition, 0.2f, LayerMask.GetMask("Default"));

            animator.SetBool(groundAnimationID, isGrounded);
        }
        
        private void OnDrawGizmosSelected()
        {
            var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = isGrounded ? transparentGreen : transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                0.2f);
        }

        private void OnMove()
        {
            if (moveAction == null || runAction == null)
                return;

            var targetSpeed = runAction.IsPressed() ? RunSpeed : WalkSpeed;
            
            // our update loop polls the "move" action value each frame
            var moveVector = moveAction.ReadValue<Vector2>();

            if (moveVector == Vector2.zero)
                targetSpeed = 0f;
            
            const float speedOffset = 0.1f;
            var currentHorizontalSpeed = new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z).magnitude;
            
            var inputMagnitude = /*moveAction.analogMovement ? _input.move.magnitude :*/ 1f;
            
            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                movementSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);
            
                // round speed to 3 decimal places
                movementSpeed = Mathf.Round(movementSpeed * 1000f) / 1000f;
            }
            else
            {
                movementSpeed = targetSpeed;
            }
            
            animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            
            if (animationBlend < 0.01f) 
                animationBlend = 0f;
            
            // normalise input direction
            var inputDirection = new Vector3(moveVector.x, 0.0f, moveVector.y).normalized;
            
            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (moveVector != Vector2.zero)
            {
                targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
                var rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity,
                    RotationSmoothTime);
            
                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
            
            
            var targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
            
            // move the player
            characterController.Move(
                targetDirection.normalized * (movementSpeed * Time.deltaTime) + 
                new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
            
            // update animator if using character
            animator.SetFloat(movementSpeedAnimationID, animationBlend);
        }
        
        private void OnJump(InputAction.CallbackContext context)
        {
            // this is the "jump" action callback method
            if (isJumping)
                return;

            isJumping = true;
            // animator.SetBool(jumpAnimationID, true);
        }
        
        private void OnEnable()
        {
            actionAsset.FindActionMap("gameplay").Enable();
        }
        private void OnDisable()
        {
            actionAsset.FindActionMap("gameplay").Disable();
        }
    }
}