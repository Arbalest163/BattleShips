using Newtonsoft.Json;
using System;

[Serializable]
public class BoardData
{
    public int Version;
    public int AccountId;
    public byte X;
    public byte Y;
    public ShipData[] ShipsData;

    public byte[] Serialize()
    {
        var data = JsonConvert.SerializeObject(ShipsData).ToCharArray();
        var result = new byte[sizeof(int) + sizeof(int) + sizeof(byte) * 2 +
                              sizeof(char) * data.Length];

        var offset = 0;
        offset += ByteConverter.AddToStream(Version, result, offset);
        offset += ByteConverter.AddToStream(AccountId, result, offset);
        offset += ByteConverter.AddToStream(X, result, offset);
        offset += ByteConverter.AddToStream(Y, result, offset);
        foreach (var c in data)
        {
            offset += ByteConverter.AddToStream((byte)c, result, offset);
        }
        
        return result;
    }

    public static BoardData Deserialize(byte[] data)
    {
        var offset = 0;

        offset += ByteConverter.ReturnFromStream(data, offset, out int version);
        if (version != Serialization.VERSION)
        {
            return null;
        }

        var result = new BoardData();

        offset += ByteConverter.ReturnFromStream(data, offset, out result.AccountId);
        offset += ByteConverter.ReturnFromStream(data, offset, out result.X);
        offset += ByteConverter.ReturnFromStream(data, offset, out result.Y);

        int size = result.X * result.Y;
        offset += ByteConverter.ReturnFromStream(data, offset, size, out byte[] content);
        offset += ByteConverter.ReturnFromStream(data, offset, size, out byte[] direction);

        result.ShipsData = new ShipData[content.Length];
        //TODO Gдумать над десериализацией.
        //for (var i = 0; i < content.Length; i++)
        //{
        //    result.ContentType[i] = (GameTileContentType)content[i];
        //}
        //result.Direction = new Direction[direction.Length];
        //for (var i = 0; i < content.Length; i++)
        //{
        //    result.Direction[i] = (Direction)direction[i];
        //}

        return result;
    }
}
