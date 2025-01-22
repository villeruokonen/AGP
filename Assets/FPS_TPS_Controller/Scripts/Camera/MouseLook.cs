using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public bool IsFirstPerson
    {
        get => _firstPerson;
        set => _firstPerson = value;
    }
    
    public Vector3 CameraPosition => _cameraPosition;
    public Quaternion CameraRotation => _cameraRotation;

    [Header("General Settings")]
    public LayerMask DrawInFirstPerson;
    public float Sensitivity = 5.0f;
    public GameObject TrackedBody;

    [Header("FPS Camera Settings")]
    public bool LockCursor;
    public float HeadOffset;

    [Header("TPS Camera Settings")]
    public Vector3 DesiredCameraPositionOffset = new Vector3(0.0f, 2.0f, -2.5f);
    public Vector3 CameraAngleOffset = new Vector3(0.0f, 0.0f, 0.0f);
    public float CameraFollowSpeed = 15;
    public float MinPitch = -30.0f;
    public float MaxPitch = 30.0f;

    [Header("Components")]
    [SerializeField] private CameraPositionValidator _positionValidator;

    private float angleX = 0.0f;

    private Vector3 _cameraPosition;
    private Quaternion _cameraRotation;

    private bool _firstPerson;

    public void CameraUpdate()
    {
        if (!TrackedBody)
        {
            Debug.LogWarning($"MouseLook {this} has no Tracked Body and will not run.");
            return;
        }

        if (LockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }

        float damping = _firstPerson ? 15 : CameraFollowSpeed;

        // Mouse X and Y input
        float mx, my;
        mx = Input.GetAxisRaw("Mouse X");
        my = Input.GetAxisRaw("Mouse Y");

        // Apply the initial rotation to the camera.
        Quaternion initialRotation = Quaternion.Euler(CameraAngleOffset);

        Vector3 camEuler = _cameraRotation.eulerAngles;

        angleX -= my * Sensitivity;

        // Clamp pitch between tps min and max pitch if third person, else straight up/down
        if (!_firstPerson)
            angleX = Mathf.Clamp(angleX, MinPitch, MaxPitch);
        else
            angleX = Mathf.Clamp(angleX, -90, 90);

        camEuler.y += mx * Sensitivity;
        Quaternion newRot = Quaternion.Euler(angleX, camEuler.y, 0.0f) *
          initialRotation;

        Vector3 forward = _cameraRotation * Vector3.forward;
        Vector3 right = _cameraRotation * Vector3.right;
        Vector3 up = _cameraRotation * Vector3.up;

        Vector3 targetPos = TrackedBody.transform.position;

        Vector3 desiredPosition = _firstPerson
            ?
                targetPos
                    + Vector3.up * HeadOffset
            :
                targetPos
                    + (forward * DesiredCameraPositionOffset.z)
                    + (right * DesiredCameraPositionOffset.x)
                    + (up * DesiredCameraPositionOffset.y);

        Vector3 position;
        if (_firstPerson)
        {
            position = desiredPosition;
        }
        else
        {
            desiredPosition = _positionValidator.ValidateCameraPosition(desiredPosition, TrackedBody);

            position = Vector3.Lerp(_cameraPosition, desiredPosition, Time.deltaTime * damping);
        }

        _cameraPosition = position;
        _cameraRotation = newRot;

        Quaternion newCharacterRot = Quaternion.Euler(TrackedBody.transform.rotation.x, camEuler.y, 0.0f) * initialRotation;

        if (!_firstPerson)
        {
            TrackedBody.transform.rotation = Quaternion.Lerp
            (TrackedBody.transform.rotation, newCharacterRot, Time.deltaTime * (damping / 3));
        }
        else
        {
            TrackedBody.transform.rotation = newCharacterRot;
        }
    }
}
