using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CommonIEnumerator
{
    private class FloatComparer : IEqualityComparer<float>
    {
        public bool Equals(float x, float y)
        {
            return x == y;
        }

        public int GetHashCode(float obj)
        {
            return obj.GetHashCode();
        }
    }

    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

    private static readonly Dictionary<float, WaitForSeconds> dicWaitForSeconds = new Dictionary<float, WaitForSeconds>(new FloatComparer());
    private static readonly Dictionary<float, WaitForSecondsRealtime> dicWaitForSecondsRealtime = new Dictionary<float, WaitForSecondsRealtime>(new FloatComparer());

    public static WaitForSeconds WaitForSecond(float second)
    {
        if(dicWaitForSeconds.TryGetValue(second, out WaitForSeconds waitForSeconds) == false)
        {
            dicWaitForSeconds.Add(second, waitForSeconds = new WaitForSeconds(second));
        }
        return waitForSeconds;
    }

    public static WaitForSecondsRealtime WaitForSecondsRealtime(float sec)
    {
        if (dicWaitForSecondsRealtime.TryGetValue(sec, out WaitForSecondsRealtime waitForSecondsRealtime) == false)
        {
            dicWaitForSecondsRealtime.Add(sec, waitForSecondsRealtime = new WaitForSecondsRealtime(sec));
        }
        return waitForSecondsRealtime;
    }

    public static IEnumerator IETimeout(Action onStart, Action onFinish, float waitTime)
    {
        onStart?.Invoke();
        yield return WaitForSecond(waitTime);
        onFinish?.Invoke();
    }

    public static IEnumerator IEWaitUntil(Func<bool> predicate, Action onFinish)
    {
        yield return new WaitUntil(predicate);
        onFinish?.Invoke();
    }
}