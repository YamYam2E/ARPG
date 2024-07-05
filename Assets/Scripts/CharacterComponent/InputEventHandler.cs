using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputEventHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset actionAsset;
    
    private InputAction moveAction;
    private InputAction runAction;
    
    public Action OnJumpEvent;
    public Action OnAttackEvent;
    
    public Vector2 MoveValue => moveAction.ReadValue<Vector2>();
    public bool IsRun => runAction.IsPressed();
    
    private void Start()
    {
        moveAction = actionAsset.FindActionMap("gameplay").FindAction("move");
        runAction = actionAsset.FindActionMap("gameplay").FindAction("run");
            
        actionAsset.FindActionMap("gameplay").FindAction("jump").performed += OnJump;
        actionAsset.FindActionMap("gameplay").FindAction("attack").performed += OnAttack;
    }
    
    private void OnJump(InputAction.CallbackContext context)
        => OnJumpEvent?.Invoke();
    
    private void OnAttack(InputAction.CallbackContext context)
        => OnAttackEvent?.Invoke();
    
    private void OnEnable()
        => actionAsset.FindActionMap("gameplay").Enable();

    private void OnDisable()
        => actionAsset.FindActionMap("gameplay").Disable();
}
