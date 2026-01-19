using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChalkAndSteel.Services
{
    public class DoorHelper
    {
        private readonly System.Random _random;

        public DoorHelper(int seed = 0)
        {
            _random = seed == 0 ? new System.Random() : new System.Random(seed);
        }

        public bool HasDoor(DoorDirections doors, DoorDirections direction)
        {
            return (doors & direction) != 0;
        }

        public DoorDirections AddDoor(DoorDirections doors, DoorDirections direction)
        {
            if (direction == DoorDirections.None)
                return doors;

            return doors | direction;
        }

        public DoorDirections RemoveDoor(DoorDirections doors, DoorDirections direction)
        {
            return doors & ~direction;
        }

        public Vector3Int GetDirectionVector(DoorDirections direction)
        {
            return direction switch
            {
                DoorDirections.North => Vector3Int.up,
                DoorDirections.East => Vector3Int.right,
                DoorDirections.South => Vector3Int.down,
                DoorDirections.West => Vector3Int.left,
                _ => Vector3Int.zero
            };
        }

        public DoorDirections GetOppositeDirection(DoorDirections direction)
        {
            return direction switch
            {
                DoorDirections.North => DoorDirections.South,
                DoorDirections.East => DoorDirections.West,
                DoorDirections.South => DoorDirections.North,
                DoorDirections.West => DoorDirections.East,
                _ => DoorDirections.None
            };
        }

        public int GetDoorCount(DoorDirections doors)
        {
            int count = 0;
            if (HasDoor(doors, DoorDirections.North)) count++;
            if (HasDoor(doors, DoorDirections.East)) count++;
            if (HasDoor(doors, DoorDirections.South)) count++;
            if (HasDoor(doors, DoorDirections.West)) count++;
            return count;
        }

        public List<DoorDirections> GetDoorDirectionsList(DoorDirections doors)
        {
            var directions = new List<DoorDirections>();

            if (HasDoor(doors, DoorDirections.North)) directions.Add(DoorDirections.North);
            if (HasDoor(doors, DoorDirections.East)) directions.Add(DoorDirections.East);
            if (HasDoor(doors, DoorDirections.South)) directions.Add(DoorDirections.South);
            if (HasDoor(doors, DoorDirections.West)) directions.Add(DoorDirections.West);

            return directions;
        }

        public void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }

        public string DoorDirectionsToString(DoorDirections doors)
        {
            var result = "";
            if (HasDoor(doors, DoorDirections.North)) result += "N";
            if (HasDoor(doors, DoorDirections.East)) result += "E";
            if (HasDoor(doors, DoorDirections.South)) result += "S";
            if (HasDoor(doors, DoorDirections.West)) result += "W";
            return string.IsNullOrEmpty(result) ? "None" : result;
        }
    }
}