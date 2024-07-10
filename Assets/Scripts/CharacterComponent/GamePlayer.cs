
using System;
using CharacterComponent.CharacterAction;
using UnityEngine;
using UnityEngine.Serialization;

namespace CharacterComponent
{
    public partial class GamePlayer
    {
        [SerializeField] private Animator animator;
        
        [SerializeField] private InputEventHandler inputEventHandler;
        
        [SerializeField] private CharacterController characterController;
        
        [SerializeField] private Weapon weapon;
        
        [Tooltip("Useful for rough ground")]
        [SerializeField] private float groundedOffset = 0.15f;
        
        [Space(10)]
        [Tooltip("")]
        [Range(0.0f, 10f)]
        [SerializeField] private float walkSpeed = 5f;
        
        [FormerlySerializedAs("RunSpeed")]
        [Tooltip("")]
        [Range(0.0f, 20f)]
        [SerializeField] private float runSpeed = 10f;
        
        [FormerlySerializedAs("JumpTimeout")]
        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        [SerializeField] private float jumpTimeout = 0.45f;

        [FormerlySerializedAs("FallTimeout")] [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        [SerializeField] private float fallTimeout = 0.15f;
        
        [FormerlySerializedAs("Gravity")] [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        [SerializeField] private float gravity = -17.0f;
        
        [FormerlySerializedAs("JumpHeight")]
        [Space(10)]
        [Tooltip("The height the player can jump")]
        [SerializeField] private float jumpHeight = 1.0f;
    }
    
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(InputEventHandler))]
    public partial class GamePlayer : MonoBehaviour
    {
        /// <summary>
        /// 값이 높아질수록 이동 방향에 맞춘 캐릭터 회전이 느려진다.
        /// </summary>
        private const float RotationSmoothTime = 0.1f;
        private const float SpeedChangeRate = 10f;
        private const float TerminalVelocity = 53.0f;
        private const int MaxAttackCombo = 2;
        
        private readonly int _movementSpeedAnimationID = Animator.StringToHash("MovementSpeed");
        private readonly int _jumpAnimationID = Animator.StringToHash("Jump");
        private readonly int _groundAnimationID = Animator.StringToHash("IsGround");
        private readonly int _attackingAnimationID = Animator.StringToHash("Attack");
        private readonly int _attackComboAnimationID = Animator.StringToHash("AttackCombo");
        private readonly int _rollingAnimationID = Animator.StringToHash("Rolling");
        private readonly int _fallingAnimationID = Animator.StringToHash("Falling");
        
        private GameObject _mainCamera;
        
        private float _rotationVelocity;
        private float _targetRotation;
        
        private float _movementSpeed;
        private float _animationBlend;
        private float _verticalVelocity;
        
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private bool _isGrounded;
        private bool _isJumping;
        private bool _isAttacking;
        private bool _isRolling;
        
        private int _attackCombo;

        private RollingAction _rollingAction;
        
        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

            if (_rollingAction == null)
            {
                _rollingAction = gameObject.AddComponent<RollingAction>();
                _rollingAction.Initialize(characterController);
            }
            
            inputEventHandler.OnAttackEvent = OnAttack;
            inputEventHandler.OnJumpEvent = OnJump;
            inputEventHandler.OnRollingEvent = OnRolling;
        }
        
        private void OnAnimatorMove()
        {
            if (_isAttacking)
                return;

            if (_rollingAction.IsRolling)
                return;
            
            JumpAndGravity();
            GroundedCheck();
            
            OnMove();
        }
        
        private void JumpAndGravity()
        {
            if (_isGrounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = fallTimeout;

                // update animator if using character
                animator.SetBool(_jumpAnimationID, false);
                // animator.SetBool(_fallingAnimationID, false);

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_isJumping && _jumpTimeoutDelta <= 0.0f) 
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                    // update animator if using character
                    animator.SetBool(_jumpAnimationID, true);
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = jumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    // animator.SetBool(_fallingAnimationID, true);
                }

                // if we are not grounded, do not jump
                _isJumping = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < TerminalVelocity)
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            var spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
            _isGrounded = Physics.CheckSphere(spherePosition, 0.2f, LayerMask.GetMask("Default"));

            animator.SetBool(_groundAnimationID, _isGrounded);
        }
        
        private void OnDrawGizmosSelected()
        {
            var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = _isGrounded ? transparentGreen : transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z),
                0.2f);
        }
        
        private void OnMove()
        {
            var targetSpeed = inputEventHandler.IsRun ? runSpeed : walkSpeed;
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
                _movementSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);
            
                // round speed to 3 decimal places
                _movementSpeed = Mathf.Round(_movementSpeed * 1000f) / 1000f;
            }
            else
            {
                _movementSpeed = targetSpeed;
            }
            
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            
            if (_animationBlend < 0.01f) 
                _animationBlend = 0f;
            
            // normalise input direction
            var inputDirection = new Vector3(moveVector.x, 0.0f, moveVector.y).normalized;
            
            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (moveVector != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                var rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);
            
                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
            
            var targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            
            // move the player
            characterController.Move(
                targetDirection.normalized * (_movementSpeed * Time.deltaTime) + 
                new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            
            // update animator if using character
            animator.SetFloat(_movementSpeedAnimationID, _animationBlend);
        }

        private void SetStopMove()
        {
            _movementSpeed = 0f;
            _animationBlend = 0f;
            animator.SetFloat(_movementSpeedAnimationID, _animationBlend);
            characterController.SimpleMove(Vector3.zero);
        }
        
        private void OnAttack()
        {
            if (!_isGrounded)
                return;
            
            if (_isAttacking)
                return;

            SetStopMove();
            
            if (_attackCombo == MaxAttackCombo)
                _attackCombo = 0;

            animator.SetInteger(_attackComboAnimationID, ++_attackCombo);
            animator.SetTrigger(_attackingAnimationID);
        }
        
        private void OnJump()
        {
            if (_isAttacking)
                return;
            
            _isJumping = true;
        }

        private void OnRolling()
        {
            if ( _isAttacking || _jumpTimeoutDelta > 0f )
                return;

            if (_rollingAction.IsRolling)
                return;
            
            SetStopMove();
            
            animator.SetTrigger(_rollingAnimationID);
        }
        
        /// <summary>
        /// Call when start to attack motion in animation event
        /// </summary>
        private void OnAttackStart()
        {
            //Debug.Log("OnAttackStart");
            _isAttacking = true;
        }
        
        /// <summary>
        /// Call when start to attack event in animation event
        /// </summary>
        private void OnAttackEventStart()
        {
            //Debug.Log("OnAttackEventStart");
            weapon.SetAbleEvent(true);
        }

        /// <summary>
        /// Call when finished to attack event in animation event
        /// </summary>
        private void OnAttackEventFinish()
        {
            //Debug.Log("OnAttackEventFinish");
            weapon.SetAbleEvent(false);
        }

        /// <summary>
        /// Call when finished to attack motion in animation event
        /// </summary>
        private void OnAttackFinish()
        {
            //Debug.Log("OnAttackFinish");
            _isAttacking = false;
        }
    }
}