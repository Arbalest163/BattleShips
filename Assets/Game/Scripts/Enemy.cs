using Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy
{
    private List<Vector2Int> _possibleCoordinatesShots;
    private List<Vector2Int> _strategyCoordinatesShots;
    
    private Direction _directionShot;

    private Vector2Int _firstShootCoordinates;

    /// <summary>
    /// Координаты последнего выстрела
    /// </summary>
    public Vector2Int LastHitCoordinates;

    /// <summary>
    /// Режим добивания
    /// </summary>
    public bool FinishingMode { get; set; }

    /// <summary>
    /// Необходимость смены направления выстрелов
    /// </summary>
    public bool NeedChangeDirection { get; set; } = true;

    /// <summary>
    /// Инициализация
    /// </summary>
    /// <param name="possibleCoordinatesShots">Возможные координаты для встрелов</param>
    public void Initialize(List<Vector2Int> possibleCoordinatesShots)
    {
        _possibleCoordinatesShots = possibleCoordinatesShots;
        _directionShot = ChooiseFiringDirection();
        _strategyCoordinatesShots = new List<Vector2Int>();
    }

    /// <summary>
    /// Получить координаты для выстрела
    /// </summary>
    /// <returns></returns>
    public Vector2Int GetCoordinatesShot()
    {
        if (FinishingMode)
        {
            bool thereWasChangeDirection = false;
            int changesDirection = 4;
            do
            {
                if (NeedChangeDirection)
                {
                    ChangeDirectionToOpposite();
                    LastHitCoordinates = _firstShootCoordinates;
                    thereWasChangeDirection = true;
                }
                var coordinates = GetCoordinatesFinishingMode();
                if(coordinates != Vector2Int.zero)
                {
                    _possibleCoordinatesShots.Remove(coordinates);
                    return coordinates;
                }
                else
                {
                    if (thereWasChangeDirection)
                    {
                        _directionShot = _directionShot.ChangeDirection();
                        NeedChangeDirection = false;
                        thereWasChangeDirection = false;
                        continue;
                    }
                    NeedChangeDirection = true;
                }
            }
            while (--changesDirection > 0);
        }

        _firstShootCoordinates = GetCoordinates();
        return _firstShootCoordinates;
    }

    private Vector2Int GetCoordinates()
    {
        if (!_strategyCoordinatesShots.Any())
        {
            FillStrategyCoordinates();
        }
        if(_strategyCoordinatesShots.Any())
        {
            var coordinates = _strategyCoordinatesShots.PopRandom();
            _possibleCoordinatesShots.Remove(coordinates);
            return coordinates;
        }
        
        return _possibleCoordinatesShots.PopRandom();
    }

    private void FillStrategyCoordinates() //TODO Подумать над рефакторингом, созданием разных стратегий
    {
        _strategyCoordinatesShots.AddRange(_possibleCoordinatesShots.Where(c => c.x == c.y)); // главная диагональ
        _strategyCoordinatesShots.AddRange(_possibleCoordinatesShots.Where(c => c.x + c.y == 9)); // побочная диагональ
        if(_strategyCoordinatesShots.Any())
        {
            return;
        }
        _strategyCoordinatesShots.AddRange(_possibleCoordinatesShots.Where(c => c.x > c.y && c.x % 2 == 0)); // область над главной диагональю
        if (_strategyCoordinatesShots.Any())
        {
            return;
        }
        _strategyCoordinatesShots.AddRange(_possibleCoordinatesShots.Where(c => c.x < c.y && c.y % 2 == 0)); // область под главной диагональю
    }

    private void ChangeDirectionToOpposite()
    {
        _directionShot = _directionShot switch
        {
            Direction.East => Direction.West,
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.West => Direction.East,
            _ => Direction.North,
        };
    }

    private Direction ChooiseFiringDirection()
    {
        var directions = new List<Direction>
        {
            Direction.East,
            Direction.North,
            Direction.South,
            Direction.West,
        };

        return directions.PickRandom();
    }

    private Vector2Int GetCoordinatesFinishingMode()
    {
        return _directionShot switch
        {
            Direction.East => _possibleCoordinatesShots
                                .Where(coord => coord.x == LastHitCoordinates.x + 1 && coord.y == LastHitCoordinates.y)
                                .FirstOrDefault(),
            Direction.South => _possibleCoordinatesShots
                                .Where(coord => coord.y == LastHitCoordinates.y - 1 && coord.x == LastHitCoordinates.x)
                                .FirstOrDefault(),
            Direction.West => _possibleCoordinatesShots
                                .Where(coord => coord.x == LastHitCoordinates.x - 1 && coord.y == LastHitCoordinates.y)
                                .FirstOrDefault(),
            Direction.North => _possibleCoordinatesShots
                                .Where(coord => coord.y == LastHitCoordinates.y + 1 && coord.x == LastHitCoordinates.x)
                                .FirstOrDefault(),
            _ => default
        };
    }
}

