using Common;
using Cysharp.Threading.Tasks;
using Extensions;
using System;
using UnityEngine.SceneManagement;

namespace Loading
{
    public class PrepareGameLoadingOperation : ILoadingOperation
    {
        public string Description => "Загрузка ...";

        public async UniTask Load(Action<float> onProgress)
        {
            onProgress?.Invoke(0.5f);
            var loadOp = SceneManager.LoadSceneAsync(Constants.Scenes.PREPARE_GAME,
                LoadSceneMode.Single);
            while (loadOp.isDone == false)
            {
                await UniTask.Delay(1);
            }
            onProgress?.Invoke(0.7f);

            var scene = SceneManager.GetSceneByName(Constants.Scenes.PREPARE_GAME);
            var prepareGame = scene.GetRoot<PrepareGame>();
            var environment = await ProjectContext.Instance.AssetProvider.LoadSceneAdditive("Sand");
            onProgress?.Invoke(0.85f);
            prepareGame.Init(environment);
            prepareGame.ClearBoard();
            onProgress?.Invoke(1.0f);
        }
    }
}
