using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shooter : MonoBehaviour
{
    [SerializeField]
    private Transform _scrollViewContent;
    [SerializeField]
    private TextMeshProUGUI _messages;
    private Vector3 _positionMessages;

    private Camera _camera;
    private GameBoard _playerGameBoard;
    private GameBoard _enemyGameBoard;
    [SerializeField] private Button _activate;

    private bool _isPlayerTurn;
    private bool _inProcessEnemyShot;

    private bool _isEnabled = false;

    private Enemy _enemy;

    private List<Vector2Int> _enemysShootCoorinates;
    private Ray TouchRay => _camera.ScreenPointToRay(Input.mousePosition);

    public void Initialize(Camera camera, GameBoard playerGameBoard, GameBoard enemyGameBoard)
    {
        _camera = camera;
        _playerGameBoard = playerGameBoard;
        _enemyGameBoard = enemyGameBoard;
    }

    private void Awake()
    {
        var enemysShootCoorinates = new List<Vector2Int>(100);
        foreach (var i in 0..9)
        {
            foreach (var j in 0..9)
            {
                enemysShootCoorinates.Add(new Vector2Int(i, j));
            }
        }
        _activate.onClick.AddListener(OnActivate);
        _positionMessages = _messages.transform.localPosition;

        _enemy = new Enemy();
        _enemy.Initialize(enemysShootCoorinates);
    }

    public void Disable()
    {
        _isEnabled = false;
    }

    private void LateUpdate()
    {
        if(_isEnabled)
        {
            if(_isPlayerTurn)
            {
                ShotPlayer();
            }
            else
            {
                if(_inProcessEnemyShot)
                {
                    return;
                }
                StartCoroutine(ShootEnemy());
            }
        }
    }

    private IEnumerator ShootEnemy()
    {
        _inProcessEnemyShot = true;

        var coordinates = _enemy.GetCoordinatesShot();
        var tile = _playerGameBoard.GetTileByShot(coordinates);
        if (tile == null)
        {
            _enemy.RemoveCoordinates(coordinates);
            yield return null;
        }
        else
        {
            yield return new WaitForSeconds(1.2f);
            var statusShot = _playerGameBoard.Shot(tile);
            if (statusShot == StatusShoot.Missing)
            {
                _enemy.NeedChangeDirection = true;
                _isPlayerTurn = true;
            }
            else if (statusShot == StatusShoot.Killed)
            {
                _enemy.FinishingMode = false;
                _playerGameBoard.FireAround(tile);
            }
            else
            {
                _enemy.LastHitCoordinates = coordinates;
                _enemy.FinishingMode = true;
                _enemy.NeedChangeDirection = false;
            }
            
            ReportResult("Противник", statusShot, Color.red);
        }

        _inProcessEnemyShot = false;
    }

    private void ShotPlayer()
    {
        if (IsPointerDown())
        {
            var plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(TouchRay, out var _))
            {
                var tile = _enemyGameBoard.GetTileByShot(TouchRay);
                if (tile != null)
                {
                    var statusShot = _enemyGameBoard.Shot(tile);
                    if (statusShot == StatusShoot.Missing)
                    {
                        _isPlayerTurn = false;
                    }
                    else if(statusShot == StatusShoot.Killed)
                    {
                        _enemyGameBoard.ShowShip(tile);
                        _enemyGameBoard.FireAround(tile);
                    }
                    ReportResult("Игрок", statusShot, Color.blue);
                }
            }
        }
    }

    private bool IsPointerDown()
    {
        // Для виндоус
        return Input.GetMouseButtonDown(0);
        // Для мобилок
        //return Input.touches.Length == 0;
    }

    private void OnActivate()
    {
        _isEnabled = true;
        TossCoin();
        _activate.gameObject.SetActive(false);
    }

    private void ReportResult(string name, StatusShoot statusShoot, Color color)
    {
        var text = $"{name}: {GetMessage(statusShoot)}{Environment.NewLine}{_messages.text}";
        AddMessage(text, color);
    }

    private void TossCoin()
    {
        _isPlayerTurn = UnityEngine.Random.Range(0, 100) > 50;
        if(_isPlayerTurn)
        {
            AddMessage("Первым ходит игрок", Color.blue);
        }
        else
        {
            AddMessage("Первым ходит противник", Color.red);
        }
    }

    private string GetMessage(StatusShoot statusShoot)
    {
        return statusShoot switch
        {
            StatusShoot.Wounded => "Ранил",
            StatusShoot.Killed => "Убил",
            _ => "Мимо"
        };
    }

    private void AddMessage(string text, Color color)
    {
        var message = Instantiate(_messages, _scrollViewContent);
        message.color = color;
        message.text = text;
        var heightElement = new Vector3(0, _messages.fontSize + 5f, 0);
        var positionCash = _positionMessages -= heightElement;
        foreach (Transform child in _scrollViewContent)
        {
            child.transform.localPosition = positionCash;
            positionCash += heightElement;
        }
    }
}
