using UnityEngine;

/// <summary>
/// Lerp and easing methdods
/// https://www.febucci.com/2018/08/easing-functions/
/// </summary>
public static class Interpolation
{
    public static float Lerp(float startValue, float endValue, float percent)
    {
        return (startValue + (endValue - startValue) * percent);
    }

    public static float EaseIn(float t)
    {
        return t * t;
    }
    public static float EaseOut(float t)
    {
        return Flip(Mathf.Sqrt(Flip(t)));
    }

    public static float EaseInOut(float t)
    {
        return Lerp(EaseIn(t), EaseOut(t), t);
    }

    public static float Flip(float x)
    {
        return 1 - x;
    }
}
