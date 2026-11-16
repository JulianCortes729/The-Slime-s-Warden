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

    private void OnDestroy()
    {
        // "new PlayerInputActions()" crea un asset de acciones: si no se libera, queda uno huérfano en cada carga del nivel
        _inputActions.Dispose();
    }

    private void Update()
    {
        // Bloqueado (diálogo o muerte): sin dirección, así el Brujo no gira ni camina en el sitio
        blackboard.moveInput = blackboard.IsControlLocked ? 0f : _inputActions.Player.Move.ReadValue<Vector2>().x;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        // Bloqueado: el salto no se guarda, así no sale solo al cerrar el diálogo
        if (blackboard.IsControlLocked) return;
        blackboard.jumpBufferTimer = blackboard.jumpBufferTime; // Resetea el temporizador de salto bufferizado al presionar salto
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        if (blackboard.IsControlLocked || blackboard.bulletCount <= 0) return;
        blackboard.shootIntent = true;
    }
}
