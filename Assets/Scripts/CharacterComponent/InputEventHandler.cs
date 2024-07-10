using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterComponent
{
    /// <summary>
    /// Input event handler
    /// </summary>
    public class InputEventHandler : MonoBehaviour
    {
        /// <summary>
        /// Reference to the input action asset
        /// </summary>
        [SerializeField] private InputActionAsset actionAsset;
    
        /// <summary>
        /// Input action for move
        /// </summary>
        private InputAction _moveAction;
    
        /// <summary>
        /// Input action for run
        /// </summary>
        private InputAction _runAction;
    
        /// <summary>
        /// Event for jump action
        /// </summary>
        public Action OnJumpEvent;

        /// <summary>
        /// Event for attack action
        /// </summary>
        public Action OnAttackEvent;

        /// <summary>
        /// Event for rolling action
        /// </summary>
        public Action OnRollingEvent;

        /// <summary>
        /// Property to get the move value
        /// </summary>
        public Vector2 MoveValue => _moveAction.ReadValue<Vector2>();
    
        /// <summary>
        /// Property to check if the run action is pressed
        /// </summary>
        public bool IsRun => _runAction.IsPressed();
    
        /// <summary>
        /// Awake is called when the script instance is being loaded
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked; 
            
            if (!actionAsset)
                throw new Exception("Input action asset is not assigned");
        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        private void Start()
        {
            _moveAction = actionAsset.FindActionMap("gameplay").FindAction("move");
            _runAction = actionAsset.FindActionMap("gameplay").FindAction("run");
            
            actionAsset.FindActionMap("gameplay").FindAction("jump").performed += OnJump;
            actionAsset.FindActionMap("gameplay").FindAction("attack").performed += OnAttack;
            actionAsset.FindActionMap("gameplay").FindAction("rolling").performed += OnRolling;
        }

        /// <summary>
        /// This function is called when jump
        /// </summary>
        private void OnJump(InputAction.CallbackContext context)
            => OnJumpEvent?.Invoke();
    
        /// <summary>
        /// This function is called when attacking
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void OnAttack(InputAction.CallbackContext context)
            => OnAttackEvent?.Invoke();
        
        /// <summary>
        /// This function is called when rolling
        /// </summary>
        /// <param name="context"></param>
        private void OnRolling(InputAction.CallbackContext context)
            => OnRollingEvent?.Invoke();
    
        /// <summary>
        /// This function is called when the behaviour becomes enabled
        /// </summary>
        private void OnEnable()
            => actionAsset.FindActionMap("gameplay").Enable();

        /// <summary>
        /// This function is called when the behaviour becomes disabled
        /// </summary>
        private void OnDisable()
            => actionAsset.FindActionMap("gameplay").Disable();
    }
}