using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon
{
    public uint height, width;
    public uint tile_size;

    public List<Corridor> corridors;
    public List<Room> rooms;

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
        uint[,] floor = f.GetFloor();

        /* Corridors */
        AddCorridors(floor);

        /* Rooms */
        AddRooms(floor, f.GetNumberOfRooms());
        
    }

    /* Main functions */

    public void AddCorridors(uint[,] floor)
    {
        int height = floor.GetLength(0), width = floor.GetLength(1);
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
                    if (floor[i + 1, j - 1] == 1 && floor[i + 2, j] == 0 && floor[i + 1, j + 1] == 0) corridor.length++;
                    corridors.Add(corridor);
                }
            }
        }
    }

    public void AddRooms(uint[,] floor, uint n_rooms)
    {
        int height = floor.GetLength(0), width = floor.GetLength(1);
        Room[] r = new Room[n_rooms * 10];
        for (uint t = 0; t < n_rooms * 10; t++) r[t] = new Room(new Vector2Int(0, 0), new List<Vector2Int>());
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
                        r[(int)floor[neighbours[3].x, neighbours[2].y] - 3].door = new Vector2Int(i, j);
                    else if (floor[neighbours[4].x, neighbours[4].y] > 2)
                        r[(int)floor[neighbours[4].x, neighbours[4].y] - 3].door = new Vector2Int(i, j);
                    else if (floor[neighbours[6].x, neighbours[6].y] > 2)
                        r[(int)floor[neighbours[6].x, neighbours[6].y] - 3].door = new Vector2Int(i, j);
                }
            }
        }
        for (uint t = 0; t < n_rooms * 10; t++)
        {
            if (r[t].corners.Count > 3) this.rooms.Add(r[t]);
        }
    }
    

    /* Auxiliary functions */

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
            if (corridor.IsIn(coord)) return false;
        return true;
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
