using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateLevel : MonoBehaviour {
    /* Parameters */
    public uint tile_size;
    public int height = 50, width = 50;
	private float base_size = 3.2f;
    public GameObject corridor_tile, room_tile, door_tile, wall_tile;

	public GameObject guardPrefab;
	public Canvas canvas;
	public GameObject endTextPrefab;
	public GameObject guardAbsorbBar;
	[Range(0,1)]
	public float difficulty = 0.1f;

	public GameObject endPrefab;
	public GameObject playerPrefab;
	public GameObject staminaBarPrefab;

	private Dungeon dungeon;

	// Use this for initialization
	void Start () {
		if (dungeon == null && transform.childCount == 0) {
			GenerateDungeon ();
		}
	}

	public IEnumerator ResetDungeon(){
		ClearDungeon ();
		yield return new WaitForSeconds (1f);
		GenerateDungeon ();
	}

	public void ResetDungeonVoid(bool editorMode = false){
		ClearDungeon (editorMode);
		GenerateDungeon ();
	}

	public void AddDifficulty(float addDifficulty){
		difficulty += addDifficulty;
		if (difficulty > 1)
			difficulty = 1;
		if (difficulty < 0)
			difficulty = 0;
	}

	public void ClearDungeon(bool editorMode = false){
		for (int i = transform.childCount - 1; i >= 0; i--) {
			DestroyCustom (transform.GetChild (i).gameObject, editorMode);
		}
		for (int i = canvas.transform.childCount - 1; i >= 0; i--) {
			DestroyCustom (canvas.transform.GetChild (i).gameObject, editorMode);
		}
		DestroyCustom (Camera.main.GetComponent<MainCameraController> (), editorMode);
		dungeon = null;
	}

	void DestroyCustom(Object obj,bool editorMode = false){
		if (editorMode) {
			DestroyImmediate (obj);
		} else {
			Destroy (obj);
		}
	}

	public void GenerateDungeon () {
		do {
			dungeon = new Dungeon ((uint)height, (uint)width);
		} while(dungeon.corridors.Count > difficulty * 30f || dungeon.corridors.Count < 2
		        || dungeon.rooms.Count < 2 || dungeon.rooms.Count > difficulty * 20f);

        // Removing non squared rooms
        Room[] rooms = new Room[dungeon.rooms.Count];
        dungeon.rooms.CopyTo(rooms);
        foreach (Room room in rooms) // Removing non squared rooms
        {
            //if (room.corners.Count != 4) dungeon.rooms.Remove(room);
        }

        //InstantiateCorridors(dungeon.corridors);
        //InstantiateRooms(dungeon.rooms);

        Debug.Log(dungeon.corridors.Count + " corridors");
        Debug.Log(dungeon.rooms.Count + " rooms");

        //InstantiateWalls(dungeon.corridors, dungeon.rooms);

        // Floor's Instantiation
        for (int i = 1; i < width - 1; i++)
        {
            // j == 0
            if (dungeon.floor[i - 1, 0] != 0 || dungeon.floor[i + 1, 0] != 0 || dungeon.floor[i, 1] != 0)
                InstantiateTile(wall_tile, tile_size, new Vector3(i, 0, 0), Quaternion.identity);
            // j == height - 1
            if (dungeon.floor[i - 1, height - 1] != 0 || dungeon.floor[i + 1, height - 1] != 0 || dungeon.floor[i, height - 2] != 0)
                InstantiateTile(wall_tile, tile_size, new Vector3(i, height - 1, 0), Quaternion.identity);
        }
        for (int j = 1; j < height - 1; j++)
        {
            // i == 0
            if (dungeon.floor[0, j + 1] != 0 || dungeon.floor[0, j - 1] != 0 || dungeon.floor[1, j] != 0)
                InstantiateTile(wall_tile, tile_size, new Vector3(0, j, 0), Quaternion.identity);
            // i == width - 1
            if (dungeon.floor[width - 1, j + 1] != 0 || dungeon.floor[width - 1, j + 1] != 0 || dungeon.floor[width - 2, j] != 0)
                InstantiateTile(wall_tile, tile_size, new Vector3(width - 1, j, 0), Quaternion.identity);
        }

        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
				if (dungeon.floor [i, j] == 0 && (dungeon.floor [i + 1, j] != 0 || dungeon.floor [i - 1, j] != 0 || dungeon.floor [i, j + 1] != 0 || dungeon.floor [i, j - 1] != 0))
					InstantiateTile(wall_tile, tile_size, new Vector3(i, j, 0), Quaternion.identity);
                else if (dungeon.floor[i, j] == 1)
                    InstantiateTile(corridor_tile, tile_size, new Vector3(i, j, 0), Quaternion.identity);
                else if (dungeon.floor[i, j] == 2)
                    InstantiateTile(door_tile, tile_size, new Vector3(i, j, 0), Quaternion.identity);
                else if (dungeon.floor[i, j] > 2)
                    InstantiateTile(room_tile, tile_size, new Vector3(i, j, 0), Quaternion.identity);
            }
        }

		InstantiateGuards (dungeon);
		SpawnPlayerAndEnd (dungeon);
    }

	void InstantiateTile(GameObject prefab, uint size, Vector3 position, Quaternion rotation){
		GameObject tile = Instantiate (prefab, size * position, rotation, transform);
		tile.transform.localScale = new Vector3 (
			(tile.transform.localScale.x * size) / base_size,
			(tile.transform.localScale.y * size) / base_size,
			(tile.transform.localScale.z * size) / base_size
		);
	}

	void InstantiateGuards(Dungeon dungeon){
		RandomIndexPicker<Corridor> corridorPicker = new RandomIndexPicker<Corridor> (dungeon.corridors);
		int guardNum = Mathf.FloorToInt (difficulty * 5f * corridorPicker.Count ());
		for (int i = 0; i < guardNum; i++) {
			if (corridorPicker.IsEmpty ())
				break;
			
			// Generate random values
			int randomState = Random.Range (0, 10);
			//int randomState = 0;
			if (randomState > 2)
				randomState = 0;
			StartingState state = (StartingState)randomState;
			float idle = Random.value * Random.Range (1, 5) + 1f;
			Characteristics chara = Characteristics.random (difficulty);
			Corridor corridor = corridorPicker.Pick ();

			// Instantiate guard
			GameObject guard = Instantiate (guardPrefab, tile_size * new Vector3 (corridor.start.x, corridor.start.y, 0), Quaternion.identity, transform);

			// Edit guardController values
			GuardController guardController = guard.GetComponent<GuardController> ();
			guardController.startingState = state;
			guardController.idleDuration = idle;
			guardController.innerState.characteristics = chara;

			// Patroling state
			if (state == StartingState.Patrol) {
				GuardBuildWaypoints (dungeon, guardController, corridor, tile_size);
			}

			// Absorb bar
			GameObject absorbBar = Instantiate (guardAbsorbBar, canvas.transform);
			AbsorbBar absorbScript = guard.GetComponent<AbsorbBar> ();
			absorbScript.bar = absorbBar.GetComponent<RectTransform> ();
			absorbScript.targetCanvas = canvas.GetComponent<RectTransform> ();
			absorbScript.maxAmout = 1;
		}
	}

	void GuardBuildWaypoints(Dungeon dungeon, GuardController guard, Corridor startCorridor, float size){
		guard.waypoints.Clear ();
		Vector2 start = size * new Vector2 (startCorridor.start.x, startCorridor.start.y);
		Vector2 next = start;
		guard.waypoints.Add (new Waypoint (next));
		Vector2Int nextPos = startCorridor.start;
		int dist = 0;
		while (dungeon.floor [nextPos.x, nextPos.y] == 1) {
			nextPos = startCorridor.Forward (startCorridor.start, ++dist);
		}
		nextPos = startCorridor.Forward (startCorridor.start, dist - 1);
		//Corridor nextCorridor = dungeon.GetCorridorFromCoord (nextPos);
		//if (nextCorridor != null) {
		next = size * new Vector2 (nextPos.x, nextPos.y);
		guard.waypoints.Add (new Waypoint (next));
		//}
	}

	void SpawnPlayerAndEnd(Dungeon dungeon){
		RandomIndexPicker<Room> roomPicker = new RandomIndexPicker<Room> (dungeon.rooms);
		Room startRoom = roomPicker.Pick ();
		Vector2 playerPos = startRoom.GetCenter ();
		SpawnPlayer (playerPos);
		Room endRoom = roomPicker.Pick ();
		Vector2 endPos = endRoom.GetCenter ();
		SpawnEnd (endPos);
	}

	void SpawnPlayer(Vector2 position){
		GameObject player = Instantiate (playerPrefab, tile_size * position, Quaternion.identity, transform);

		GameObject mainCamera = Camera.main.gameObject;
		mainCamera.AddComponent<MainCameraController> ();
		mainCamera.GetComponent<MainCameraController> ().target = player;
		mainCamera.GetComponent<MainCameraController> ().Setup ();

		GameObject staminaBar = Instantiate (staminaBarPrefab, canvas.transform);
		player.GetComponent<PlayerController> ().staminaBar = staminaBar.GetComponent<RectTransform> ();
		player.GetComponent<PlayerController> ().staminaDrain = difficulty * 20f;
	}

	void SpawnEnd(Vector2 position){
		GameObject end = Instantiate (endPrefab, tile_size * position, Quaternion.identity, transform);
		GameObject endText = Instantiate (endTextPrefab, canvas.transform);
		endText.SetActive (false);
		end.GetComponent<Finish> ().endText = endText;
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
        uint offset = 1;
        if (room.corners.Count == 4)
            offset = 2;
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
            
            corridor = new Corridor(room.corners[i] + pos, dir, (uint)Vector2Int.Distance(room.corners[i], room.corners[i + 1]) + offset);
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
        corridor = new Corridor(room.corners[room.corners.Count - 1] + pos, dir, (uint)Vector2Int.Distance(room.corners[room.corners.Count - 1], room.corners[0]) + offset);
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
