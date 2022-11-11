using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Common;
using UnityEngine.ResourceManagement.ResourceProviders;
using Core.UI;
using System.Threading.Tasks;
using Loading;

public class PrepareGame : MonoBehaviour, ICleanUp
{
    [SerializeField] private GameBoard _mainBoard;

    [SerializeField] private PrepareTilesBuilder _tilesBuilder;

    [SerializeField] private Camera _camera;

    [SerializeField] private Hud _hud;

    [SerializeField] private GameHint _hint;

    private CancellationTokenSource _prepareCancellation;

    [SerializeField] private GameTileContentFactory _contentFactory;

    private static PrepareGame _instance;

    private SceneInstance _environment;
    public string SceneName => Constants.Scenes.PREPARE_GAME;
    public IEnumerable<GameObjectFactory> Factories => new GameObjectFactory[] { _contentFactory };
    private void OnEnable()
    {
        _instance = this;
    }


    public void Init(SceneInstance environment)
    {
        _environment = environment;
        _hud.QuitGame += GoToMainMenu;
        _hud.NewGame += OnNewGameBtnClicked;
        _mainBoard.Initialize(_contentFactory);
        _tilesBuilder.Initialize(_contentFactory, _camera, _mainBoard);
    }

    public void ClearBoard()
    {
        Cleanup();
        _tilesBuilder.Enable();

        try
        {
            _prepareCancellation?.Dispose();
            _prepareCancellation = new CancellationTokenSource();
        }
        catch (TaskCanceledException ex) { Debug.Log(ex.Message); }
    }

    public void Cleanup()
    {
        _tilesBuilder.Disable();
        _prepareCancellation?.Cancel();
        _prepareCancellation?.Dispose();
        _mainBoard.Clear();
    }

    private async void GoToMainMenu()
    {
        var operations = new Queue<ILoadingOperation>();
        operations.Enqueue(new ClearGameOperation(this));
        await ProjectContext.Instance.AssetProvider.UnloadAdditiveScene(_environment);
        await ProjectContext.Instance.LoadingScreenProvider.LoadAndDestroy(operations);
    }

    private async void OnNewGameBtnClicked()
    {
        if(!_tilesBuilder.CheckFull())
        {
            _hint.TryShow();
            return;
        }
        //TODO Ножно сохранять поле и потом загружать его
        var boardData = _mainBoard.GenerateBordData();
        var operations = new Queue<ILoadingOperation>();
        operations.Enqueue(new MainGameLoadingOperation(this, boardData));
        await ProjectContext.Instance.AssetProvider.UnloadAdditiveScene(_environment);
        await ProjectContext.Instance.LoadingScreenProvider.LoadAndDestroy(operations);
    }

}
