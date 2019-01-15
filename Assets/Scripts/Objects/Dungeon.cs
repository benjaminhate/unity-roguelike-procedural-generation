using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon : MonoBehaviour
{
    public uint height, width;
    public uint tile_size;
    public GameObject wall_tile, corridor_tile, room_tile, door_tile;

    public List<Corridor> corridors;
    public List<Room> rooms;

    void Start()
    {
        this.corridors = new List<Corridor>();
        this.rooms = new List<Room>();
        corridors.Clear();
        rooms.Clear();

        /* 2D array representing the level */
        Floor f = new Floor(height, width);
        uint[,] floor = f.GetFloor();

        /* Instantiation to debug */
        for (uint i = 0; i < width; i++)
        {
            for (uint j = 0; j < height; j++)
            {
                if (floor[i, j] == 0)
                    Instantiate(wall_tile, tile_size * new Vector3(i, j, 0), Quaternion.identity);
                else if (floor[i, j] == 1)
                    Instantiate(corridor_tile, tile_size * new Vector3(i, j, 0), Quaternion.identity);
                else if (floor[i, j] == 2)
                    Instantiate(door_tile, tile_size * new Vector3(i, j, 0), Quaternion.identity);
                else
                    Instantiate(room_tile, tile_size * new Vector3(i, j, 0), Quaternion.identity);
            }
        }

        /* Corridors */
        AddCorridors(floor);
        Debug.Log(corridors.Count + " corridors");

        /* Rooms */
        Debug.Log(f.GetNumberOfRooms() + " rooms");
        Room[] r = AddRooms(floor, f.GetNumberOfRooms());
        for (uint t = 0; t < f.GetNumberOfRooms(); t++)
        {
            if (r[t].corners.Count > 0) this.rooms.Add(r[t]);
        }
        Debug.Log(rooms.Count + " rooms");

        /*foreach(Corridor corridor in corridors)
        {
            Vector2Int pos = corridor.start;
            while (corridor.IsIn(pos))
            {
                Instantiate(corridor_tile, tile_size * new Vector3(pos[0], 0, pos[1]), Quaternion.identity);
                pos = MoveForward(pos, corridor.dir);
            }
        }*/
    }

    public Dungeon(uint tile_size, uint height = 50, uint width = 50)
    {
        /* Initialisation of attributes */
        this.height = height;
        this.width = width;
        this.tile_size = tile_size;

        this.corridors = new List<Corridor>();
        this.rooms = new List<Room>();

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
                    while (floor[i, j] == 1 && floor[i - 1, j + 1] != 1 && floor[i + 1, j + 1] != 1)
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
                    while (floor[i, j] == 1 && floor[i + 1, j + 1] != 1 && floor[i + 1, j - 1] != 1)
                    {
                        corridor.length += 1;
                        i += 1;
                    }
                    corridors.Add(corridor);
                }
            }
        }
    }

    public Room[] AddRooms(uint[,] floor, uint n_rooms)
    {
        int height = floor.GetLength(0), width = floor.GetLength(1);
        Room[] rooms = new Room[n_rooms * 10];
        for (uint t = 0; t < n_rooms * 10; t++) rooms[t] = new Room(new Vector2Int(0, 0), new List<Vector2Int>());
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (IsRoomCorner(floor, new Vector2Int(i, j)))
                    rooms[(int)floor[i, j] - 3].corners.Add(new Vector2Int(i, j));
                else if (floor[i, j] == 2)
                {
                    if (floor[i - 1, j] > 2)
                        rooms[(int)floor[i - 1, j] - 3].door = new Vector2Int(i, j);
                    else if (floor[i + 1, j] > 2)
                        rooms[(int)floor[i + 1, j] - 3].door = new Vector2Int(i, j);
                    else if (floor[i, j - 1] > 2)
                        rooms[(int)floor[i, j - 1] - 3].door = new Vector2Int(i, j);
                    else if (floor[i, j + 1] > 2)
                        rooms[(int)floor[i, j + 1] - 3].door = new Vector2Int(i, j);
                }
            }
        }
        return rooms;
    }
    

    /* Auxiliary functions */

    public bool IsRoomCorner(uint[,] floor, Vector2Int coord)
    {
        if (floor[coord[0], coord[1]] < 3) return false;
        if ((floor[coord[0] - 1, coord[1]] == 0 && floor[coord[0], coord[1] - 1] == 0) || (floor[coord[0] - 1, coord[1]] == 0 && floor[coord[0], coord[1] - 1] == 2) || (floor[coord[0] - 1, coord[1]] == 2 && floor[coord[0], coord[1] - 1] == 0))
            return true;
        else if ((floor[coord[0] - 1, coord[1]] == 0 && floor[coord[0], coord[1] + 1] == 0) || (floor[coord[0] - 1, coord[1]] == 0 && floor[coord[0], coord[1] + 1] == 2) || (floor[coord[0] - 1, coord[1]] == 2 && floor[coord[0], coord[1] + 1] == 0))
            return true;
        else if ((floor[coord[0], coord[1] + 1] == 0 && floor[coord[0] + 1, coord[1]] == 0) || (floor[coord[0], coord[1] + 1] == 0 && floor[coord[0] + 1, coord[1]] == 2) || (floor[coord[0], coord[1] + 1] == 2 && floor[coord[0] + 1, coord[1]] == 0))
            return true;
        else if ((floor[coord[0] + 1, coord[1]] == 0 && floor[coord[0], coord[1] - 1] == 0) || (floor[coord[0] + 1, coord[1]] == 0 && floor[coord[0], coord[1] - 1] == 2) || (floor[coord[0] + 1, coord[1]] == 2 && floor[coord[0], coord[1] - 1] == 0))
            return true;
        else if (floor[coord[0] - 1, coord[1]] > 2 && floor[coord[0], coord[1] - 1] > 2 && floor[coord[0] - 1, coord[1] - 1] == 0)
            return true;
        else if (floor[coord[0] - 1, coord[1]] > 2 && floor[coord[0], coord[1] + 1] > 2 && floor[coord[0] - 1, coord[1] + 1] == 0)
            return true;
        else if (floor[coord[0], coord[1] + 1] > 2 && floor[coord[0] + 1, coord[1]] > 2 && floor[coord[0] + 1, coord[1] + 1] == 0)
            return true;
        else if (floor[coord[0] + 1, coord[1]] > 2 && floor[coord[0], coord[1] - 1] > 2 && floor[coord[0] + 1, coord[1] - 1] == 0)
            return true;

        return false;   
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
