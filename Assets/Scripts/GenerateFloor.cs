using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

//public enum Direction { LEFT = 0, UP, RIGHT, DOWN };

public class GenerateFloor : MonoBehaviour
{
    /* Parameters */
    public uint height = 50, width = 50;
    public GameObject wall_tile, corridor_tile, room_tile, door_tile;

    private uint[,] floor;
    private uint tile_size = 2; // Default size

    /* Useful auxiliary functions */

    public bool IsAnEdge(Vector2Int coord)
    {
        return (coord[0] == width - 1 || coord[1] == height - 1 || coord[0] == 0 || coord[1] == 0);
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
                return Direction.LEFT;
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
                return Direction.RIGHT;
        }
    }

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

    private uint max_path, n_path;
    private uint gap_between_bifurcations = 6;

    public void DrawCorridor(Vector2Int coord, Direction dir, uint last_bifurcation)
    {
        uint choice;
        while (!IsAnEdge(coord))
        {
            choice = GetCase();
            if (n_path < max_path && last_bifurcation > gap_between_bifurcations)
            {
                switch (choice)
                {
                    case 0:
                        DrawCorridor(coord, TurnLeft(dir), 0);
                        break;
                    case 1:
                        DrawCorridor(coord, TurnRight(dir), 0);
                        break;
                    case 2:
                        dir = TurnLeft(dir);
                        break;
                    case 3:
                        dir = TurnRight(dir);
                        break;
                }
                if (choice != 4)
                {
                    last_bifurcation = 0;
                    n_path += 1;
                }
            }

            coord = MoveForward(coord, dir);
            floor[coord[0], coord[1]] = 1;
            last_bifurcation += 1;
        }
        floor[coord[0], coord[1]] = 0;
    }

    public bool HasGroundNeighbour(Vector2Int coord)
    {
        if (IsAnEdge(coord)) return true;
        bool res = (floor[coord[0] - 1, coord[1]] == 1);
        res |= (floor[coord[0] - 1, coord[1] + 1] == 1);
        res |= (floor[coord[0], coord[1] + 1] == 1);
        res |= (floor[coord[0] + 1, coord[1] + 1] == 1);
        res |= (floor[coord[0] + 1, coord[1]] == 1);
        res |= (floor[coord[0] + 1, coord[1] - 1] == 1);
        res |= (floor[coord[0], coord[1] - 1] == 1);
        res |= (floor[coord[0] - 1, coord[1] - 1] == 1);
        return res;
    }

    public uint[] GetCloseNeighboursLabels(Vector2Int coord)
    {
        if (IsAnEdge(coord)) return new uint[0];
        uint[] labels = new uint[4];
        labels[0] = floor[coord[0] - 1, coord[1]]; // Left
        labels[1] = floor[coord[0], coord[1] + 1]; // Top
        labels[2] = floor[coord[0] + 1, coord[1]]; // Right
        labels[3] = floor[coord[0], coord[1] - 1]; // Bottom
        return labels;
    }

    public bool IsDeadEnd(Vector2Int coord)
    {
        if (floor[coord[0], coord[1]] != 1) return false; // Only a corridor can be a dead-end
        uint[] labels = GetCloseNeighboursLabels(coord);
        if (labels[0] == 0 && labels[1] == 0 && labels[3] == 0) return true; // Left dead-end
        else if (labels[0] == 0 && labels[1] == 0 && labels[2] == 0) return true; // Top dead-end
        else if (labels[1] == 0 && labels[2] == 0 && labels[3] == 0) return true; // Right dead-end
        else if (labels[0] == 0 && labels[3] == 0 && labels[2] == 0) return true; // Bottom dead-end
        else return false;
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

    public bool IsCloseToRoom(Vector2Int coord)
    { 
        if (IsAnEdge(coord)) return false;
        Vector2Int[] neighbours = GetNeighbours(coord);
        for (uint i = 0; i < 8; i++)
        {
            if (floor[neighbours[i][0], neighbours[i][1]] > 2) return true;
        }
        Vector2Int[] farther_neighbours = GetNeighbours(neighbours[1]);
        if (farther_neighbours.Length == 8)
            for (uint i = 0; i < 4; i++)
                if (floor[farther_neighbours[i][0], farther_neighbours[i][1]] > 2) return true;
        farther_neighbours = GetNeighbours(neighbours[3]);
        if (farther_neighbours.Length == 8)
            for (uint i = 1; i < 6; i++)
                if (floor[farther_neighbours[i][0], farther_neighbours[i][1]] > 2) return true;
        farther_neighbours = GetNeighbours(neighbours[5]);
        if (farther_neighbours.Length == 8)
            for (uint i = 4; i < 8; i++)
                if (floor[farther_neighbours[i][0], farther_neighbours[i][1]] > 2) return true;
        farther_neighbours = GetNeighbours(neighbours[7]);
        if (farther_neighbours.Length == 8)
        {
            for (uint i = 6; i < 8; i++)
                if (floor[farther_neighbours[i][0], farther_neighbours[i][1]] > 2) return true;
            if (floor[farther_neighbours[0][0], farther_neighbours[0][1]] > 2) return true;
        }
        return false;
    }

    // Use this for initialization
    void Start () {
        // TODO
        /*
         * Assert than <wall> and <ground> sizes are the same
         * Extract their size to initialise tile_size
         */

        floor = new uint[width, height]; // Values initialised at 0 by default
        max_path = (uint) (Mathf.Sqrt(width * height) / 4);
        n_path = 0;

        // Corridor's creation

        /* Selection of starting point */
        // We choose lower edge by default, and not too close to the left or right side
        Vector2Int coord = new Vector2Int((int)Random.Range(width / 4, 3 * width / 4), 0);
        // Initial direction for digging
        Direction dir = Direction.UP; 

        /* Moving one tile up */
        floor[coord[0], coord[1]] = 0;
        coord = MoveForward(coord, dir);
        floor[coord[0], coord[1]] = 1;

        DrawCorridor(coord, dir, 0);

        // Rooms' creation
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!HasGroundNeighbour(new Vector2Int(i, j)))
                {
                    floor[i, j] = 2;
                }
            }
        }

        // Rooms' labellisation
        uint label = 3;
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (floor[i, j] <= 1) continue;
                uint[] labels = GetCloseNeighboursLabels(new Vector2Int(i, j));
                /* Only Left and Bottom neighbours matter */
                if (labels[(int)Direction.LEFT] <= 2 && labels[(int)Direction.DOWN] <= 2)
                {
                    floor[i, j] = label;
                    label += 1;
                }
                else if (labels[(int)Direction.LEFT] > 2 && labels[(int)Direction.DOWN] <= 2)
                {
                    floor[i, j] = labels[(int)Direction.LEFT];
                }
                else if (labels[(int)Direction.LEFT] <= 2 && labels[(int)Direction.DOWN] > 2)
                {
                    floor[i, j] = labels[(int)Direction.DOWN];
                }
                else if (labels[(int)Direction.LEFT] == labels[(int)Direction.DOWN])
                {
                    floor[i, j] = labels[(int)Direction.LEFT];
                } 
                else if (labels[(int)Direction.LEFT] != labels[(int)Direction.DOWN])
                {
                    int k = j;
                    while (floor[i, k] > 1)
                    {
                        floor[i, k] = labels[(int)Direction.LEFT];
                        k -= 1;
                    }
                    label -= 1;
                }
            }
        }

        // TODO : merge some rooms

        /* INFO : Number of rooms : label - 3 */
        // Compute each room's surface
        uint[] room_surfaces = new uint[label - 3];
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (floor[i, j] > 2) room_surfaces[floor[i, j] - 3] += 1;
            }
        }

        // Removing rooms that are too small
        /* Using arbitrary value */
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (floor[i, j] > 2 && room_surfaces[floor[i, j] - 3] < Mathf.Min(width, height)) floor[i, j] = 0;
            }
        }
        
        // Compute each room wall's length (only tiles next to corridor)
        uint[] wall_surfaces = new uint[label - 3];
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (floor[i, j] == 0)
                {
                    uint[] labels = GetCloseNeighboursLabels(new Vector2Int(i, j));
                    if (labels[(int)Direction.LEFT] > 2)
                    {
                        wall_surfaces[labels[(int)Direction.LEFT] - 3] += 1;
                    }
                    else if (labels[(int)Direction.UP] > 2)
                    {
                        wall_surfaces[labels[(int)Direction.UP] - 3] += 1;
                    }
                    else if (labels[(int)Direction.RIGHT] > 2)
                    {
                        wall_surfaces[labels[(int)Direction.RIGHT] - 3] += 1;
                    }
                    else if (labels[(int)Direction.DOWN] > 2)
                    {
                        wall_surfaces[labels[(int)Direction.DOWN] - 3] += 1;
                    }
                }
            }
        }

        // Doors' creation
        bool[] door_created_labels = new bool[label - 3]; // Every element is set to false by default
        uint[] visited_surfaces = new uint[label - 3];
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (floor[i, j] == 0)
                {
                    uint[] labels = GetCloseNeighboursLabels(new Vector2Int(i, j));
                    /* Probability should depend on wall's length, and a door should be added in all cases */
                    if (labels[(int)Direction.LEFT] > 2 && !door_created_labels[labels[(int)Direction.LEFT] - 3])
                    {
                        if (Random.Range(0.0F, 1.0F) < 1.0F / wall_surfaces[labels[(int)Direction.LEFT] - 3] || visited_surfaces[labels[(int)Direction.LEFT] - 3] >= wall_surfaces[labels[(int)Direction.LEFT] - 3] - 1)
                        {
                            floor[i, j] = 2;
                            door_created_labels[labels[(int)Direction.LEFT] - 3] = true;
                        }
                        visited_surfaces[labels[(int)Direction.LEFT] - 3] += 1;
                    }
                    else if (labels[(int)Direction.UP] > 2 && !door_created_labels[labels[(int)Direction.UP] - 3])
                    {
                        if (Random.Range(0.0F, 1.0F) < 1.0F / wall_surfaces[labels[(int)Direction.UP] - 3] || visited_surfaces[labels[(int)Direction.UP] - 3] >= wall_surfaces[labels[(int)Direction.UP] - 3] - 1)
                        {
                            floor[i, j] = 2;
                            door_created_labels[labels[(int)Direction.UP] - 3] = true;
                        }
                        visited_surfaces[labels[(int)Direction.UP] - 3] += 1;
                    }
                    else if (labels[(int)Direction.RIGHT] > 2 && !door_created_labels[labels[(int)Direction.RIGHT] - 3])
                    {
                        if (Random.Range(0.0F, 1.0F) < 1.0F / wall_surfaces[labels[(int)Direction.RIGHT] - 3] || visited_surfaces[labels[(int)Direction.RIGHT] - 3] >= wall_surfaces[labels[(int)Direction.RIGHT] - 3] - 1)
                        {
                            floor[i, j] = 2;
                            door_created_labels[labels[(int)Direction.RIGHT] - 3] = true;
                        }
                        visited_surfaces[labels[(int)Direction.RIGHT] - 3] += 1;
                    }
                    else if (labels[(int)Direction.DOWN] > 2 && !door_created_labels[labels[(int)Direction.DOWN] - 3])
                    {
                        if (Random.Range(0.0F, 1.0F) < 1.0F / wall_surfaces[labels[(int)Direction.DOWN] - 3] || visited_surfaces[labels[(int)Direction.DOWN] - 3] >= wall_surfaces[labels[(int)Direction.DOWN] - 3] - 1)
                        {
                            floor[i, j] = 2;
                            door_created_labels[labels[(int)Direction.DOWN] - 3] = true;
                        }
                        visited_surfaces[labels[(int)Direction.DOWN] - 3] += 1;
                    }
                    
                }
            }
        }

        // Fill dead-ends : Limiations -> only detect 1 tile large corridors dead-ends
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (IsDeadEnd(new Vector2Int(i, j)))
                {
                    if (GetCloseNeighboursLabels(new Vector2Int(i, j))[0] == 1) // Left is free
                    {
                        int k = i;
                        while (GetCloseNeighboursLabels(new Vector2Int(k, j))[1] == 0 && GetCloseNeighboursLabels(new Vector2Int(k, j))[3] == 0 && k > 1)
                        {
                            floor[k, j] = 0;
                            k -= 1;
                        }
                    }
                    else if (GetCloseNeighboursLabels(new Vector2Int(i, j))[2] == 1) // Right is free
                    {
                        int k = i;
                        while (GetCloseNeighboursLabels(new Vector2Int(k, j))[1] == 0 && GetCloseNeighboursLabels(new Vector2Int(k, j))[3] == 0 && k < width - 1)
                        {
                            floor[k, j] = 0;
                            k += 1;
                        }
                    }
                    else if (GetCloseNeighboursLabels(new Vector2Int(i, j))[1] == 1) // Top is free
                    {
                        int l = j;
                        while (GetCloseNeighboursLabels(new Vector2Int(i, l))[0] == 0 && GetCloseNeighboursLabels(new Vector2Int(i, l))[2] == 0 && l < height - 1)
                        {
                            floor[i, l] = 0;
                            l += 1;
                        }
                    }
                    else if (GetCloseNeighboursLabels(new Vector2Int(i, j))[3] == 1) // Bottom is free
                    {
                        int l = j;
                        while (GetCloseNeighboursLabels(new Vector2Int(i, l))[0] == 0 && GetCloseNeighboursLabels(new Vector2Int(i, l))[2] == 0 && l > 1)
                        {
                            floor[i, l] = 0;
                            l -= 1;
                        }
                    }
                }
            }
        }

        // Floor's Instantiation
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

    }

    // Update is called once per frame
    void Update () {	
	}
}
