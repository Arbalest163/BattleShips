using System;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class GameTileContent : MonoBehaviour
{
    [SerializeField]
    private Renderer[] _renderers = Array.Empty<Renderer>();

    private Color[] _colors;

    [SerializeField]
    private GameTileContentType _type;

    public Direction Direction { get; set; }

    public Vector3 PositionHead => transform.localPosition;

    public GameTileContentType Type => _type;

    public virtual int Size => (int)_type;

    public List<Vector3> PositionTiles { get; private set; }

    protected GameTileContentFactory OriginFactory { get; private set; }

    /// <summary>
    /// Инициализация
    /// </summary>
    /// <param name="factory">Фабрика контента</param>
    public virtual void Initialize(GameTileContentFactory factory)
    {
        OriginFactory = factory;
        PositionTiles = new List<Vector3>(Size);
    }

    /// <summary>
    /// Удаление объекта
    /// </summary>
    public void Recycle()
    {
        OriginFactory.Reclaim(this);
    }

    public override bool Equals(object obj)
    {
        return obj is GameTileContent &&
               base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return PositionHead.GetHashCode();
    }

    /// <summary>
    /// Изменение цвета объекта
    /// </summary>
    /// <param name="avaliable"></param>
    public void SetTransparent(bool avaliable)
    {
        foreach (var renderer in _renderers)
        {
            if (avaliable)
                renderer.material.color = Color.green;
            else
                renderer.material.color = Color.red;
        }
    }

    /// <summary>
    /// Установка нормального цвета из кэша
    /// </summary>
    public void SetNormal()
    {
        for (var i = 0; i < _renderers.Length; i++)
            _renderers[i].material.color = _colors[i];
    }

    /// <summary>
    /// Кэширование цветов
    /// </summary>
    public void CasheColor()
    {
        for (var i = 0; i < _renderers.Length; i++)
            _colors[i] = _renderers[i].material.color;
    }

    private void Awake()
    {
        _colors = new Color[_renderers.Length];
    }

    //private void OnDrawGizmos()
    //{
    //    for (int x = 0; x < Size; x++)
    //    {
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawCube(transform.position + new Vector3(x, 0, 0), new Vector3(1, .1f, 1));
    //    }
    //}
}
