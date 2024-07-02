using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterComponent
{
    public class InputComponent : MonoBehaviour
    {
        public InputActionAsset actionAsset;
        public CharacterController characterController;
        
        private InputAction moveAction;
        private InputAction runAction;
        
        private void Start()
        {
            // find the "move" action, and keep the reference to it, for use in Update
            moveAction = actionAsset.FindActionMap("gameplay").FindAction("move");

            // for the "run" action, we add a callback method for when it is performed
            runAction = actionAsset.FindActionMap("gameplay").FindAction("run");
            
            // for the "jump" action, we add a callback method for when it is performed
            actionAsset.FindActionMap("gameplay").FindAction("jump").performed += OnJump;
        }
        
        private void FixedUpdate()
        {
            if (moveAction == null)
                return;

            if (runAction == null)
                return;
            
            if( runAction.IsPressed() )
                Debug.Log("Run!");
            
            // our update loop polls the "move" action value each frame
            var moveVector = moveAction.ReadValue<Vector2>();
            var fixedVector = new Vector3(moveVector.x, 0f, moveVector.y);
            
            // move character
            characterController.Move(fixedVector * Time.fixedDeltaTime);
        }
        
        private void OnJump(InputAction.CallbackContext context)
        {
            // this is the "jump" action callback method
            Debug.Log("Jump!");
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