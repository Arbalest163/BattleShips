using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private GameTileContentType _type;

    [SerializeField]
    private TextMeshProUGUI _text;

    private Action<GameTileContentType> _listenerAction;

    private Func<GameTileContentType, (string, Color)> _listenerChange;

    public void AddListener(Action<GameTileContentType> listenerAction)
    {
        _listenerAction = listenerAction;
    }

    public void AddListener(Func<GameTileContentType, (string, Color)> listenerChange)
    {
        _listenerChange = listenerChange;
    }

    public void ChangeText(string text, Color color)
    {
        _text.text = text;
        _text.color = color;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        _listenerAction?.Invoke(_type);
    }

    private void OnGUI()
    {
        var (text, color) = _listenerChange?.Invoke(_type) ?? ("0", Color.red);
        _text.text = text;
        _text.color = color;
    }
}
