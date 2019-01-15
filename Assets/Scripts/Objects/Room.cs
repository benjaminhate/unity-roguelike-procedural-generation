using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//http://csharphelper.com/blog/2014/07/determine-whether-a-point-is-inside-a-polygon-in-c/

public class Room {
    public List<Vector2Int> corners;
    public Vector2Int door;

    public Room(Vector2Int door, List<Vector2Int> corners = null)
    {
        this.corners = corners;
        this.door = door;
    }

    public bool IsInside(Vector2Int point)
    {
        return true;
    }
}
