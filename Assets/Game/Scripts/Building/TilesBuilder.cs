using Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TilesBuilder : MonoBehaviour
{
    protected GameTileContentFactory _contentFactory;
    
    protected GameBoard _gameBoard;

    protected bool _isEnabled;

    /// <summary>
    /// Направление строительства
    /// </summary>
    public Direction DirectionBuilding { get; private set; }

    /// <summary>
    /// Инициализация
    /// </summary>
    /// <param name="contentFactory">Фабрика контента</param>
    /// <param name="gameBoard">Игровое поле</param>
    public void Initialize(GameTileContentFactory contentFactory, GameBoard gameBoard)
    {
        _contentFactory = contentFactory;
        _gameBoard = gameBoard;
    }

    /// <summary>
    /// Очистка поля
    /// </summary>
    public void OnClear()
    {
        _gameBoard.Clear();
    }

    /// <summary>
    /// Активация строителя
    /// </summary>
    public void Enable()
    {
        _isEnabled = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Деактивация строителя
    /// </summary>
    public void Disable()
    {
        _isEnabled = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Автоматическое заполнение поля
    /// </summary>
    /// <exception cref="System.Exception"></exception>
    public void OnAutoBuilding()
    {
        bool success = true;
        var countAttempts = 20;

        do
        {
            _gameBoard.Clear();
            var boardCoordinates = new List<Vector2Int>(100);
            foreach (var i in 0..9)
            {
                foreach (var j in 0..9)
                {
                    boardCoordinates.Add(new Vector2Int(i, j));
                }
            }
            for (int i = 4; i > 0 && success; i--)
            {
                var type = (GameTileContentType)i;
                var countShips = GetQuantityLimit(type);
                for (var j = 0; j < countShips && success; j++)
                {
                    success = BuildShips(ref boardCoordinates, type);
                }
            }
        }
        while (!success && --countAttempts > 0);

        if(!success)
        {
            throw new System.Exception("Расстановка не удалась!");
        }
    }

    /// <summary>
    /// Получение лимитов на строительтво корабля по типу
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    protected int GetQuantityLimit(GameTileContentType type)
    {
        return type switch
        {
            GameTileContentType.SingleDeckShip => 4,
            GameTileContentType.TwoDeckShip => 3,
            GameTileContentType.ThreeDeckShip => 2,
            GameTileContentType.FourDeckShip => 1,
            _ => 0
        };
    }

    /// <summary>
    /// Смена направления строительства
    /// </summary>
    protected void OnChangeDirectionBuilding()
    {
        DirectionBuilding = DirectionBuilding.ChangeDirection();
    }

    private bool BuildShips(ref List<Vector2Int> boardCoordinates, GameTileContentType type)
    {
        bool avaliable;
        var countAttemptsBuilding = 30;
        var pendingTile = _contentFactory.Get(type);
        do
        {
            var countChangeDirection = GetCountChangeDirectionByType(type);
            var coordinates = boardCoordinates.PickRandom();
            do
            {
                OnChangeDirectionBuilding();
                pendingTile.Direction = DirectionBuilding;
                pendingTile.transform.localRotation = DirectionBuilding.GetRotation();

                avaliable = _gameBoard.CheckPossibilityBuilding(coordinates, pendingTile);
                if (avaliable)
                {
                    var tiles = _gameBoard.GetTiles(coordinates, pendingTile).ToArray();
                    _gameBoard.Build(tiles, pendingTile);
                    var removeCordinates = _gameBoard.GetCoordinatesAroundContent(pendingTile);
                    boardCoordinates.RemoveAll(c => removeCordinates.Contains(c));
                }
            }
            while (!avaliable && --countChangeDirection > 0);
        }
        while (!avaliable && --countAttemptsBuilding > 0);

        if (!avaliable)
        {
            pendingTile.Recycle();
            return false;
        }
        return true;
    }

    private int GetCountChangeDirectionByType(GameTileContentType type)
        => type == GameTileContentType.SingleDeckShip ? 1 : 4;
}
