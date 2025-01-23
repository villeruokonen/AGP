using UnityEngine;
using UnityEngine.InputSystem;

public struct MovementInputData
{
    public Vector3 moveInput;
    public Vector2 lookInput;
    public bool isJumping;
    public bool isCrouching;
    public bool isSprinting;
    public bool didSwitchCamera;
}

public class MovementInput : MonoBehaviour
{
    public void OnMoveInput(InputAction.CallbackContext ctx)
    {
        var vec2 = ctx.ReadValue<Vector2>();
        _moveInput = new Vector3(vec2.x, 0, vec2.y);
    }

    public void OnLookInput(InputAction.CallbackContext ctx)
    {
        _lookInput = ctx.ReadValue<Vector2>() * Time.smoothDeltaTime;
    }

    public void OnCrouchInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            _crouching = true;
        else if (ctx.canceled)
            _crouching = false;
    }

    public void OnJumpInput(InputAction.CallbackContext ctx)
    {
        _jumping = ctx.ReadValueAsButton();
    }

    public void OnSprintInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            _sprinting = true;
        else if (ctx.canceled)
            _sprinting = false;
    }

    public void OnCameraSwitchInput(InputAction.CallbackContext ctx)
    {
        if (!ctx.started)
            return;
        
        _switchedCamera = true;
    }

    private Vector3 _moveInput;
    private Vector2 _lookInput;
    private bool _crouching;
    private bool _jumping;
    private bool _sprinting;
    private bool _switchedCamera;

    public MovementInputData PollInput()
    {
        var data = new MovementInputData()
        {
            moveInput = _moveInput,
            lookInput = _lookInput,
            isJumping = _jumping,
            isCrouching = _crouching,
            isSprinting = _sprinting,
            didSwitchCamera = _switchedCamera
        };
        
        // Clear flag
        if (_switchedCamera)
            _switchedCamera = false;

        return data;

    }
}
