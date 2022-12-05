using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    private const byte MINIMAL_CHECK_SIZE = 2;
    private const byte SIZE = 10;
    private const byte COUNT_ALL_SHIPS = 10;

    [SerializeField]
    private GameTile _tilePrefab;

    private GameTile[] _tiles;

    private IList<GameTileContent> _ships;

    private GameTileContentFactory _contentFactory;

    private float _offsetBoard;

    /// <summary>
    /// Размер поля по горизонатли
    /// </summary>
    public byte SizeX => SIZE;
    /// <summary>
    /// Размер поля по вертикали
    /// </summary>
    public byte SizeY => SIZE;

    /// <summary>
    /// Инициализация игрового поля
    /// </summary>
    /// <param name="contentFactory">Фабрика контента</param>
    /// <param name="offsetBoard">Смещение поля относительно центра экрана</param>
    public void Initialize(GameTileContentFactory contentFactory, float offsetBoard = 0f)
    {
        _offsetBoard = offsetBoard;
        _contentFactory = contentFactory;
        _tiles = new GameTile[SizeX * SizeY];
        _ships =  new List<GameTileContent>(COUNT_ALL_SHIPS);
        var offset = new Vector2((SizeX - 1) * 0.5f + _offsetBoard, (SizeY - 1) * 0.5f);
        for (int i = 0, z = 0; z < SizeY; z++)
        {
            for (int x = 0; x < SizeX; x++, i++)
            {
                GameTile tile = _tiles[i] = Instantiate(_tilePrefab);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(x - offset.x, 0f, z - offset.y);
            }
        }
        Clear();
    }

    /// <summary>
    /// Построить контент
    /// </summary>
    /// <param name="tiles">Тайлы, на которых необходимо произвести строительство</param>
    /// <param name="content">Контент</param>
    public void Build(GameTile[] tiles, GameTileContent content)
    {
        for (int i = tiles.Length - 1; i >= 0; i--)
        {
            tiles[i].Content = content;
            content.PositionTiles.Add(tiles[i].LocalPosition);
            if(i == 0)
            {
                content.transform.localPosition = tiles[i].LocalPosition;
            }
        }
        _ships.Add(content);
    }

    /// <summary>
    /// Удаление контента на тайлах
    /// </summary>
    /// <param name="tiles"></param>
    public void DestroyTiles(GameTile[] tiles)
    {
        foreach(var tile in tiles)
        {
            _ships.Remove(tile.Content);
            tile.Content.Recycle();
            tile.Content = _contentFactory.Get(GameTileContentType.Empty);
        }
    }

    /// <summary>
    /// Получение тайлов по координатам из луча
    /// </summary>
    /// <param name="ray">Луч</param>
    /// <param name="content">Контент, для которого необходимо получить тайлы</param>
    /// <returns>Коллекция тайлов</returns>
    public IEnumerable<GameTile> GetTiles(Ray ray, GameTileContent content)
    {
        var coordinates = ConvertToFiledCoordinates(ray);
        return GetTiles(coordinates, content);
    }

    /// <summary>
    /// Получение тайлов по координатам
    /// </summary>
    /// <param name="coordinates">Координаты начала получения тайлов</param>
    /// <param name="content">Контент, для которого необходимо получить тайлы</param>
    /// <returns>Коллекция тайлов</returns>
    public IEnumerable<GameTile> GetTiles(Vector2Int coordinates, GameTileContent content)
    {
        if (!CheckOutOfBounds(coordinates, content.Size, content.Direction))
        {
            var x = coordinates.x;
            var y = coordinates.y;

            for (int i = 0; i < content.Size; i++)
            {
                if (content.Direction == Direction.East)
                {
                    yield return _tiles[x + i + y * SizeX];
                }
                else if (content.Direction == Direction.North)
                {
                    yield return _tiles[x + (y + i) * SizeY];
                }
                else if (content.Direction == Direction.West)
                {
                    yield return _tiles[x - i + y * SizeX];
                }
                else if (content.Direction == Direction.South)
                {
                    yield return _tiles[x + (y - i) * SizeY];
                }
            }
        }
    }

    /// <summary>
    /// Получение тайла для выстрела по координатам из луча
    /// </summary>
    /// <param name="ray">Луч</param>
    /// <returns>Тайл для выстрела или null</returns>
    public GameTile GetTileByShot(Ray ray)
    {
        var coordinates = ConvertToFiledCoordinates(ray);
        return GetTileByShot(coordinates);
    }

    /// <summary>
    /// Получение тайла для выстрела по координатам
    /// </summary>
    /// <param name="coordinates">Координаты</param>
    /// <returns>Тайл для выстрела или null</returns>
    public GameTile GetTileByShot(Vector2Int coordinates)
    {
        var tile = GetTile(coordinates);
        return tile != null && tile.Content.Type < GameTileContentType.ExplosionMissing
            ? tile 
            : null;
    }

    /// <summary>
    /// Получение тайла по координатам
    /// </summary>
    /// <param name="coordinates">Координаты</param>
    /// <returns>Тайл null</returns>
    public GameTile GetTile(Vector2Int coordinates)
    {
        if (!CheckOutOfBounds(coordinates))
        {
            return _tiles[coordinates.x + coordinates.y * SizeX];
        }
        return null;
    }

    /// <summary>
    /// Получение тайлов для удаления контента по координатам из луча
    /// </summary>
    /// <param name="ray">Луч</param>
    /// <returns>Колекция тайлов для удаления, либо пустая коллекция null</returns>
    public GameTile[] GetTilesByDestroy(Ray ray)
    {
        var coordinates = ConvertToFiledCoordinates(ray);
        var tile = GetTile(coordinates);
        if (tile == null || tile.Content.Type == GameTileContentType.Empty)
        {
            return Array.Empty<GameTile>();
        }
        return _tiles.Where(x => x.Content.Equals(tile.Content)).ToArray();
    }

    /// <summary>
    /// Получение количества контента на поле по типу
    /// </summary>
    /// <param name="type">Тип контента</param>
    /// <returns>Количество контента, определённого типа</returns>
    public int GetCountShips(GameTileContentType type)
    {
        return _ships.Where(x => x.Type == type).Count();
    }

    /// <summary>
    /// Проверка возможности строительства контента по координатам из луча
    /// </summary>
    /// <param name="ray">Луч</param>
    /// <param name="content">Контент</param>
    /// <returns>true, если по данным координатам возможно построить контент, иначе false</returns>
    public bool CheckPossibilityBuilding(Ray ray, GameTileContent content)
    {
        var coordinates = ConvertToFiledCoordinates(ray);
        return CheckPossibilityBuilding(coordinates, content);
    }

    /// <summary>
    /// Проверка возможности строительства контента по координатам
    /// </summary>
    /// <param name="coordinates">Луч</param>
    /// <param name="content">Контент</param>
    /// <returns>true, если по данным координатам возможно построить контент, иначе false</returns>
    public bool CheckPossibilityBuilding(Vector2Int coordinates, GameTileContent content)
    {
        var contentSize = content.Size;
        var contentDirection = content.Direction;
        if (CheckOutOfBounds(coordinates, contentSize, contentDirection))
        {
            return false;
        }

        var x = coordinates.x;
        var y = coordinates.y;

        if (_tiles[x + y * SizeX].Content.Type != GameTileContentType.Empty)
        {
            return false;
        }

        var (checkX, checkY) = GetSizeCheckingBoard(contentSize, contentDirection);

        for (int i = -1; i < checkY; i++)
        {
            for (int j = -1; j < checkX; j++)
            {
                var checkCoordinates = contentDirection == Direction.East || contentDirection == Direction.North
                    ? new Vector2Int(x + j, y + i)
                    : new Vector2Int(x - j, y - i);

                var possibilityBuilding = CheckPossibilityBuilding(checkCoordinates);

                if (!possibilityBuilding)
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Очистка поля
    /// </summary>
    public void Clear()
    {
        foreach (var tile in _tiles)
        {
            if(tile.Content != null)
            {
                tile.Content.Recycle();
            }
            tile.Content = _contentFactory.Get(GameTileContentType.Empty);
        }
        _ships.Clear();
    }

    /// <summary>
    /// Сгенерировать контейнер, содержащий информацию об игровом поле
    /// </summary>
    /// <returns>Информация об игровом поле</returns>
    public BoardData GenerateBordData()
    {
        var shipsData = new List<ShipData>();
        foreach(var ship in _ships)
        {
            var coordinates = ConvertToFiledCoordinates(ship.PositionHead);
            shipsData.Add(new ShipData
            {
                X = (byte)coordinates.x,
                Y = (byte)coordinates.y,
                Type = ship.Type,
                Direction = ship.Direction,
            });
        }
        return new BoardData
        {
            Version = 1,
            AccountId = 1,
            X = SizeX,
            Y = SizeY,
            ShipsData = shipsData.ToArray()
        };
    }

    /// <summary>
    /// Загрузить поле из контейнера
    /// </summary>
    /// <param name="boardData"></param>
    public void LoadBoardData(BoardData boardData)
    {
        foreach(var ship in boardData.ShipsData)
        {
            var prefab = _contentFactory.Get(ship.Type);
            prefab.Direction = ship.Direction;
            prefab.transform.localRotation = ship.Direction.GetRotation();
            var tiles = GetTiles(new Vector2Int(ship.X, ship.Y), prefab).ToArray();
            Build(tiles, prefab);
        }
    }

    /// <summary>
    /// Проверить возможность произвести выстрел по координатам из луча
    /// </summary>
    /// <param name="ray">Луч</param>
    /// <returns>true, если выстрел возможен, иначе false</returns>
    public bool CheckPosibilityShoot(Ray ray)
    {
        var coordinates = ConvertToFiledCoordinates(ray);
        return CheckPosibilityShoot(coordinates);
    }

    /// <summary>
    /// Проверить возможность произвести выстрел по координатам
    /// </summary>
    /// <param name="coordinates">Координаты</param>
    /// <returns>true, если выстрел возможен, иначе false</returns>
    public bool CheckPosibilityShoot(Vector2Int coordinates)
    {
        var available = !CheckOutOfBounds(coordinates);
        if (available)
        {
            return _tiles[coordinates.x + coordinates.y * SizeX].Content.Type != GameTileContentType.ExplosionMissing;
        }
        return false;
    }

    /// <summary>
    /// Произвести выстрел по тайлу
    /// </summary>
    /// <param name="tile">Тайл, по которому производится выстрел</param>
    /// <returns>
    /// Статус выстрела: <br/>
    /// Missing - мимо <br/>
    /// Wounded - ранил <br/> 
    /// Killed - убил
    /// </returns>
    public StatusShoot Shot(GameTile tile)
    {
        var explosion = _contentFactory.GetExplosion(tile);

        if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = explosion;
            return StatusShoot.Missing;
        }
        else
        {
            var content = tile.Content;
            tile.Content = explosion;
            var countTiles = _tiles.Where(x => x.Content.Equals(content)).Count();
            if (countTiles > 0)
                return StatusShoot.Wounded;
            else
                return StatusShoot.Killed;
        }
    }

    /// <summary>
    /// Скрыть корабли на поле
    /// </summary>
    public void HideShips()
    {
        foreach (var ship in _ships)
        {
            SetLayerGameObjects(ship.gameObject, SetLayerGameObject, "EnemyBoard");
        }
    }

    /// <summary>
    /// Открыть корабли на поле
    /// </summary>
    public void ShowShips()
    {
        foreach (var ship in _ships)
        {
            SetLayerGameObjects(ship.gameObject, SetLayerGameObject, "Default");
        }
    }

    /// <summary>
    /// Открыть корабль, находящийся на указанном тайле
    /// </summary>
    /// <param name="tile">Тайл</param>
    public void ShowShip(GameTile tile)
    {
        var ship = _ships.Where(x => x.PositionTiles.Contains(tile.LocalPosition)).FirstOrDefault();
        SetLayerGameObjects(ship.gameObject, SetLayerGameObject, "Default");
    }
    
    /// <summary>
    /// Проверить наличие "живых" кораблей на поле
    /// </summary>
    /// <returns>true, если "живые" корабли на поле есть, иначе false</returns>
    public bool CheckForShips()
    {
        return _tiles == null
                || _tiles.Where(x => x.Content.Type > GameTileContentType.Empty 
                                    && x.Content.Type < GameTileContentType.ExplosionMissing)
                .Count() > 0;
    }

    /// <summary>
    /// Получение всех координат контента и вокруг него
    /// </summary>
    /// <param name="content">Контент</param>
    /// <returns>Коллекция координат</returns>
    public Vector2Int[] GetCoordinatesAroundContent(GameTileContent content)
    {
        return GetTilesAround(content).Select(t => ConvertToFiledCoordinates(t.LocalPosition)).ToArray();
    }

    /// <summary>
    /// Произвести обстрел вокруг контента, находящегося на выбранном тайле
    /// </summary>
    /// <param name="tile">Тайл</param>
    public void FireAround(GameTile tile)
    {
        var ship = _ships.Where(x => x.PositionTiles.Contains(tile.LocalPosition)).FirstOrDefault();
        if (ship == null)
        {
            return;
        }

        var tilesAround = GetTilesAround(ship).Where(t => t.Content.Type == GameTileContentType.Empty);

        foreach(var emptyTile in tilesAround)
        {
            emptyTile.Content = _contentFactory.GetExplosion(emptyTile);
        }
    }

    
    private IEnumerable<GameTile> GetTilesAround(GameTileContent content)
    {
        var direction = content.Direction;
        var cordinatesHead = ConvertToFiledCoordinates(content.PositionHead);
        var x = cordinatesHead.x;
        var y = cordinatesHead.y;

        var (checkX, checkY) = GetSizeCheckingBoard(content.Size, direction);

        for (int i = -1; i < checkY; i++)
        {
            for (int j = -1; j < checkX; j++)
            {
                var coordinates = direction == Direction.East || direction == Direction.North
                    ? new Vector2Int(x + j, y + i)
                    : new Vector2Int(x - j, y - i);

                var tile = GetTile(coordinates);
                if (tile != null)
                {
                    yield return tile;
                }
            }
        }
    }

    private Vector2Int ConvertToFiledCoordinates(Vector3 coordinates)
    {
        var x = AdjustСoordinates(coordinates.x + _offsetBoard + SizeX * 0.5f);
        var y = AdjustСoordinates(coordinates.z + SizeY * 0.5f);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Метод необходим для преобразования float к int с определённым способом округления.<br/>
    /// По умолчания значения от -1 до 0 округляются до нуля, <br/>
    /// из-за чего данные отрицательные числа будут считаться находящимися в пределах поля,<br/> 
    /// что, по факту, не так. Поэтому необходимо значения меньше нуля окрглять до -1 вручную.
    /// Это необходимо для фикса, когда игрок нажимает левее/ниже поля <br/>
    /// А строительсвто/выстрел происходят по нулевой координате на поле
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    private int AdjustСoordinates(float num)
    {
        return num < 0 ? -1 : (int)num;
    }

    private Vector2Int ConvertToFiledCoordinates(Ray ray)
    {
        if (Physics.Raycast(ray, out var hit, float.MaxValue, 1))
        {
            return ConvertToFiledCoordinates(hit.point);
        }
        return new Vector2Int(-1, -1);
    }

    private bool CheckPossibilityBuilding(Vector2Int coordinates)
    {
        var tile = GetTile(coordinates);
        if (tile != null)
        {
            return tile.Content.Type == GameTileContentType.Empty;
        }
        return true;
    }

    private bool CheckOutOfBounds(Vector2Int coordinates)
    {
        var x = coordinates.x;
        var y = coordinates.y;
        if (x < 0 || x >= SizeX || y < 0 || y >= SizeY)
        {
            return true;
        }
        return false;
    }
    private bool CheckOutOfBounds(Vector2Int coordinates, int size, Direction direction)
    {
        if (CheckOutOfBounds(coordinates))
        {
            return true;
        }

        var x = coordinates.x;
        var y = coordinates.y;
        if (direction == Direction.East)
        {
            if (x + size > SizeX)
            {
                return true;
            }
        }
        else if (direction == Direction.North)
        {
            if (y + size > SizeY)
            {
                return true;
            }
        }
        else if (direction == Direction.West)
        {
            if (x - size + 1 < 0)
            {
                return true;
            }
        }
        else if (direction == Direction.South)
        {
            if (y - size + 1 < 0)
            {
                return true;
            }
        }
        return false;
    }

    private (int, int) GetSizeCheckingBoard(int size, Direction direction)
    {
        int checkX = MINIMAL_CHECK_SIZE;
        int checkY = MINIMAL_CHECK_SIZE;
        var checkSize = size + 1;

        if (direction == Direction.East || direction == Direction.West)
        {
            checkX = Math.Max(MINIMAL_CHECK_SIZE, checkSize);
        }
        else if (direction == Direction.North || direction == Direction.South)
        {
            checkY = Math.Max(MINIMAL_CHECK_SIZE, checkSize);
        }

        return (checkX, checkY);
    }

    private void SetLayerGameObjects(GameObject parent, Action<GameObject, string> action, string layer)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            action(child.gameObject, layer);
        }
    }

    private void SetLayerGameObject(GameObject gameObject, string layer)
    {
        gameObject.layer = LayerMask.NameToLayer(layer);
    }
}

public enum StatusShoot
{
    Missing,
    Wounded,
    Killed,
}
