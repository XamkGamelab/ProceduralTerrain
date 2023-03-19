using UnityEngine;

public class MathExtensions {

    /// <summary>
    /// Clamp quaternion's x between min and max angles.
    /// </summary>
    /// <param name="_q">Quaternion to clamp.</param>
    /// <param name="_minimumX">Minimum angle.</param>
    /// <param name="_maximumX">Maximum angle.</param>
    /// <returns>Clamped quaternion.</returns>
    public static Quaternion ClampRotationAroundXAxis(Quaternion _q, float _minimumX, float _maximumX)
    {
        _q.x /= _q.w;
        _q.y /= _q.w;
        _q.z /= _q.w;
        _q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(_q.x);
        angleX = Mathf.Clamp(angleX, _minimumX, _maximumX);
        _q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return _q;
    }
}
