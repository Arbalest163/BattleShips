using System;
using Common;
using Cysharp.Threading.Tasks;
using Extensions;
using UnityEngine.SceneManagement;

namespace Loading
{
    public class MainGameLoadingOperation : ILoadingOperation
    {
        public string Description => "Загрузка игры...";

        private readonly ICleanUp _gameCleanUp;
        private readonly BoardData _boardData;

        public MainGameLoadingOperation(ICleanUp gameCleanUp, BoardData boardData)
        {
            _gameCleanUp = gameCleanUp;
            _boardData = boardData;
        }
        public async UniTask Load(Action<float> onProgress)
        {
            onProgress?.Invoke(0.2f);
            //foreach (var factory in _gameCleanUp.Factories)
            //{
            //    await factory.Unload();
            //}
            

            var loadOp = SceneManager.LoadSceneAsync(Constants.Scenes.MAIN_GAME,
                LoadSceneMode.Single);
            while (loadOp.isDone == false)
            {
                await UniTask.Delay(1);
            }
            onProgress?.Invoke(0.4f);

            var scene = SceneManager.GetSceneByName(Constants.Scenes.MAIN_GAME);
            var mainGame = scene.GetRoot<MainGame>();
            var environment = await ProjectContext.Instance.AssetProvider.LoadSceneAdditive("Sand");
            onProgress?.Invoke(0.6f);
            mainGame.Init(environment, _boardData);
            onProgress?.Invoke(0.8f);
            await UniTask.Delay(1);
            onProgress?.Invoke(1.0f);
        }
    }
}