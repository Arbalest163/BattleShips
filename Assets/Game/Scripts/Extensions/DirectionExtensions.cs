using UnityEngine;
public static class DirectionExtensions
{
    private static Quaternion[] _rotations =
    {
        Quaternion.identity,
        Quaternion.Euler(0f, 90f, 0f),
        Quaternion.Euler(0f, 180f, 0f),
        Quaternion.Euler(0f, 270f, 0f)
    };

    public static Quaternion GetRotation(this Direction direction) => _rotations[(int)direction];

    public static Direction ChangeDirection(this Direction current)
        => current + 1 > Direction.North ? Direction.East : current + 1;
        
}

public enum Direction : byte
{
    East = 0,
    South = 1,
    West = 2,
    North = 3,
}
