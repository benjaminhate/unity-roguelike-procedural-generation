using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor {
    public uint height, width;
    public uint max_corridor;
    private uint gap_between_bifurcations = 6;

    public List<Corridor> corridors;
    public List<Room> rooms;

    public Floor(uint height = 50, uint width = 50, uint max_corridor = 20)
    {
        /* Initialisation of attributes */
        this.height = height;
        this.width = width;
        this.max_corridor = max_corridor;

        corridors = new List<Corridor>();
        rooms = new List<Room>();

        /* Selection of starting point and direction */
        Vector2Int coord = new Vector2Int((int) Random.Range(width / 4, 3 * width / 4), 1);
        Direction dir = Direction.UP;

        /* Corridors */
        AddCorridors(coord, dir, 0);

        /* Rooms */
    }

    /* Main functions */

    public void AddCorridors(Vector2Int coord, Direction dir, uint last_bifurcation)
    {
        Corridor corridor = new Corridor(coord, dir);

        while (!IsAnEdge(coord) /*&& IsFree(coord)*/)
        {
            uint choice = GetCase();
            if (choice < 4 && corridors.Count < max_corridor && last_bifurcation > gap_between_bifurcations)
            {
                Direction new_dir = (choice == 0 || choice == 2) ? TurnLeft(dir) : TurnRight(dir);
                AddCorridors(MoveForward(coord, new_dir), new_dir, 0);
                if (choice < 2) AddCorridors(MoveForward(coord, dir), dir, 0);
                this.corridors.Add(corridor);
                return;
            }
            coord = MoveForward(coord, dir);
            corridor.length++;
            last_bifurcation++;
        }
        this.corridors.Add(corridor);
    }

    /* Auxiliary functions */

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

    /*public Vector2Int MoveForward(Vector2Int coord, Direction dir)
    {
        if (dir <= Direction.DOWN && dir >= Direction.LEFT)
            coord += (dir != Direction.DOWN) ? new Vector2Int((int) dir - 1, (int) dir % 2) : new Vector2Int(0, -1);
        return coord;
    }

    public Direction Turn(Direction original_dir, Direction turn_direction)
    {
        if (turn_direction == Direction.DOWN || turn_direction == Direction.UP) return original_dir;
        int way = (turn_direction == Direction.LEFT) ? -1 : 1;
        return (Direction) (((int) original_dir + way) % 4);
    */

    public uint GetCase()
    {
        float proba = Random.Range(0.0F, 1.0F);
        if (proba < 0.035F) // Splits in two paths : one left, and one in the same direction
            return 0;
        else if (proba < 0.07F)  // Splits in two paths : one right, and one in the same direction
            return 1;
        else if (proba < 0.085F) // Turns left
            return 2;
        else if (proba < 0.1F) // Turns right
            return 3;
        else // Continues forward
            return 4;
    }
}
