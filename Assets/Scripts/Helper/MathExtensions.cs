using UnityEngine;

public class MathExtensions {

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
