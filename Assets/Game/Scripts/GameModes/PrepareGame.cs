using System.Collections.Generic;
using UnityEngine;
using Common;
using UnityEngine.ResourceManagement.ResourceProviders;
using Core.UI;
using Loading;

public class PrepareGame : MonoBehaviour, ICleanUp
{
    [SerializeField] private GameBoard _mainBoard;

    [SerializeField] private PrepareTilesBuilder _tilesBuilder;

    [SerializeField] private Camera _camera;

    [SerializeField] private Hud _hud;

    [SerializeField] private GameHint _hint;

    [SerializeField] private GameTileContentFactory _contentFactory;

    private SceneInstance _environment;
    public string SceneName => Constants.Scenes.PREPARE_GAME;
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
    }

    public void Cleanup()
    {
        _tilesBuilder.Disable();
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
        var boardData = _mainBoard.GenerateBordData();
        var operations = new Queue<ILoadingOperation>();
        operations.Enqueue(new MainGameLoadingOperation(boardData));
        await ProjectContext.Instance.AssetProvider.UnloadAdditiveScene(_environment);
        await ProjectContext.Instance.LoadingScreenProvider.LoadAndDestroy(operations);
    }

}
