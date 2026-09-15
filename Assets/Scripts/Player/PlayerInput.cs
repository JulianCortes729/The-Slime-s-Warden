using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerBlackboard))]
public class PlayerInput : MonoBehaviour
{
    [SerializeField] private PlayerBlackboard blackboard;
    private PlayerInputActions _inputActions;

    private void Awake()
    {
        if (blackboard == null)
            blackboard = GetComponent<PlayerBlackboard>();

        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.Player.Jump.performed += OnJumpPerformed;
        _inputActions.Player.Shoot.performed += OnShootPerformed;
    }

    private void OnDisable()
    {
        _inputActions.Disable();
        _inputActions.Player.Jump.performed -= OnJumpPerformed;
        _inputActions.Player.Shoot.performed -= OnShootPerformed;
    }

    private void Update()
    {
        blackboard.moveInput = _inputActions.Player.Move.ReadValue<Vector2>().x;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        blackboard.jumpBufferTimer = blackboard.jumpBufferTime; // Resetea el temporizador de salto bufferizado al presionar salto   
    }   

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        // FIX #19: era blackboard.contBullet — renombrado a bulletCount en PlayerBlackboard
        if (blackboard.bulletCount <= 0) return;
        blackboard.shootIntent = true;
    }
}
