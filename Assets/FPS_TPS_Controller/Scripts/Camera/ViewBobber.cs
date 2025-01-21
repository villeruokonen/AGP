using UnityEngine;

public class ViewBobber : MonoBehaviour
{
    [SerializeField] private float _viewBobbingIntensity = 1;

    private float _viewBobSine;
    private float _stepSine;
    private float _stepTime;
    private int _stepSign;
    private int _lastSign;

    private float _targetValue;

    public void StartBobbing()
    {

    }

    public void StopBobbing()
    {

    }
}
