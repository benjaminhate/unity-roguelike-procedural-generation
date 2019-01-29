using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Floor
{
    /* Parameters */
    private uint height, width;
    private uint n_rooms;
    private uint[,] floor;

    public uint[,] GetFloor()
    {
        return this.floor;
    }

    public uint GetNumberOfRooms()
    {
        return this.n_rooms;
    }
         
    public Floor(uint height = 50, uint width = 50)
    {
        this.height = height;
        this.width = width;

        // TODO
        /*
         * Assert than <wall> and <ground> sizes are the same
         * Extract their size to initialise tile_size
         */

        floor = new uint[width, height]; // Values initialised at 0 by default
        max_path = (uint)(Mathf.Sqrt(width * height) / 4);
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
				if (labels.Length == 0)
					break;
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
		uint[] room_surfaces = new uint[label - 3 + 1];
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
				if (floor [i, j] > 2 && floor [i, j] - 3 < room_surfaces.Length)
					room_surfaces [floor [i, j] - 3] += 1;
            }
        }

        // Removing rooms that are too small
        /* Using arbitrary value */
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
				if (floor [i, j] > 2 && floor [i, j] - 3 < room_surfaces.Length)
				if (room_surfaces [floor [i, j] - 3] < Mathf.Min (width, height) / 2.0f
				    || room_surfaces [floor [i, j] - 3] > Mathf.Max (width, height) * 2.0f)
					floor [i, j] = 0;
            }
        }

        this.n_rooms = label - 3;
        for (uint cpt = 0; cpt < label - 3; cpt++)
        {
            if (room_surfaces[cpt] < Mathf.Min(width, height)) this.n_rooms--;
        }

        // Compute each room wall's length (only tiles next to corridor)
		uint[] wall_surfaces = new uint[label - 3 + 1];
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (floor[i, j] == 0)
                {
                    uint[] labels = GetCloseNeighboursLabels(new Vector2Int(i, j));
					if (labels.Length == 0)
						break;
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
        bool[] door_created_labels = new bool[label - 3 + 1]; // Every element is set to false by default
        uint[] visited_surfaces = new uint[label - 3 + 1];
        float proba = 1.3F;
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (floor[i, j] == 0 && !IsWallCorner(new Vector2Int(i, j)))
                {
                    uint[] labels = GetCloseNeighboursLabels(new Vector2Int(i, j));
					if (labels.Length == 0)
						break;
                    /* Probability should depend on wall's length, and a door should be added in all cases */
                    if (labels[(int)Direction.LEFT] > 2 && !door_created_labels[labels[(int)Direction.LEFT] - 3])
                    {
                        if (Random.Range(0.0F, 1.0F) < proba / wall_surfaces[labels[(int)Direction.LEFT] - 3] || visited_surfaces[labels[(int)Direction.LEFT] - 3] >= wall_surfaces[labels[(int)Direction.LEFT] - 3] - 1)
                        {
                            floor[i, j] = 2;
                            door_created_labels[labels[(int)Direction.LEFT] - 3] = true;
                        }
                        visited_surfaces[labels[(int)Direction.LEFT] - 3] += 1;
                    }
                    else if (labels[(int)Direction.UP] > 2 && !door_created_labels[labels[(int)Direction.UP] - 3])
                    {
                        if (Random.Range(0.0F, 1.0F) < proba / wall_surfaces[labels[(int)Direction.UP] - 3] || visited_surfaces[labels[(int)Direction.UP] - 3] >= wall_surfaces[labels[(int)Direction.UP] - 3] - 1)
                        {
                            floor[i, j] = 2;
                            door_created_labels[labels[(int)Direction.UP] - 3] = true;
                        }
                        visited_surfaces[labels[(int)Direction.UP] - 3] += 1;
                    }
                    else if (labels[(int)Direction.RIGHT] > 2 && !door_created_labels[labels[(int)Direction.RIGHT] - 3])
                    {
                        if (Random.Range(0.0F, 1.0F) < proba / wall_surfaces[labels[(int)Direction.RIGHT] - 3] || visited_surfaces[labels[(int)Direction.RIGHT] - 3] >= wall_surfaces[labels[(int)Direction.RIGHT] - 3] - 1)
                        {
                            floor[i, j] = 2;
                            door_created_labels[labels[(int)Direction.RIGHT] - 3] = true;
                        }
                        visited_surfaces[labels[(int)Direction.RIGHT] - 3] += 1;
                    }
                    else if (labels[(int)Direction.DOWN] > 2 && !door_created_labels[labels[(int)Direction.DOWN] - 3])
                    {
                        if (Random.Range(0.0F, 1.0F) < proba / wall_surfaces[labels[(int)Direction.DOWN] - 3] || visited_surfaces[labels[(int)Direction.DOWN] - 3] >= wall_surfaces[labels[(int)Direction.DOWN] - 3] - 1)
                        {
                            floor[i, j] = 2;
                            door_created_labels[labels[(int)Direction.DOWN] - 3] = true;
                        }
                        visited_surfaces[labels[(int)Direction.DOWN] - 3] += 1;
                    }

                }
            }
        }

        // Fill dead-ends : Limitations -> only detect 1 tile large corridors dead-ends
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (IsDeadEnd(new Vector2Int(i, j)))
                {
					uint[] labels = GetCloseNeighboursLabels (new Vector2Int (i, j));
                    if (labels[0] == 1) // Left is free
                    {
                        /*int k = i;
						uint[] internLabels = GetCloseNeighboursLabels (new Vector2Int (k, j));
						while (internLabels.Length > 0 && internLabels [1] == 0 && internLabels [3] == 0 && k > 1) {
							floor [k, j] = 0;
							k -= 1;
							internLabels = GetCloseNeighboursLabels (new Vector2Int (k, j));
						}*/
						FillDeadEnd (new Vector2Int (i, j), -1, 1, true, 1);
                    }
					else if (labels[2] == 1) // Right is free
                    {
                        /*int k = i;
                        while (GetCloseNeighboursLabels(new Vector2Int(k, j))[1] == 0 && GetCloseNeighboursLabels(new Vector2Int(k, j))[3] == 0 && k < width - 1)
                        {
                            floor[k, j] = 0;
                            k += 1;
                        }*/
						FillDeadEnd (new Vector2Int (i, j), 1, 1, false, (int)width - 1);
                    }
					else if (labels[1] == 1) // Top is free
                    {
                        /*int l = j;
                        while (GetCloseNeighboursLabels(new Vector2Int(i, l))[0] == 0 && GetCloseNeighboursLabels(new Vector2Int(i, l))[2] == 0 && l < height - 1)
                        {
                            floor[i, l] = 0;
                            l += 1;
                        }*/
						FillDeadEnd (new Vector2Int (i, j), 1, 0, false, (int)height - 1);
                    }
					else if (labels[3] == 1) // Bottom is free
                    {
                        int l = j;
                        while (GetCloseNeighboursLabels(new Vector2Int(i, l))[0] == 0 && GetCloseNeighboursLabels(new Vector2Int(i, l))[2] == 0 && l > 1)
                        {
                            floor[i, l] = 0;
                            l -= 1;
                        }
						FillDeadEnd (new Vector2Int (i, j), -1, 0, true, 1);
                    }
                }
            }
        }
    }

	void FillDeadEnd(Vector2Int start, int addVal, int indexModif, bool sup, int supVal){
		uint[] internLabels = GetCloseNeighboursLabels (start);
		while (internLabels.Length > 0 && internLabels [indexModif] == 0 && internLabels [indexModif + 2] == 0
		       && ((sup && start [1 - indexModif] > supVal) || (!sup && start [1 - indexModif] < supVal))) {
			floor [start.x, start.y] = 0;
			start [1 - indexModif] += addVal;
			internLabels = GetCloseNeighboursLabels (start);
		}
	}

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

    public bool IsNearCorridor(Vector2Int coord, Direction dir)
    {
        bool isNear = false;
        switch(dir)
        {
            case Direction.LEFT:
                isNear = floor[coord[0] - 1, coord[1]] == 1;  
                isNear |= floor[coord[0], coord[1] - 1] == 1;
                isNear |= floor[coord[0], coord[1] + 1] == 1;
                isNear |= floor[coord[0] + 1, coord[1] - 1] == 1;
                isNear |= floor[coord[0] + 1, coord[1] + 1] == 1;
                break;
            case Direction.UP:
                isNear = floor[coord[0], coord[1] + 1] == 1;
                isNear |= floor[coord[0] - 1, coord[1]] == 1;
                isNear |= floor[coord[0] + 1, coord[1]] == 1;
                isNear |= floor[coord[0] - 1, coord[1] - 1] == 1;
                isNear |= floor[coord[0] + 1, coord[1] - 1] == 1;
                break;
            case Direction.RIGHT:
                isNear = floor[coord[0] + 1, coord[1]] == 1;
                isNear |= floor[coord[0], coord[1] - 1] == 1;
                isNear |= floor[coord[0], coord[1] + 1] == 1;
                isNear |= floor[coord[0] - 1, coord[1] - 1] == 1;
                isNear |= floor[coord[0] - 1, coord[1] + 1] == 1;
                break;
            case Direction.DOWN :
                isNear = floor[coord[0], coord[1] - 1] == 1;
                isNear |= floor[coord[0] - 1, coord[1]] == 1;
                isNear |= floor[coord[0] + 1, coord[1]] == 1;
                isNear |= floor[coord[0] - 1, coord[1] + 1] == 1;
                isNear |= floor[coord[0] + 1, coord[1] + 1] == 1;
                break;
        }
        return isNear;
    }

    private uint max_path, n_path;
    private uint gap_between_bifurcations = 6;

    public void DrawCorridor(Vector2Int coord, Direction dir, uint last_bifurcation)
    {
        uint choice;
        while (!IsAnEdge(coord))
        {
            choice = GetCase();
            if (n_path < max_path && last_bifurcation > gap_between_bifurcations && !IsNearCorridor(coord, dir))
            {
                switch (choice)
                {
                    case 0:
                        DrawCorridor(coord, TurnLeft(dir), 0);
                        //DrawCorridor(coord, Turn(dir, Direction.LEFT), 0);
                        break;
                    case 1:
                        DrawCorridor(coord, TurnRight(dir), 0);
                        //DrawCorridor(coord, Turn(dir, Direction.RIGHT), 0);
                        break;
                    case 2:
                        dir = TurnLeft(dir);
                        //dir = Turn(dir, Direction.LEFT);
                        break;
                    case 3:
                        dir = TurnRight(dir);
                        //dir = Turn(dir, Direction.RIGHT);
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
		if (labels.Length == 0)
			return false;
		else if (labels [0] == 0 && labels [1] == 0 && labels [3] == 0)
			return true; // Left dead-end
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

    public bool IsWallCorner(Vector2Int coord)
    {
        if (floor[coord.x, coord.y] != 0) return false;
        Vector2Int[] neighbours = GetNeighbours(coord);
        if (floor[neighbours[0].x, neighbours[0].y] == 0 && floor[neighbours[2].x, neighbours[2].y] == 0 && floor[neighbours[1].x, neighbours[1].y] > 2)
            return true;
        else if (floor[neighbours[2].x, neighbours[2].y] == 0 && floor[neighbours[4].x, neighbours[4].y] == 0 && floor[neighbours[3].x, neighbours[3].y] > 2)
            return true;
        else if (floor[neighbours[4].x, neighbours[4].y] == 0 && floor[neighbours[6].x, neighbours[6].y] == 0 && floor[neighbours[5].x, neighbours[5].y] > 2)
            return true;
        else if (floor[neighbours[6].x, neighbours[6].y] == 0 && floor[neighbours[0].x, neighbours[0].y] == 0 && floor[neighbours[7].x, neighbours[7].y] > 2)
            return true;
        return false;
    }
}
