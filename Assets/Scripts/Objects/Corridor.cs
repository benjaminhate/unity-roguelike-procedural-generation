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

    public bool IsIn(Vector2Int coord)
    {
        Vector2Int pos = this.start;
        while (Vector2Int.Distance(pos, this.start) < this.length)
        {
            if (pos == coord) return true;
            pos = this.Forward(pos);
        }
        return false;
    }

    public Vector2Int Forward(Vector2Int coord)
    {
        switch (this.dir)
        {
            case Direction.LEFT:
                coord[0] -= 1;
                break;
            case Direction.UP:
                coord[1] += 1;
                break;
            case Direction.RIGHT:
                coord[0] += 1;
                break;
            case Direction.DOWN:
                coord[1] -= 1;
                break;
        }
        return coord;
    }
}
