using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterComponent
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(InputEventHandler))]
    public class GamePlayer : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private InputEventHandler inputEventHandler;
        [SerializeField] private CharacterController characterController;
        
        private const float RotationSmoothTime = 0.12f;
        private const float SpeedChangeRate = 10f;
        private const float TerminalVelocity = 53.0f;
        
        private readonly int movementSpeedAnimationID = Animator.StringToHash("MovementSpeed");
        private readonly int JumpAnimationID = Animator.StringToHash("Jump");
        private readonly int GroundAnimationID = Animator.StringToHash("IsGround");
        private readonly int AttackingAnimationID = Animator.StringToHash("Attack");
        private readonly int FallingAnimationID = Animator.StringToHash("Falling");
       
        private GameObject mainCamera;
        
        private float rotationVelocity;
        private float targetRotation;
        private float movementSpeed;
        private float animationBlend;
        private float verticalVelocity;
        
        private float jumpTimeoutDelta;
        private float fallTimeoutDelta;

        private bool isGrounded;
        private bool isJumping;
        private bool isAttacking;
        
        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.18f;
        
        [Space(10)]
        [Tooltip("")]
        [Range(0.0f, 10f)]
        public float WalkSpeed = 5f;
        
        [Tooltip("")]
        [Range(0.0f, 20f)]
        public float RunSpeed = 10f;
        
        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.45f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;
        
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -17.0f;
        
        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.0f;
        
        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        
        private void FixedUpdate()
        {
            if (isAttacking)
                return;
            
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
                animator.SetBool(JumpAnimationID, false);
                animator.SetBool(FallingAnimationID, false);

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
                    animator.SetBool(JumpAnimationID, true);
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
                    animator.SetBool(FallingAnimationID, true);
                }

                // if we are not grounded, do not jump
                isJumping = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (verticalVelocity < TerminalVelocity)
            {
                verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            var spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            isGrounded = Physics.CheckSphere(spherePosition, 0.2f, LayerMask.GetMask("Default"));

            animator.SetBool(GroundAnimationID, isGrounded);
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
            var targetSpeed = inputEventHandler.IsRun ? RunSpeed : WalkSpeed;
            var moveVector = inputEventHandler.MoveValue;
            
            if ( moveVector == Vector2.zero)
                targetSpeed = 0f;
            
            const float speedOffset = 0.1f;
            
            var currentHorizontalSpeed = 
                new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z).magnitude;
            
            /*
             * 새로운 유니티 Input System은 Keyboard 입력에 대한 analogMovement 기능을 제공하지 않는다.
             */
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
        
        

        private void OnAttackStart()
        {
            Debug.Log("OnAttackStart");
            isAttacking = true;
        }

        private void OnAttackFinish()
        {
            Debug.Log("OnAttackFinish");
            isAttacking = false;
        }

        private void OnAttack(InputAction.CallbackContext context)
        {
            if (isAttacking)
                return;
            
            isAttacking = true;
            animator.SetTrigger(AttackingAnimationID);
        }
    }
}