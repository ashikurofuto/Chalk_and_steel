using System;
/// <summary>
/// Направления дверей в комнате.
/// </summary>
[Flags]
public enum DoorDirections
{
    None = 0,
    North = 1 << 0,
    East = 1 << 1,
    South = 1 << 2,
    West = 1 << 3
}