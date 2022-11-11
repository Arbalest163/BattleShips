using System;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
    private GameTileContent _content;

    public Vector3 LocalPosition => transform.localPosition;

    public GameTileContent Content
    {
        get => _content;
        set
        {
            _content = value;
            _content.transform.localPosition = transform.localPosition;
        }
    }

    public override bool Equals(object obj)
    {
       return obj is GameTile tile
            && Content.Equals(tile.Content);
    }

    public override int GetHashCode()
    {
        return LocalPosition.GetHashCode();
    }
}

