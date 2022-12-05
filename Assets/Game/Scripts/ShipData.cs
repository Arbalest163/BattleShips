using System;

[Serializable]
public class ShipData
{
    public byte X { get; set; }
    public byte Y { get; set; }
    public GameTileContentType Type;
    public Direction Direction { get; set; }
}

