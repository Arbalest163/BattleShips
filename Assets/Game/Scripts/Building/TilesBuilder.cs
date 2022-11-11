using Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TilesBuilder : MonoBehaviour
{
    public Direction DirectionBuilding { get; private set; }

    protected GameTileContentFactory _contentFactory;
    
    protected GameBoard _gameBoard;

    protected bool _isEnabled;

    protected GameTileContent _pendingTile;

    public void Initialize(GameTileContentFactory contentFactory, GameBoard gameBoard)
    {
        _contentFactory = contentFactory;
        _gameBoard = gameBoard;
    }
    public void Enable()
    {
        _isEnabled = true;
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        _isEnabled = false;
        gameObject.SetActive(false);
    }

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
    protected void OnChangeDirectionBuilding()
    {
        DirectionBuilding = DirectionBuilding.ChangeDirection();
    }

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

    private bool BuildShips(ref List<Vector2Int> boardCoordinates, GameTileContentType type)
    {
        bool avaliable;
        var countAttemptsBuilding = 30;
        do
        {
            var countChangeDirection = type == GameTileContentType.SingleDeckShip ? 1 : 4;
            var coordinates = boardCoordinates.PickRandom();
            do
            {
                OnChangeDirectionBuilding();
                _pendingTile = _contentFactory.Get(type);
                _pendingTile.Direction = DirectionBuilding;
                _pendingTile.transform.localRotation = DirectionBuilding.GetRotation();

                avaliable = _gameBoard.CheckPossibilityBuilding(coordinates, _pendingTile);
                if (avaliable)
                {
                    var tiles = _gameBoard.GetTiles(coordinates, _pendingTile).ToArray();
                    _gameBoard.Build(tiles, _pendingTile);
                    boardCoordinates.Remove(coordinates);
                }
                else
                {
                    Destroy(_pendingTile.gameObject);
                }
                _pendingTile = null;
            }
            while (!avaliable && --countChangeDirection > 0);
        }
        while (!avaliable && --countAttemptsBuilding > 0);

        if (!avaliable)
        {
            return false;
        }
        return true;
    }

    public void OnClear()
    {
        _gameBoard.Clear();
    }
}
