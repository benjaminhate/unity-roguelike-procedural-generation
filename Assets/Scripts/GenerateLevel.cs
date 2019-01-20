using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour {
    /* Parameters */
    public uint tile_size;
    public GameObject corridor_tile, room_tile, door_tile, wall_tile;

	// Use this for initialization
	void Start () {
        Dungeon dungeon = new Dungeon();

        // Removing non squared rooms
        Room[] rooms = new Room[dungeon.rooms.Count];
        dungeon.rooms.CopyTo(rooms);
        foreach (Room room in rooms)
        {
            if (room.corners.Count != 4) dungeon.rooms.Remove(room);
        }

        InstantiateCorridors(dungeon.corridors);
        InstantiateRooms(dungeon.rooms);

        Debug.Log(dungeon.corridors.Count + " corridors");
        Debug.Log(dungeon.rooms.Count + " rooms");

        InstantiateWalls(dungeon.corridors, dungeon.rooms);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void InstantiateCorridors(List<Corridor> corridors)
    {
        foreach(Corridor corridor in corridors)
        {
            InstantiateCorridor(corridor, corridor_tile);
        }
    }

    void InstantiateCorridor(Corridor corridor, GameObject tile, Room room = null)
    {
        Vector2Int position = corridor.start;
        while (Vector2Int.Distance(position, corridor.start) < corridor.length)
        {
            if (room == null || (room != null && room.door != position))
                Instantiate(tile, tile_size * new Vector3(position.x, position.y, 0), Quaternion.identity);
            position = corridor.Forward(position);
        }
    }

    void InstantiateRooms(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            if (room.corners.Count == 4)
                InstantiateSquareRoom(room);
            else
                InstantiateRoom(room);
        }
    }

    void InstantiateSquareRoom(Room room)
    {
        Instantiate(door_tile, tile_size * new Vector3(room.door.x, room.door.y, 0), Quaternion.identity);
        Corridor corridor = new Corridor(room.corners[0], GetDirection(room.corners[0], room.corners[1]), (uint)Vector2Int.Distance(room.corners[0], room.corners[1]) + 1);
        Vector2Int pos = new Vector2Int(0, 0);
        while (room.corners[0] + pos != room.corners[3])
        {
            corridor.start = room.corners[0] + pos;
            InstantiateCorridor(corridor, room_tile);
            pos.y -= 1;
        }
        corridor.start = room.corners[0] + pos;
        InstantiateCorridor(corridor, room_tile);
    }

    void InstantiateRoom(Room room)
    {
        Instantiate(door_tile, tile_size * new Vector3(room.door.x, room.door.y, 0), Quaternion.identity);

        Corridor corridor;
        for (int i = 0; i < room.corners.Count - 1; i++)
        {
            corridor = new Corridor(room.corners[i], GetDirection(room.corners[i], room.corners[i + 1]), (uint)Vector2Int.Distance(room.corners[i], room.corners[i + 1]));
            InstantiateCorridor(corridor, room_tile);
        }
        corridor = new Corridor(room.corners[room.corners.Count - 1], GetDirection(room.corners[room.corners.Count - 1], room.corners[0]), (uint)Vector2Int.Distance(room.corners[room.corners.Count - 1], room.corners[0]));
        InstantiateCorridor(corridor, room_tile);
    }

    void InstantiateWalls(List<Corridor> corridors, List<Room> rooms)
    {
        foreach (Corridor corridor in corridors)
        {
            //InstantiateCorridorWalls(corridor);
        }
        foreach(Room room in rooms)
        {
            InstantiateRoomWalls(room);
        }
    }

    void InstantiateCorridorWalls(Corridor corridor)
    {
        Vector2Int position = corridor.start;
        if (corridor.dir == Direction.LEFT || corridor.dir == Direction.RIGHT)
        {
            while (Vector2Int.Distance(position, corridor.start) < corridor.length - 1)
            {
                position = corridor.Forward(position);
                Instantiate(wall_tile, tile_size * new Vector3(position.x, position.y + 1, 0), Quaternion.identity);
                Instantiate(wall_tile, tile_size * new Vector3(position.x, position.y - 1, 0), Quaternion.identity);
            }
        }
        else if (corridor.dir == Direction.UP || corridor.dir == Direction.DOWN)
        {
            while (Vector2Int.Distance(position, corridor.start) < corridor.length - 1)
            {
                position = corridor.Forward(position);
                Instantiate(wall_tile, tile_size * new Vector3(position.x - 1, position.y, 0), Quaternion.identity);
                Instantiate(wall_tile, tile_size * new Vector3(position.x + 1, position.y, 0), Quaternion.identity);
            }
        }
    }

    void InstantiateRoomWalls(Room room)
    {
        Corridor corridor;
        Direction dir;
        Vector2Int pos;
        for (int i = 0; i < room.corners.Count - 1; i++)
        {
            dir = GetDirection(room.corners[i], room.corners[i + 1]);
            pos = new Vector2Int(0, 0);
            switch (dir)
            {
                case Direction.LEFT:
                    pos = new Vector2Int(0, 1);
                    break;
                case Direction.UP:
                    pos = new Vector2Int(1, 0);
                    break;
                case Direction.RIGHT:
                    pos = new Vector2Int(0, -1);
                    break;
                case Direction.DOWN:
                    pos = new Vector2Int(-1, 0);
                    break;
            }

            corridor = new Corridor(room.corners[i] + pos, dir, (uint)Vector2Int.Distance(room.corners[i], room.corners[i + 1]) + 2);
            InstantiateCorridor(corridor, wall_tile, room);
        }
        dir = GetDirection(room.corners[room.corners.Count - 1], room.corners[0]);
        pos = new Vector2Int(0, 0);
        switch (dir)
        {
            case Direction.LEFT:
                pos = new Vector2Int(0, 1);
                break;
            case Direction.UP:
                pos = new Vector2Int(1, 0);
                break;
            case Direction.RIGHT:
                pos = new Vector2Int(0, -1);
                break;
            case Direction.DOWN:
                pos = new Vector2Int(-1, 0);
                break;
        }
        corridor = new Corridor(room.corners[room.corners.Count - 1] + pos, dir, (uint)Vector2Int.Distance(room.corners[room.corners.Count - 1], room.corners[0]) + 2);
        InstantiateCorridor(corridor, wall_tile, room);
    }

    Direction GetDirection(Vector2Int start, Vector2Int end)
    {
        if (start.x < end.x && start.y == end.y)
            return Direction.RIGHT;
        else if (start.x >= end.x && start.y == end.y)
            return Direction.LEFT;
        else if (start.x == end.x && start.y < end.y)
            return Direction.UP;
        else if (start.x == end.x && start.y >= end.y)
            return Direction.DOWN;
        else
            return Direction.RIGHT;
    }
}
