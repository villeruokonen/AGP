using UnityEngine;

public class CameraPositionValidator : MonoBehaviour
{
    [Tooltip("The \"thickness\" of the normal offset if something was in the way of the camera")]
    [SerializeField] private float _normalOffset = 0.05f;

    [SerializeField] private float _checkRadius = 0.2f;

    /// <summary>
    /// Corrects a third-person camera position, based on an object that should remain visible.
    /// (Walls may not be in the way, etc.)
    /// </summary>
    /// <param name="desiredPosition">The target camera position</param>
    /// <param name="targetObject">The gameobject to look for</param>
    /// <returns></returns>
    public Vector3 ValidateCameraPosition(Vector3 desiredPosition, GameObject targetObject)
    {
        var pos = targetObject.transform.position;
        var direction = desiredPosition - pos;

        float distance = Vector3.Distance(desiredPosition, pos);
        float radius = _checkRadius;

        int mask = ~(1 << targetObject.layer);

        var cast = Physics.SphereCast(pos, radius, direction, out var hitInfo, distance, mask, QueryTriggerInteraction.Ignore);
        var cols = Physics.OverlapSphere(pos, radius, mask, QueryTriggerInteraction.Ignore);

        if (cols.Length == 0 && !cast)
            return desiredPosition;

        Vector3 newPos = hitInfo.point + hitInfo.normal * _normalOffset;
        return newPos;
    }
}
