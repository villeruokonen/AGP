using System.Collections;
using UnityEngine;

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

    public void UpdateSystems(MovementInputData inputData)
    {
        if (inputData.didSwitchCamera)
        {
            IsFirstPerson = !IsFirstPerson;
        }

        _mouseLook.CameraUpdate();
    }
    
    IEnumerator CameraShakeCoroutine(float amplitude, float duration)
    {
        for (float i = duration * 1.02f; i > 0; i -= 0.02f)
        {
            Vector3 _camShakePos = _camera.transform.position + (Vector3)Random.insideUnitCircle / (1 - amplitude);
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, _camShakePos, i * 0.02f / (1 - amplitude));
            yield return new WaitForSeconds(0.02f);
        }
        
        yield break;
    }
}
