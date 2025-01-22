using System.Collections;
using UnityEngine;

public class ViewBobber : MonoBehaviour
{
    public float ViewBobValue => _bobValue;
    public float BobSpeed 
    {
        get => _viewBobbingSpeed;
        set => _viewBobbingSpeed = value;
    }

    [SerializeField] private float _viewBobbingIntensity = 1;
    [SerializeField] private float _bobbingSpeedMultiplier = 1;
    [SerializeField] private float _smoothingSpeed = 1;
    private float _viewBobbingSpeed;

    private bool _isBobbing;

    private float _bobValue;
    private float _bobValueTarget;

    private float _bobTimer = 0;

    public void StartBobbing()
    {
        if (_isBobbing)
            return;

        _isBobbing = true;
    }

    public void StopBobbing()
    {
        if (!_isBobbing)
            return;

        _isBobbing = false;
    }

    private void Update()
    {
        _bobValue = Mathf.Lerp(_bobValue, _bobValueTarget, Time.deltaTime * _smoothingSpeed);

        if (!_isBobbing)
        {
            _bobValueTarget = 0;
            return;
        }

        float waveslice = Mathf.Sin(_bobTimer);
        _bobTimer += Time.deltaTime * _viewBobbingSpeed * _bobbingSpeedMultiplier;

        // Ensure timer stays in -2pi - 2pi range
        if (_bobTimer > Mathf.PI * 2)
        {
            _bobTimer -= Mathf.PI * 2;
        }

        _bobValueTarget = waveslice * _viewBobbingIntensity;
    }
}
