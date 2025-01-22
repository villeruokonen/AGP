using System.Collections;
using UnityEngine;

public struct MovementCameraOptions
{
    public bool didSwitchCamera;
    public bool doBob;
    public float moveSpeed;
}

public class MovementCamera : MonoBehaviour
{
    public float HeadOffset
    {
        get => _mouseLook.HeadOffset;
        set => _mouseLook.HeadOffset = value;
    }
    public bool IsFirstPerson
    {
        get => _firstPerson;
        set 
        {
            _firstPerson = value;
            _mouseLook.IsFirstPerson = value;
        }
    }

    public MouseLook MouseLook => _mouseLook;
    public Camera Camera => _camera;
    public ViewBobber ViewBobber => _viewBobber;

    [SerializeField] private MouseLook _mouseLook;
    [SerializeField] private Camera _camera;
    [SerializeField] private ViewBobber _viewBobber;

    private bool _firstPerson;

    private void Start()
    {
        // Camera should not be parented 
        // as it needs to move freely.
        if (_camera.transform.parent)
            _camera.transform.SetParent(null);
    }

    public void UpdateSystems(MovementCameraOptions cameraOptions)
    {
        if (cameraOptions.didSwitchCamera)
        {
            IsFirstPerson = !IsFirstPerson;
        }

        _mouseLook.CameraUpdate();

        if (cameraOptions.doBob)
        {
            _viewBobber.BobSpeed = cameraOptions.moveSpeed;
            _viewBobber.StartBobbing();
        }
        else
        {
            _viewBobber.StopBobbing();
        }

        _camera.transform.SetPositionAndRotation(
            _mouseLook.CameraPosition + Vector3.up * _viewBobber.ViewBobValue, 
            _mouseLook.CameraRotation);
    }

}
