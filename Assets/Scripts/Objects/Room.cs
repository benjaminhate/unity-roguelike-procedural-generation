using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//http://csharphelper.com/blog/2014/07/determine-whether-a-point-is-inside-a-polygon-in-c/

public class Room {
    public List<Vector2Int> corners;
    public Vector2Int door;

    public Room(Vector2Int door, List<Vector2Int> corners = null)
    {
        this.door = door;
        this.corners = corners;
    }

    public bool IsInside(Vector2Int point)
    {
        return true;
    }

	public Vector2 GetCenter() {
		if (this.corners.Count == 4)
            return GetCentroid();
        Vector2 pos = (Vector2) (this.corners[0] + this.corners[1]) / 2.0f;
        for (int i = 1; i < this.corners.Count; ++i)
        {
            Vector2 new_pos = (Vector2) (this.corners[i] + this.corners[(i + 1) % this.corners.Count]) / 2.0f;
            if (Vector2.Distance(pos, this.door) < Vector2.Distance(new_pos, this.door))
                pos = new_pos;
        }
        return pos;
	}

	public Vector2 GetCentroid()
	{
		Vector2 center = new Vector2(0f, 0f);
		foreach (Vector2Int corner in corners)
		{
			center += corner;
		}
		return center / corners.Count;
	}
}
