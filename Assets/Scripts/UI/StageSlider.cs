using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSlider : MonoBehaviour
{
    [SerializeField]
    private MonsterSpawner spawner;

    private Slider slider;

    public Coroutine waveCoroutine;

    private void OnEnable()
    {
        if (slider != null)
        {
            slider.value = 0.0f;
        }

        if (waveCoroutine == null)
        {
            waveCoroutine = StartCoroutine(FilledSlider());
        }
    }
    private void OnDisable()
    {
        if (waveCoroutine != null)
            StopCoroutine(waveCoroutine);

        waveCoroutine = null;
    }
    private void Awake()
    {
        slider = GetComponent<Slider>();

        waveCoroutine = StartCoroutine(FilledSlider());
    }

    public IEnumerator FilledSlider()
    {
        float duration = 2.0f;
        float elapsed = 0.0f;
        float startValue = slider.value;
        float endValue = 0.2f;

        while (elapsed < duration)
        {
            if (PlayerController.CurrentPlayerState == PlayerState.Moving)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float value = Mathf.Lerp(startValue, endValue, t);
                slider.value = value;

                if (Mathf.Approximately(value, endValue))
                {
                    startValue = endValue;
                    endValue += 0.2f;
                    elapsed = 0.0f;

                    if (Mathf.Approximately(value, 1.0f))
                        break;
                }
            }
            yield return null;
        }

        slider.value = endValue;

        StopCoroutine(waveCoroutine);
        waveCoroutine = null;
    }
}
