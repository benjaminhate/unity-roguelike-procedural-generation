using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon
{
    public uint height, width;
    public uint tile_size;

    public List<Corridor> corridors;
    public List<Room> rooms;

    public uint[,] floor;

    public Dungeon(uint height = 50, uint width = 50)
    {
        /* Initialisation of attributes */
        this.height = height;
        this.width = width;

        this.corridors = new List<Corridor>();
        this.rooms = new List<Room>();
        corridors.Clear();
        rooms.Clear();

        /* 2D array representing the level */
        Floor f = new Floor(height, width);
        this.floor = f.GetFloor();

		for (uint i = 0; i < width; i++) {
			for (uint j = 0; j < height; j++) {
				if (floor [i, j] == 1)
					Debug.Log ("Floor [" + i + "," + j + "] : " + floor [i, j]);
			}
		}

        /* Corridors */
		AddCorridors (floor);

        /* Rooms */
		AddRooms (floor, f.GetNumberOfRooms ());
		SortRoomsCorners ();
    }

    /* Main functions */

    public void AddCorridors(uint[,] floor)
    {
        // Vertical corridors
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (floor[i, j] == 1 && floor[i, j + 1] == 1)
                {
                    Corridor corridor = new Corridor(new Vector2Int(i, j), Direction.UP);
                    while (floor[i, j] == 1 && floor[i - 1, j + 1] != 1 && floor[i + 1, j + 1] != 1 && j < height - 2)
                    {
                        corridor.length += 1;
                        j += 1;
                    }
                    corridors.Add(corridor);
                }
            }
        }

        // Horizontal corridors
        for (int j = 1; j < height - 1; j++)
        {
            for (int i = 1; i < width - 1; i++)
            {
                if (floor[i, j] == 1 && floor[i + 1, j] == 1)
                {
                    Corridor corridor = new Corridor(new Vector2Int(i, j), Direction.RIGHT);
                    while (floor[i, j] == 1 && floor[i + 1, j + 1] != 1 && floor[i + 1, j - 1] != 1 && i < width - 2)
                    {
                        corridor.length += 1;
                        i += 1;
                    }
                    //if (floor[i + 1, j - 1] == 1 && floor[i + 2, j] == 0 && floor[i + 1, j + 1] == 0) corridor.length++;
                    corridors.Add(corridor);
                }
            }
        }
    }

    public void AddRooms(uint[,] floor, uint n_rooms)
    {
        Room[] r = new Room[n_rooms * 50];
        for (uint t = 0; t < n_rooms * 50; t++) r[t] = new Room(new Vector2Int(0, 0), new List<Vector2Int>());
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (IsRoomCorner(floor, new Vector2Int(i, j)))
                    r[(int)floor[i, j] - 3].corners.Add(new Vector2Int(i, j));
                if (floor[i, j] == 2)
                {
                    Vector2Int[] neighbours = GetNeighbours(new Vector2Int(i, j));
                    if (floor[neighbours[0].x, neighbours[0].y] > 2)
                        r[(int)floor[neighbours[0].x, neighbours[0].y] - 3].door = new Vector2Int(i, j);
                    else if (floor[neighbours[2].x, neighbours[2].y] > 2)
                        r[(int)floor[neighbours[2].x, neighbours[2].y] - 3].door = new Vector2Int(i, j);
                    else if (floor[neighbours[4].x, neighbours[4].y] > 2)
                        r[(int)floor[neighbours[4].x, neighbours[4].y] - 3].door = new Vector2Int(i, j);
                    else if (floor[neighbours[6].x, neighbours[6].y] > 2)
                        r[(int)floor[neighbours[6].x, neighbours[6].y] - 3].door = new Vector2Int(i, j);
                }
            }
        }
        for (uint t = 0; t < n_rooms * 50; t++) 
        {
            if (r[t].corners.Count > 3) this.rooms.Add(r[t]);
        }
    }

    public void SortRoomsCorners() // Use Graham Scan algorithm
    {
        foreach(Room room in this.rooms)
        {
            if (room.corners.Count == 4)
                room.corners = SortSquareRoomCorners(room.corners);
            else
                room.corners = SortRoomCorners(room.corners);
        }
    }

    public List<Vector2Int> SortSquareRoomCorners(List<Vector2Int> corners) // Only works for square rooms
    {
        List<Vector2Int> sorted_corners = new List<Vector2Int>();
        Vector2 centre = FindCentroid(corners);
        foreach(Vector2Int corner in corners)
        {
            int index = 0;
            while (index < sorted_corners.Count)
            {
                if (GetAngle(centre, corner) < GetAngle(centre, sorted_corners[index])) break;
                index++;
            }
            sorted_corners.Insert(index, corner);
        }
        return sorted_corners;
    }

    public List<Vector2Int> SortRoomCorners(List<Vector2Int> corners) // For non square rooms
    {
        List<Vector2Int> sorted_corners = new List<Vector2Int>();
        sorted_corners.Add(corners[0]);
        Vector2Int[] vertices = GetConnectedVertices(sorted_corners[sorted_corners.Count - 1], corners);
        Vector2 centroid = FindCentroid(corners);
        int previous_move = 0;
        if (vertices[0] == new Vector2Int(0, 0) && vertices[1] == new Vector2Int(0, 0))
            previous_move = (centroid.x > corners[0].x) ? 2 : 3;
        else if (vertices[1] == new Vector2Int(0, 0) && vertices[2] == new Vector2Int(0, 0))
            previous_move = (centroid.y > corners[0].y) ? 0 : 3;
        else if (vertices[2] == new Vector2Int(0, 0) && vertices[3] == new Vector2Int(0, 0))
            previous_move = (centroid.x < corners[0].x) ? 0 : 1;
        else if (vertices[3] == new Vector2Int(0, 0) && vertices[0] == new Vector2Int(0, 0))
            previous_move = (centroid.y < corners[0].y) ? 2 : 1;
        int i = 0;
        while (sorted_corners.Count < corners.Count && i < 25)
        {
            i++;
            vertices = GetConnectedVertices(sorted_corners[sorted_corners.Count - 1], corners);
            if (vertices[0] == new Vector2Int(0, 0) && vertices[1] == new Vector2Int(0, 0))
            {
                if (previous_move == 3)
                {
                    sorted_corners.Add(vertices[2]);
                    previous_move = 0;
                }
                else if (previous_move == 2)
                {
                    sorted_corners.Add(vertices[3]);
                    previous_move = 1;
                }     
            }
            else if (vertices[1] == new Vector2Int(0, 0) && vertices[2] == new Vector2Int(0, 0))
            {
                if (previous_move == 3)
                {
                    sorted_corners.Add(vertices[0]);
                    previous_move = 2;
                }
                else if (previous_move == 0)
                {
                    sorted_corners.Add(vertices[3]);
                    previous_move = 1;
                }
            }
            else if (vertices[2] == new Vector2Int(0, 0) && vertices[3] == new Vector2Int(0, 0))
            {
                if (previous_move == 1)
                {
                    sorted_corners.Add(vertices[0]);
                    previous_move = 2;
                }
                else if (previous_move == 0)
                {
                    sorted_corners.Add(vertices[1]);
                    previous_move = 3;
                }
            }
            else if (vertices[3] == new Vector2Int(0, 0) && vertices[0] == new Vector2Int(0, 0))
            {
                if (previous_move == 1)
                {
                    sorted_corners.Add(vertices[2]);
                    previous_move = 0;
                }
                else if (previous_move == 2)
                {
                    sorted_corners.Add(vertices[1]);
                    previous_move = 3;
                }
            }
        }
        return sorted_corners;
    }

    public Vector2Int[] GetConnectedVertices(Vector2Int vertex, List<Vector2Int> corners)
    {
        Vector2Int[] vertices = new Vector2Int[4]; // Left, Top, Right, Bottom
        for (int i = 0; i < 4; i++)
            vertices[i] = new Vector2Int(0, 0);
        foreach (Vector2Int corner in corners)
        {
            if (corner.x < vertex.x && corner.y == vertex.y)
                vertices[0] = corner;
            else if (corner.x > vertex.x && corner.y == vertex.y)
                vertices[2] = corner;
            else if (corner.x == vertex.x && corner.y < vertex.y)
                vertices[3] = corner;
            else if (corner.x == vertex.x && corner.y > vertex.y)
                vertices[1] = corner;
        }
        return vertices;
    }
    

    /* Auxiliary functions */

    public float GetAngle(Vector2 p1, Vector2 p2)
    {
        Vector2 diff = p2 - p1;
        if (diff.x >= 0 && diff.y >= 0) // Top-Right
            return Mathf.Atan2(diff.y, diff.x);
        else if (diff.x < 0 && diff.y >= 0) // Top-Left
            return Mathf.Atan2(diff.y, diff.x);
        else if (diff.x < 0 && diff.y < 0) // Bottom-Left
            return Mathf.Atan2(diff.y, diff.x) + 2 * Mathf.PI;
        else if (diff.x >= 0 && diff.y < 0) // Bottom-Right
            return Mathf.Atan2(diff.y, diff.x) + 2 * Mathf.PI;
        return 0f;
    }

    public Vector2 FindCentroid(List<Vector2Int> corners)
    {
        Vector2 centre = new Vector2(0f, 0f);
        foreach (Vector2Int corner in corners)
        {
            centre += corner;
        }
        return centre / corners.Count;
    }

    public Vector2[] GetBoundingBox(List<Vector2Int> corners)
    {
        float big_number = 100000000000f;
        Vector2 bottom_left = new Vector2(big_number, big_number), top_right = -bottom_left;
        foreach (Vector2Int corner in corners)
        {
            if (corner.x < bottom_left.x)
                bottom_left.x = corner.x;
            else if (corner.x > top_right.x)
                top_right.x = corner.x;
            if (corner.y < bottom_left.y)
                bottom_left.y = corner.y;
            else if (corner.y > top_right.y)
                top_right.y = corner.y;
        }
        Vector2[] bounding_box = new Vector2[2];
        bounding_box[0] = bottom_left;
        bounding_box[1] = top_right;
        return bounding_box;
    }


    public bool IsRoomCorner(uint[,] floor, Vector2Int coord)
    {
        if (floor[coord[0], coord[1]] < 3) return false;
        Vector2Int[] neighbours = GetNeighbours(coord);
        if (floor[neighbours[0].x, neighbours[0].y] == 0 && floor[neighbours[1].x, neighbours[1].y] == 0 && floor[neighbours[2].x, neighbours[2].y] == 0)
            return true;
        else if (floor[neighbours[2].x, neighbours[2].y] == 0 && floor[neighbours[3].x, neighbours[3].y] == 0 && floor[neighbours[4].x, neighbours[4].y] == 0)
            return true;
        else if (floor[neighbours[4].x, neighbours[4].y] == 0 && floor[neighbours[5].x, neighbours[5].y] == 0 && floor[neighbours[6].x, neighbours[6].y] == 0)
            return true;
        else if (floor[neighbours[6].x, neighbours[6].y] == 0 && floor[neighbours[7].x, neighbours[7].y] == 0 && floor[neighbours[0].x, neighbours[0].y] == 0)
            return true;
        else if (floor[neighbours[0].x, neighbours[0].y] > 2 && floor[neighbours[1].x, neighbours[1].y] == 0 && floor[neighbours[2].x, neighbours[2].y] > 2)
            return true;
        else if (floor[neighbours[2].x, neighbours[2].y] > 2 && floor[neighbours[3].x, neighbours[3].y] == 0 && floor[neighbours[4].x, neighbours[4].y] > 2)
            return true;
        else if (floor[neighbours[4].x, neighbours[4].y] > 2 && floor[neighbours[5].x, neighbours[5].y] == 0 && floor[neighbours[6].x, neighbours[6].y] > 2)
            return true;
        else if (floor[neighbours[6].x, neighbours[6].y] > 2 && floor[neighbours[7].x, neighbours[7].y] == 0 && floor[neighbours[0].x, neighbours[0].y] > 2)
            return true;
        else if (floor[neighbours[0].x, neighbours[0].y] == 2 && floor[neighbours[1].x, neighbours[1].y] == 0 && floor[neighbours[2].x, neighbours[2].y] == 0)
            return true;
        else if (floor[neighbours[2].x, neighbours[2].y] == 2 && floor[neighbours[3].x, neighbours[3].y] == 0 && floor[neighbours[4].x, neighbours[4].y] == 0)
            return true;
        else if (floor[neighbours[4].x, neighbours[4].y] == 2 && floor[neighbours[5].x, neighbours[5].y] == 0 && floor[neighbours[6].x, neighbours[6].y] == 0)
            return true;
        else if (floor[neighbours[6].x, neighbours[6].y] == 2 && floor[neighbours[7].x, neighbours[7].y] == 0 && floor[neighbours[0].x, neighbours[0].y] == 0)
            return true;
        else if (floor[neighbours[0].x, neighbours[0].y] == 0 && floor[neighbours[1].x, neighbours[1].y] == 0 && floor[neighbours[2].x, neighbours[2].y] == 2)
            return true;
        else if (floor[neighbours[2].x, neighbours[2].y] == 0 && floor[neighbours[3].x, neighbours[3].y] == 0 && floor[neighbours[4].x, neighbours[4].y] == 2)
            return true;
        else if (floor[neighbours[4].x, neighbours[4].y] == 0 && floor[neighbours[5].x, neighbours[5].y] == 0 && floor[neighbours[6].x, neighbours[6].y] == 2)
            return true;
        else if (floor[neighbours[6].x, neighbours[6].y] == 0 && floor[neighbours[7].x, neighbours[7].y] == 0 && floor[neighbours[0].x, neighbours[0].y] == 2)
            return true;
        return false;   
    }

    public Vector2Int[] GetNeighbours(Vector2Int coord)
    {
        if (IsAnEdge(coord)) return new Vector2Int[0];
        Vector2Int[] neighbours = new Vector2Int[8];
        neighbours[0] = new Vector2Int(coord[0] - 1, coord[1]); // Left
        neighbours[1] = new Vector2Int(coord[0] - 1, coord[1] + 1); // Top-Left
        neighbours[2] = new Vector2Int(coord[0], coord[1] + 1); // Top
        neighbours[3] = new Vector2Int(coord[0] + 1, coord[1] + 1); // Top-Right
        neighbours[4] = new Vector2Int(coord[0] + 1, coord[1]); // Right
        neighbours[5] = new Vector2Int(coord[0] + 1, coord[1] - 1); // Bottom-Right
        neighbours[6] = new Vector2Int(coord[0], coord[1] - 1); // Bottom
        neighbours[7] = new Vector2Int(coord[0] - 1, coord[1] - 1); // Bottom-Left
        return neighbours;
    }

    public bool IsAnEdge(Vector2Int coord)
    {
        return (coord[0] == width - 1 || coord[1] == height - 1 || coord[0] == 0 || coord[1] == 0);
    }

    public bool IsFree(Vector2Int coord) // Costly
    {
		foreach (Corridor corridor in this.corridors)
			if (corridor.IsIn (coord))
				return false;
        return true;
    }

	public Corridor GetCorridorFromCoord(Vector2Int coord){
		foreach (Corridor corridor in corridors)
			if (corridor.IsIn (coord))
				return corridor;
		return null;
	}

    public Vector2Int MoveForward(Vector2Int coord, Direction dir)
    {
        switch (dir)
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

    public Direction TurnLeft(Direction dir)
    {
        switch (dir)
        {
            case Direction.LEFT:
                return Direction.DOWN;
            case Direction.UP:
                return Direction.LEFT;
            case Direction.RIGHT:
                return Direction.UP;
            case Direction.DOWN:
                return Direction.RIGHT;
            default:
                return dir;
        }
    }

    public Direction TurnRight(Direction dir)
    {
        switch (dir)
        {
            case Direction.LEFT:
                return Direction.UP;
            case Direction.UP:
                return Direction.RIGHT;
            case Direction.RIGHT:
                return Direction.DOWN;
            case Direction.DOWN:
                return Direction.LEFT;
            default:
                return dir;
        }
    }
}
