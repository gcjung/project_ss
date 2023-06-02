using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
public class SceneLoadManager : Manager<SceneLoadManager>
{
    public override void Init()
    {
        if (Ininialized)
        {
            return;
        }

        Ininialized = true;
    }
    public override void InitializedFininsh()
    {
        //
    }

    public async void LoadSceneAsync(string sceneName)
    {
        Debug.Log(sceneName);

        SceneManager.LoadScene("Loading");

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            LoadingPanel.SliderValue = progress;

            await Task.Yield();
        }
    }
}
