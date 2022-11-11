using System;

[Serializable]
public class ShipData
{
    public int X { get; set; }
    public int Y { get; set; }
    public GameTileContentType Type;
    public Direction Direction { get; set; }
}

