﻿using System;
using Common;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Loading
{
    public class ClearGameOperation : ILoadingOperation
    {
        public string Description => "Очистка...";

        private readonly ICleanUp _gameCleanUp;

        public ClearGameOperation(ICleanUp gameCleanUp)
        {
            _gameCleanUp = gameCleanUp;
        }

        public async UniTask Load(Action<float> onProgress)
        {
            onProgress?.Invoke(0.2f);
            _gameCleanUp.Cleanup();
            onProgress?.Invoke(0.6f);

            var loadOp = SceneManager.LoadSceneAsync(Constants.Scenes.MAIN_MENU, LoadSceneMode.Additive);
            while (loadOp.isDone == false)
            {
                await UniTask.Delay(1);
            }
            onProgress?.Invoke(0.75f);

            var unloadOp = SceneManager.UnloadSceneAsync(_gameCleanUp.SceneName);
            while (unloadOp.isDone == false)
            {
                await UniTask.Delay(1);
            }
            onProgress?.Invoke(1f);
        }
    }
}