using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private enum MovementState
    {
        Grounded,
        Falling,
        Climbing
    }

    [Header("Options")]
    [SerializeField] private bool _enableViewBobbing = true;

    private bool IsGrounded => _char.isGrounded;

    [Header("Values")]
    [SerializeField] private float _gravity = -25.8f;
    [SerializeField] private float _terminalVelocity = -120f;
    [SerializeField] private float _walkSpeed = 10;
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
    [SerializeField] private float _fallSoundVelocityThreshold = -10f;

    [Header("Components")]
    [SerializeField] private MovementSound _sound;
    [SerializeField] private MovementCamera _camController;
    [SerializeField] private MovementInput _input;

    private bool _jumping;
    private bool _climbing;
    private bool _wasGrounded;
    private bool _crouched;
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

        _camController.UpdateSystems(inputData);
    }

    void UpdateMovement(MovementInputData input)
    {
        var pressingMoveKeys = input.inputDirection.magnitude > 0;

        var desiredMoveSpeed = _crouched ? _crouchedWalkSpeed : _walkSpeed;
        _moveSpeed = Mathf.Lerp(_moveSpeed, desiredMoveSpeed, Time.deltaTime * 10);

        Vector3 desiredVelocity;
        if (pressingMoveKeys)
        {
            var translatedDirection = Quaternion.Euler(0, transform.eulerAngles.y, 0) * input.inputDirection;
            desiredVelocity = translatedDirection.normalized * _moveSpeed;
        }
        else
            desiredVelocity = Vector3.zero;

        Vector3 velocity = CalculateVelocity(_lastVelocity, desiredVelocity);

        _curVelocity = new Vector3(velocity.x, _curVelocity.y, velocity.z);
        _curVelocity = ClampHorizontalVelocity(_curVelocity);

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

        _char.Move(_curVelocity * Time.deltaTime);
    }

    Vector3 ClampHorizontalVelocity(Vector3 velocity)
    {
        Vector3 vel = velocity;

        vel.x = Mathf.Clamp(vel.x, -_walkSpeed, _walkSpeed);
        vel.z = Mathf.Clamp(vel.z, -_walkSpeed, _walkSpeed);

        return vel;
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
            }
            
            if (_curVelocity.y < _fallSoundVelocityThreshold)
            {
                _sound.PlayLandingSound();
            }
        }

        var gravity = _gravity;

        // head bonk => stop and fall instantly with a bit of downwards gravity
        if (_jumping && !HasSpaceToJump())
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

        if (_curVelocity.y < 0)
        {
            _jumping = false;
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

        if (!_crouched)
        {
            _curVelocity.y = _jumpHeight;
        }
        else
        {
            // Jump a little lower if crouched
            _curVelocity.y = _jumpHeight * _crouchJumpFactor;
        }

        _sound.PlayJumpSound();
    }

    /* void UpdateViewBobbingAndFootsteps()
    {
        // Don't waste time doing math if it's not used in either of these effects
        if (!_enableViewBobbing && !_playFootSteps)
            return;
        
        // Don't bob the view or play footstep sounds if we're not moving enough 
        // (eg. running against or very acutely along a wall)
        bool actuallyMoving = Vector3.Distance(_curPos, _lastPos) > 0.002f;

        if (!actuallyMoving)
            return;
        
        _lastPos = _char.transform.position;

        if (_enableViewBobbing)
        {
            if (_pressingMoveKeys && IsGrounded)
            {
                _viewBobSine = Mathf.Lerp(_viewBobSine, Mathf.Sin(Time.time * _curSpeed) * _viewBobbingIntensity, Time.deltaTime);
            }
            else
            {
                _viewBobSine = Mathf.Lerp(_viewBobSine, 0, Time.deltaTime * 15);
            }
            _mouseLook.ViewBobValue = _viewBobSine;
        }

        if (_playFootSteps)
        {
            // Don't bother continuing if we haven't been grounded for long enough, or not trying to move
            if (!IsGrounded || !_pressingMoveKeys) { return; }
            _stepTime += Time.deltaTime;

            _stepSine = Mathf.Lerp(_stepSine, (Mathf.Sin(_stepTime * _curSpeed) * _viewBobbingIntensity), Time.deltaTime);
            // Check whether step sine is negative or positive and set sign accordingly (see further)
            _stepSign = _stepSine < 0 ? -1 : 1;

            // Hacky solution to footsteps:
            // If the sine has passed 0
            // (into negative if it was positive, into positive if it was negative),
            // only then play a footstep sound, and update the last sign of the step sine function
            if (_stepSign == _lastSign) { return; }
            _lastSign = _stepSign;

            float multiplier = _curSpeed / 3.5f;
            var randPitch = Random.Range(0.85f, 1f);
            var volume = Mathf.Clamp(Random.Range(0.8f, 1) * multiplier / 2, 0.2f, 1);
            PlayMovementSound(GetFootstepClip(), volume, transform.position, randPitch);
        }
    } */
}


