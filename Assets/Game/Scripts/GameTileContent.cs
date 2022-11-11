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

    public virtual void Initialize(GameTileContentFactory factory)
    {
        OriginFactory = factory;
        PositionTiles = new List<Vector3>(Size);
    }
    public void Recycle()
    {
        OriginFactory.Reclaim(this);
    }

    public virtual void GameUpdate() { }

    public override bool Equals(object obj)
    {
        return obj is GameTileContent &&
               base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return PositionHead.GetHashCode();
    }

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

    public void SetNormal()
    {
        for (var i = 0; i < _renderers.Length; i++)
            _renderers[i].material.color = _colors[i];
    }

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
