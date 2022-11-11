using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField]
    private GameTile _tilePrefab;

    private GameTile[] _tiles;

    private List<GameTileContent> _ships = new List<GameTileContent>(10);

    private GameTileContentFactory _contentFactory;

    private float _offsetBoard;

    private const int MINIMAL_CHECK_SIZE = 2;

    private const byte SIZE = 10;
    public byte SizeX => SIZE;
    public byte SizeY => SIZE;

    public void Initialize(GameTileContentFactory contentFactory, float offsetBoard = 0f)
    {
        _offsetBoard = offsetBoard;
        _contentFactory = contentFactory;
        _tiles = new GameTile[SizeX * SizeY];
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

    public void DestroyTiles(GameTile[] tiles)
    {
        foreach(var tile in tiles)
        {
            _ships.Remove(tile.Content);
            tile.Content.Recycle();
            tile.Content = _contentFactory.Get(GameTileContentType.Empty);
        }
    }

    private Vector2Int ConvertToFiledCoordinates(Ray ray)
    {
        if (Physics.Raycast(ray, out var hit, float.MaxValue, 1))
        {
            return ConvertToFiledCoordinates(hit.point);
        }
        return  new Vector2Int(-1, -1);
    }

    private Vector2Int ConvertToFiledCoordinates(Vector3 coordinates)
    {
        var x = Adjust—oordinates(coordinates.x + _offsetBoard + SizeX * 0.5f);
        var y = Adjust—oordinates(coordinates.z + SizeY * 0.5f);
        return new Vector2Int(x, y);
    }

    private int Adjust—oordinates(float num)
    {
        return num < 0 ? -1 : (int)num;
    }

    public IEnumerable<GameTile> GetTiles(Ray ray, GameTileContent content)
    {
        var coordinates = ConvertToFiledCoordinates(ray);
        return GetTiles(coordinates, content);
    }

    public GameTile GetTileByShot(Ray ray)
    {
        var coordinates = ConvertToFiledCoordinates(ray);
        return GetTileByShot(coordinates);
    }

    public GameTile GetTileByShot(Vector2Int coordinates)
    {
        if (!CheckOutOfBounds(coordinates))
        {
            var x = coordinates.x;
            var y = coordinates.y;
            var tile = _tiles[x + y * SizeX];
            if(tile.Content.Type < GameTileContentType.ExplosionMissing)
            {
                return tile;
            }
        }
        return null;
    }
    public GameTile GetTile(Ray ray)
    {
        var coordinates = ConvertToFiledCoordinates(ray);
        return GetTile(coordinates);
    }

    public GameTile GetTile(Vector2Int coordinates)
    {
        if (!CheckOutOfBounds(coordinates))
        {
            return _tiles[coordinates.x + coordinates.y * SizeX];
        }
        return null;
    }

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
                else if(content.Direction == Direction.North)
                {
                    yield return _tiles[x + (y + i) * SizeY];
                }
                else if (content.Direction == Direction.West)
                {
                    yield return _tiles[x - i + y * SizeX];
                }
                else if(content.Direction == Direction.South)
                {
                    yield return _tiles[x + (y - i) * SizeY];
                }
            }
        }
    }

    public GameTile[] GetTilesByDestroy(Ray ray)
    {
        var tile = GetTile(ray);
        if (tile == null || tile.Content.Type == GameTileContentType.Empty)
        {
            return Array.Empty<GameTile>();
        }
        return _tiles.Where(x => x.Content.Equals(tile.Content)).ToArray();
    }

    public int GetCountShips(GameTileContentType type)
    {
        return _ships.Where(x => x.Type == type).Count();
    }

    private bool CheckPossibilityBuilding(Vector2Int coordinates)
    {
        var tile = GetTile(coordinates);
        if(tile != null)
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

    public bool CheckPossibilityBuilding(Ray ray, GameTileContent content)
    {
        var coordinates = ConvertToFiledCoordinates(ray);
        return CheckPossibilityBuilding(coordinates, content);
    }

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

    public BoardData GenerateBordData()
    {
        var shipsData = new List<ShipData>();
        foreach(var ship in _ships)
        {
            var coordinates = ConvertToFiledCoordinates(ship.PositionHead);
            shipsData.Add(new ShipData
            {
                X = coordinates.x,
                Y = coordinates.y,
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

    public bool CheckPosibilityShoot(Ray ray)
    {
        var coordinates = ConvertToFiledCoordinates(ray);
        return CheckPosibilityShoot(coordinates);
    }

    public bool CheckPosibilityShoot(Vector2Int coordinates)
    {
        var available = !CheckOutOfBounds(coordinates);
        if (available)
        {
            return _tiles[coordinates.x + coordinates.y * SizeX].Content.Type != GameTileContentType.ExplosionMissing;
        }
        return false;
    }

    public StatusShoot Shot(GameTile tile)
    {
        var explosion = GetExplosion(tile);

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

    public void HideShips()
    {
        foreach (var ship in _ships)
        {
            SetLayerGameObjects(ship.gameObject, SetLayerGameObject, "EnemyBoard");
        }
    }

    public void ShowShips()
    {
        foreach (var ship in _ships)
        {
            SetLayerGameObjects(ship.gameObject, SetLayerGameObject, "Default");
        }
    }

    public void ShowShip(GameTile tile)
    {
        var ship = _ships.Where(x => x.PositionTiles.Contains(tile.LocalPosition)).FirstOrDefault();
        SetLayerGameObjects(ship.gameObject, SetLayerGameObject, "Default");
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

    public bool CheckForShips()
    {
        return _tiles == null
                || _tiles.Where(x => x.Content.Type > GameTileContentType.Empty && x.Content.Type < GameTileContentType.ExplosionMissing).Count() > 0;
    }

    private GameTileContent GetExplosion(GameTile tile)
    {
        return tile.Content.Type == GameTileContentType.Empty
             ? _contentFactory.Get(GameTileContentType.ExplosionMissing)
             : _contentFactory.Get(GameTileContentType.ExplosionShip);
    }

    public void FireAround(GameTile tile)
    {
        var ship = _ships.Where(x => x.PositionTiles.Contains(tile.LocalPosition)).FirstOrDefault();
        if (ship == null)
        {
            return;
        }
        
        var direction = ship.Direction;
        var cordinates = ConvertToFiledCoordinates(ship.PositionHead);
        var x = cordinates.x;
        var y = cordinates.y;

        var (checkX, checkY) = GetSizeCheckingBoard(ship.Size, direction);

        for (int i = -1; i < checkY; i++)
        {
            for (int j = -1; j < checkX; j++)
            {
                var checkCoordinates = direction == Direction.East || direction == Direction.North
                    ? new Vector2Int(x + j, y + i)
                    : new Vector2Int(x - j, y - i);

                var shotTile = GetTile(checkCoordinates);
                if(shotTile != null && shotTile.Content.Type == GameTileContentType.Empty)
                {
                    shotTile.Content = GetExplosion(shotTile);
                }
            }
        }
    }
}

public enum StatusShoot
{
    Missing,
    Wounded,
    Killed,
}
