using System.Collections.Generic;
using UnityEngine;
using Common;
using Loading;
using UnityEngine.ResourceManagement.ResourceProviders;
using Core.UI;
using GameResult;

public class MainGame : MonoBehaviour, ICleanUp
{
    private Vector2Int _boardSize = new Vector2Int(10, 10);

    [SerializeField] private GameBoard _mainBoard;
    [SerializeField] private GameBoard _enemyBoard;

    [SerializeField] private TilesBuilder _tilesBuilder;

    [SerializeField] private Shooter _shooter;

    [SerializeField] private Hud _defenderHud;

    [SerializeField] private GameResultWindow _gameResultWindow;

    [SerializeField] private Camera _camera;

    private bool _gameInProcess;

    private SceneInstance _environment;

    [SerializeField] private GameTileContentFactory _mainContentFactory;
    [SerializeField] private GameTileContentFactory _enemyContentFactory;

    private static MainGame _instance;
    public string SceneName => Constants.Scenes.MAIN_GAME;
    public IEnumerable<GameObjectFactory> Factories => new GameObjectFactory[] { _mainContentFactory, _enemyContentFactory };
    private void OnEnable()
    {
        _instance = this;
        _gameInProcess = true;
    }

    public void Init(SceneInstance environment, BoardData boardData)
    {
        _environment = environment;
        _defenderHud.QuitGame += GoToMainMenu;
        _mainBoard.Initialize(_mainContentFactory, 7f);
        _mainBoard.LoadBoardData(boardData);
        _enemyBoard.Initialize(_enemyContentFactory, -7f);
        _tilesBuilder.Initialize(_enemyContentFactory, _enemyBoard);
        _tilesBuilder.OnAutoBuilding();
        _enemyBoard.HideShips();
        _tilesBuilder.Disable();
        _shooter.Initialize(_camera, _mainBoard, _enemyBoard);
    }

    public void Cleanup()
    {
        _tilesBuilder.Disable();
        _mainBoard.Clear();
        _enemyBoard.Clear();
    }

    private async void GoToMainMenu()
    {
        var operations = new Queue<ILoadingOperation>();
        operations.Enqueue(new ClearGameOperation(this));
        await ProjectContext.Instance.AssetProvider.UnloadAdditiveScene(_environment);
        await ProjectContext.Instance.LoadingScreenProvider.LoadAndDestroy(operations);
    }

    private async void Update()
    {
        if (_gameInProcess)
        {
            if (CheckEndGame(out var result))
            {
                _shooter.Disable();
                _gameInProcess = false;
                _enemyBoard.ShowShips();
                await ResultPopup.Instance.AwaitForDecision(GetMessageStatus(result.Value));
                _gameResultWindow.Show(result.Value, OnNewGame, GoToMainMenu);
            }
        }
        Physics.SyncTransforms();
    }

    private async void OnNewGame()
    {
        var operations = new Queue<ILoadingOperation>();
        operations.Enqueue(new PrepareGameLoadingOperation());
        await ProjectContext.Instance.LoadingScreenProvider.LoadAndDestroy(operations);
    }

    private bool CheckEndGame(out GameResultType? result)
    {
        result = null;
        if (!_instance._mainBoard.CheckForShips())
        {
            result = GameResultType.Defeat;
            return true;
        }
        if (!_instance._enemyBoard.CheckForShips())
        {
            result = GameResultType.Victory;
            return true;
        }
        return false;
    }

    private string GetMessageStatus(GameResultType gameResultType)
    {
        return gameResultType switch
        {
            GameResultType.Victory => "Вы победили!",
            _ => "Вы проиграли!"
        };
    }
}
