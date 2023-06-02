using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSlider : MonoBehaviour
{
    [SerializeField]
    private MonsterSpawn spawner;

    private Slider slider;

    private Coroutine waveCoroutine;

    public static int WaveCount { get; private set; } = 0;

    private void OnEnable()
    {
        ClearWaveTrigger();

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
        StopCoroutine(waveCoroutine);
        waveCoroutine = null;
    }
    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        slider.interactable = false;

        waveCoroutine = StartCoroutine(FilledSlider());
    }

    public IEnumerator FilledSlider()
    {
        float duration = 10.0f;
        float elapsed = 0.0f;
        float startValue = slider.value;
        float endValue = 1.0f;

        while (elapsed < duration)
        {
            if (PlayerController.CurrentPlayerState == PlayerState.Moving)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float value = Mathf.Lerp(startValue, endValue, t);
                slider.value = value;
            }
            yield return null;
        }

        slider.value = endValue;
    }

    public void OnSliderValueChanged(float value)
    {
        float errorRange = 0.001f;

        if (Mathf.Abs(value - 0.2f) <= errorRange && WaveCount == 0)
        {
            WaveCount++;
            spawner.StartSpawn();
        }
        else if (Mathf.Abs(value - 0.4f) <= errorRange && WaveCount == 1)
        {
            WaveCount++;
            spawner.StartSpawn();
        }
        else if (Mathf.Abs(value - 0.6f) <= errorRange && WaveCount == 2)
        {
            WaveCount++;
            spawner.StartSpawn();
        }
        else if (Mathf.Abs(value - 0.8f) <= errorRange && WaveCount == 3)
        {
            WaveCount++;
            spawner.StartSpawn();
        }
        else if (Mathf.Abs(value - 1.0f) <= errorRange && WaveCount == 4)
        {
            WaveCount++;    // WaveCount = 5
            spawner.StartSpawn();

            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }
    }

    private void ClearWaveTrigger()
    {
        WaveCount = 0;
    }
}
