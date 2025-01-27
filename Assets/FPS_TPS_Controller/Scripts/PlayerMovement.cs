using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private enum MovementState
    {
        Grounded,
        Falling,
        Climbing
    }

    private bool IsGrounded => _char.isGrounded;
    private float Gravity => -Mathf.Abs(Physics.gravity.y * _gravityFactor);

    [Header("Values")]
    [SerializeField] private float _gravityFactor = -25.8f;
    [SerializeField] private float _terminalVelocity = -120f;
    [SerializeField] private float _walkSpeed = 10;
    [SerializeField] private float _sprintSpeed = 12;
    [SerializeField] private float _crouchedWalkSpeed = 5;
    [SerializeField] private float _jumpHeight = 12;
    [SerializeField] private float _crouchJumpFactor = 0.8f;
    [SerializeField] private float _groundFriction = 15;
    [SerializeField] private float _airFriction = 0.6f;
    [SerializeField] private float _airMovementFactor = 1f;
    [SerializeField] private float _standingHeight = 1.8f;
    [SerializeField] private float _crouchedHeight = 0.4f;
    [SerializeField] private float _baseHeadOffset = 0.8f;
    [SerializeField] private float _crouchedHeadOffset = 0.5f;

    [Header("Components")]
    [SerializeField] private MovementCamera _camController;
    [SerializeField] private MovementInput _input;

    [Header("Events")]
    [Tooltip("Called when jumping.")]
    public UnityEvent OnJump = new();

    [Tooltip("Called when landing, where parameter T0 is float velocity.")]
    public UnityEvent<float> OnLand = new();

    private bool _jumping;
    private bool _climbing;
    private bool _wasGrounded;
    private bool _crouched;
    private bool _sprinting;
    private bool _initialized;

    private Vector3 _lastVelocity;
    private Vector3 _curVelocity;

    private float _moveSpeed;

    private CharacterController _char;
    private MovementState _state;

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        if (!_initialized)
            return;

        MainUpdate();
    }

    // Set references and load defaults
    void Initialize()
    {
        _char = GetComponent<CharacterController>();

        _camController.MouseLook.TrackedBody = gameObject;
        _camController.MouseLook.LockCursor = true;
        _camController.IsFirstPerson = true;

        _initialized = true;
    }

    void MainUpdate()
    {
        _lastVelocity = _curVelocity;

        MovementInputData inputData = _input.PollInput();

        UpdateMovement(inputData);
        UpdateJumpAndGravity(inputData);

        var cameraOptions = new MovementCameraOptions
        {
            didSwitchCamera = inputData.didSwitchCamera,
            lookInput = inputData.lookInput,
            doBob = IsGrounded && _char.velocity.magnitude > 0.2f,
            moveSpeed = _moveSpeed
        };

        _camController.UpdateSystems(cameraOptions);
    }

    void UpdateMovement(MovementInputData input)
    {
        var pressingMoveKeys = input.moveInput.magnitude > 0;

        var desiredMoveSpeed = _crouched ? _crouchedWalkSpeed : _sprinting ? _sprintSpeed : _walkSpeed;
        _moveSpeed = Mathf.Lerp(_moveSpeed, desiredMoveSpeed, Time.deltaTime * 10);

        Vector3 desiredVelocity;
        if (pressingMoveKeys)
        {
            var translatedDirection = Quaternion.Euler(0, transform.eulerAngles.y, 0) * input.moveInput;
            desiredVelocity = translatedDirection.normalized * _moveSpeed;
        }
        else
        {
            desiredVelocity = Vector3.zero;
        }

        Vector3 velocity = CalculateVelocity(_lastVelocity, desiredVelocity);

        _curVelocity = new Vector3(velocity.x, _curVelocity.y, velocity.z);

        if (!_crouched && input.isCrouching)
        {
            _crouched = true;
        }

        // Releasing LeftControl uncrouches if it's possible 
        // (if there's enough room above the player's head to accommodate their standing size)
        if (_crouched && !input.isCrouching && CanUnCrouch())
        {
            _crouched = false;
        }

        if (_crouched)
            _camController.HeadOffset = _crouchedHeadOffset;
        else
            _camController.HeadOffset = _baseHeadOffset;

        _char.height = _crouched ? _crouchedHeight : _standingHeight;

        _sprinting = input.isSprinting;

        Debug.DrawRay(transform.position, _curVelocity);

        _char.Move(_curVelocity * Time.deltaTime);
    }

    // Messy math for smooth movement
    Vector3 CalculateVelocity(Vector3 lastVelocity, Vector3 targetVelocity)
    {
        Vector3 velocity;
        var friction = IsGrounded ? _groundFriction : _airFriction;

        if (!IsGrounded)
        {
            var airFactor = _airMovementFactor;

            velocity = Vector3.Lerp(lastVelocity, targetVelocity * airFactor, Time.deltaTime * friction);
        }
        else
        {
            velocity = Vector3.Lerp(lastVelocity, targetVelocity, Time.deltaTime * friction);
        }

        return velocity;
    }

    bool CanUnCrouch()
    {
        int mask = LayerMask.NameToLayer("Player");
        mask = 1 << mask;
        mask = ~mask;

        Vector3 halfExtents = new Vector3(_char.radius, _standingHeight / 4, _char.radius);

        return !Physics.BoxCast(transform.position, halfExtents,
            Vector3.up, Quaternion.identity, _standingHeight, mask, QueryTriggerInteraction.Ignore);
    }

    bool HasSpaceToJump()
    {
        // Check whether there is space to jump.
        int mask = LayerMask.NameToLayer("Player");
        mask = 1 << mask;
        mask = ~mask;

        Vector3 halfExtents = new Vector3(_char.radius, _standingHeight / 8, _char.radius);

        return !Physics.BoxCast(transform.position, halfExtents,
            Vector3.up, Quaternion.identity, _standingHeight / 2, mask, QueryTriggerInteraction.Ignore);
    }

    void UpdateJumpAndGravity(MovementInputData inputData)
    {
        var gravity = Gravity;

        if (inputData.isJumping && IsGrounded)
        {
            TryJump();
        }

        if (!_wasGrounded && IsGrounded)
        {
            _jumping = false;
            if (_char.velocity.y < 0)
            {
                _curVelocity = new Vector3(_curVelocity.x, _char.velocity.y, _curVelocity.z);

                // Only invoke event if we actually landed with meaningful velocity.
                if (_char.velocity.y < gravity * Time.deltaTime)
                    OnLand.Invoke(_curVelocity.y);
            }
        }
        
        if (_jumping && (_curVelocity.y <= 0 || !HasSpaceToJump()))
        {
            _curVelocity.y = gravity * Time.deltaTime;
            _jumping = false;
        }

        // add gravity when falling
        if (!IsGrounded)
        {
            _curVelocity.y += gravity * Time.deltaTime;
        }

        // clamp Y velocity at terminal velocity
        if (_curVelocity.y < _terminalVelocity)
        {
            _curVelocity.y = _terminalVelocity;
        }

        _state = _climbing ? MovementState.Climbing : IsGrounded ? MovementState.Grounded : MovementState.Falling;
        _wasGrounded = IsGrounded;
    }

    void TryJump()
    {
        if (!IsGrounded)
            return;

        if (!HasSpaceToJump())
            return;

        _jumping = true;

        float jumpVelocity = Mathf.Sqrt(_jumpHeight * -2f * Gravity);

        if (!_crouched)
        {
            _curVelocity.y = jumpVelocity;
        }

        else
        {
            // Jump a little lower if crouched
            _curVelocity.y = jumpVelocity * _crouchJumpFactor;
        }

        OnJump.Invoke();
    }
}


