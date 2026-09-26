using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Domain.Player;

namespace Systems
{
    public class SceneManager
    {
        public Scene CurrentScene { get => currentScene; }

        private Scene currentScene;

        public void Init()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {

        }

        public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action callback = null)
        {
            currentScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            Managers.StartCoroutineManager(LoadSceneCoroutine, (sceneName, mode, callback));
        }

        private IEnumerator LoadSceneCoroutine((string sceneName, LoadSceneMode mode, Action callback) args)
        {
            AsyncOperation asyncOper = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(args.sceneName, args.mode);
            while (!asyncOper.isDone)
            {
                yield return null;
            }

            args.callback?.Invoke();
        }

        public void UnloadScene(string sceneName, Action callback = null)
        {
            Managers.StartCoroutineManager(UnloadSceneCoroutine, (sceneName, callback));
        }

        private IEnumerator UnloadSceneCoroutine((string sceneName, Action callback) args)
        {
            AsyncOperation asyncOper = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(args.sceneName);
            while (!asyncOper.isDone)
            {
                yield return null;
            }
            
            args.callback?.Invoke();
        }
    }
}