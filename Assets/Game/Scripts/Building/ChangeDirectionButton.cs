using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeDirectionButton : MonoBehaviour, IPointerDownHandler
{
    private Action _listenerAction;
    [SerializeField]
    private TextMeshProUGUI _text;

    public void AddListener(Action listenerAction)
    {
        _listenerAction = listenerAction;
    }

    public void ChangeText(string text)
    {
        _text.text = text;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        _listenerAction?.Invoke();
    }
}
