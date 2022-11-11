using Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PrepareTilesBuilder : TilesBuilder
{
    [SerializeField]
    private List<BuildButton> _buttons;

    [SerializeField]
    private ChangeDirectionButton _changeDirectionButton;

    [SerializeField]
    private Button _autoBuildingButton;

    [SerializeField]
    private Button _clearButton;

    private Camera _camera;
    private Ray TouchRay => _camera.ScreenPointToRay(Input.mousePosition);

    private void Awake()
    {
        _buttons.ForEach(b =>
        {
            b.AddListener(OnBuildingSelected);
            b.AddListener(OnUpdateTextButtons);
        });
        _changeDirectionButton.AddListener(OnChangeDirectionBuilding);
        _autoBuildingButton.onClick.AddListener(OnAutoBuilding);
        _clearButton.onClick.AddListener(OnClear);
    }

    private void Update()
    {
        if (_isEnabled == false)
        {
            return;
        }
        if (_pendingTile == null)
        {
            ProcessDestroying();
        }
        else
        {
            ProcessBuilding();
        }
    }

    public void Initialize(GameTileContentFactory contentFactory, Camera camera, GameBoard gameBoard)
    {
        base.Initialize(contentFactory, gameBoard);
        _camera = camera;
    }

    public bool CheckFull()
    {
        foreach(var i in 1..4)
        {
            var type = (GameTileContentType)i;
            if(_gameBoard.GetCountShips(type) != GetQuantityLimit(type))
            {
                return false;
            }
        }
        return true;
    }

    private void OnBuildingSelected(GameTileContentType type)
    {
        if (_gameBoard.GetCountShips(type) == GetQuantityLimit(type))
        {
            return;
        }
        _pendingTile = _contentFactory.Get(type);
        _pendingTile.Direction = DirectionBuilding;
        _pendingTile.transform.localRotation = DirectionBuilding.GetRotation();
        _pendingTile.CasheColor();
    }

    private (string, Color) OnUpdateTextButtons(GameTileContentType type)
    {
        if (_isEnabled)
        {
            var count = GetQuantityLimit(type) - _gameBoard.GetCountShips(type);
            var color = count > 0 ? Color.green : Color.red;
            return (count.ToString(), color);
        }
        return ("0", Color.red);
    }

    private void ProcessBuilding()
    {
        var plane = new Plane(Vector3.up, Vector3.zero);
        bool avaliable = false;
        if (plane.Raycast(TouchRay, out var position))
        {
            var buildPosition = TouchRay.GetPoint(position);

            avaliable = _gameBoard.CheckPossibilityBuilding(TouchRay, _pendingTile);

            _pendingTile.transform.position = new Vector3(buildPosition.x, 0, buildPosition.z);
            _pendingTile.SetTransparent(avaliable);
        }

        if (IsPointerUp())
        {
            if (avaliable)
            {
                var tiles = _gameBoard.GetTiles(TouchRay, _pendingTile).ToArray();
                _gameBoard.Build(tiles, _pendingTile);
                _pendingTile.SetNormal();
            }
            else
            {
                Destroy(_pendingTile.gameObject);
            }
            _pendingTile = null;
        }
    }

    private void ProcessDestroying()
    {
        if (IsPointerUp())
        {
            var tiles = _gameBoard.GetTilesByDestroy(TouchRay);
            if (tiles.Any())
            {
                _gameBoard.DestroyTiles(tiles);
            }
        }
    }

    private bool IsPointerUp()
    {
        // Для виндоус
        return Input.GetMouseButtonUp(0);
        // Для мобилок
        //return Input.touches.Length == 0;
    }
}

