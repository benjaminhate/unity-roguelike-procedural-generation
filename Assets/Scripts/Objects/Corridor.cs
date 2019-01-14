using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { LEFT = 0, UP, RIGHT, DOWN };

public class Corridor {
    public Vector2Int start;
    public Direction dir;
    public uint length;

    public Corridor(Vector2Int start, Direction dir, uint length = 1)
    {
        this.start = start;
        this.dir = dir;
        this.length = length;
    }

    public bool IsIn(Vector2Int point)
    {
        if (Vector2Int.Distance(point, start) >= length) return false;
        switch(this.dir)
        {
            case Direction.LEFT:
                return (point.y <= start.y && point.x == start.x);
            case Direction.UP:
                return (point.y == start.y && point.x >= start.x);
            case Direction.RIGHT:
                return (point.y >= start.y && point.x == start.x);
            case Direction.DOWN:
                return (point.y == start.y && point.x <= start.x);
            default:
                return false;
        }
    }
}
