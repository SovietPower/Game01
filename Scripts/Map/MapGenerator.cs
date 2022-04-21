using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 地图生成及地图信息管理。
public class MapGenerator : MonoBehaviour
{
	public Transform tilePrefab;
	public Transform obstaclePrefab;

	public Transform navmeshMaskPrefab; // 消除(遮盖)地图外的导航范围
	// 如果障碍的烘焙范围过大、可移动空间过小，可调高tileScale，或调低Navigation的AgentRadius（全局影响，默认0.5，过小会影响寻路）

	Transform mapFloor;
	Transform navmeshFloor; // 最大导航范围

	[Header ("地图列表")]
	public Map[] maps;
	public int mapIndex;
	Map curMap;
	System.Random rng; // Random Number Generator

	[Header ("地图样式")]
	public float tileScale = 1.5f; // 所有物体的缩放比
	public int maxN, maxM; // max size of map
	// ! 注意每当调整maxN, maxM, tileScale 时，都需重新bake以获取正确导航区域。通常不修改它们。

	[Range(0, 1)] public float tileContentPercent; // tile的大小占比

	// Else // ! 记得初始化
	bool[,] obstacleMap; // 障碍分布

	List<Coord> allCoords; // 所有坐标
	Queue<Coord> shuffledAllCoords; // 打乱后的所有坐标

	Tile[,] tileMap; // 所有tile

	List<Coord> allEmptyCoords; // 所有没有障碍的坐标
	Queue<Coord> shuffledEmptyCoords; // 打乱后的所有没有障碍的坐标

	Player player;
	Transform playerT;

	public void Start() // Start() 只调用一次。MapEditor的OnInspectorGUI()会执行多次
	{
		mapFloor = transform.Find("Map Floor");
		navmeshFloor = transform.Find("Navmesh Floor");

		FindObjectOfType<EnemySpawner>().OnNewLevel += OnNewLevel;

		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		playerT = player.transform;
	}

	#region Game
	void OnNewLevel(int levelNumber) // todo OnNewStage
	{
		mapIndex = levelNumber - 1;
		GenerateMap();
		ResetPlayerPosition();
	}

	public void ResetPlayerPosition()
	{
		playerT.transform.position = tileMap[curMap.spawnPoint.x, curMap.spawnPoint.y].transform.position + Vector3.up*4;
	}

	#endregion

	#region Generate
	public void GenerateMap()
	{
		curMap = maps[mapIndex];
		rng = new System.Random(curMap.seed);

		curMap.n = Mathf.Min(curMap.n, maxN); curMap.m = Mathf.Min(curMap.m, maxM);

		mapFloor.localScale = new Vector3(curMap.n * tileScale, curMap.m * tileScale, 0.1f); // 地面及碰撞体大小
		// GetComponent<BoxCollider>().size = new Vector3(curMap.n*tileScale, 0.5f, curMap.m*tileScale);

		GenerateNavmesh();
		GenerateShuffledCoords();
		GenerateTiles();
		GenerateObstacles();
	}

	// 生成最大导航网格，和导航网格蒙版(遮盖地图外的导航部分)
	public void GenerateNavmesh()
	{
		navmeshFloor.localScale = new Vector3(maxN, maxM, 0f) * tileScale; // 因为绕x旋转了90，所以此时z轴是原本的y轴高度，y轴是原本的z轴宽度

		string parentName = "Navmesh Masks";
		if (transform.Find(parentName)) // 销毁以前的所有格子
			DestroyImmediate(transform.Find(parentName).gameObject);

		Transform par = new GameObject(parentName).transform;
		par.parent = transform;

		// ((curMap.n+maxN)/4, 0, 0) 即为左右两侧mask的中心位置
		// 注意不能直接除2或除4，当n为奇数时会导致舍入，从而偏离中心
		Transform maskLeft = Instantiate(navmeshMaskPrefab, (curMap.n+maxN)*0.25f*tileScale*Vector3.left, Quaternion.identity);
		maskLeft.parent = par;
		maskLeft.localScale = new Vector3((maxN-curMap.n)*0.5f, 1, curMap.m) * tileScale;

		Transform maskRight = Instantiate(navmeshMaskPrefab, (curMap.n+maxN)*0.25f*tileScale*Vector3.right, Quaternion.identity);
		maskRight.parent = par;
		maskRight.localScale = new Vector3((maxN-curMap.n)*0.5f, 1, curMap.m) * tileScale;

		Transform maskTop = Instantiate(navmeshMaskPrefab, (curMap.m+maxM)*0.25f*tileScale*Vector3.forward, Quaternion.identity);
		maskTop.parent = par;
		maskTop.localScale = new Vector3(maxN, 1, (maxM-curMap.m)*0.5f) * tileScale;

		Transform maskBottom = Instantiate(navmeshMaskPrefab, (curMap.m+maxM)*0.25f*tileScale*Vector3.back, Quaternion.identity);
		maskBottom.parent = par;
		maskBottom.localScale = new Vector3(maxN, 1, (maxM-curMap.m)*0.5f) * tileScale;
	}

	// 生成随机坐标序列
	public void GenerateShuffledCoords()
	{
		allCoords = new List<Coord>();
		for (int x=0, n=curMap.n, m=curMap.m; x<n; ++x)
			for (int y=0; y<m; ++y)
				allCoords.Add(new Coord(x, y));
		shuffledAllCoords = new Queue<Coord>(MyTools.RandomShuffle(allCoords.ToArray(), curMap.seed));
	}

	// 生成 tile 对象
	public void GenerateTiles()
	{
		int n=curMap.n, m=curMap.m;
		tileMap = new Tile[n, m];

		string parentName = "Tiles";
		if (transform.Find(parentName)) // 销毁以前的所有格子
			DestroyImmediate(transform.Find(parentName).gameObject);

		Transform tiles = new GameObject(parentName).transform;
		tiles.parent = transform;

		for (int x=0; x<n; ++x)
			for (int y=0; y<m; ++y)
			{
				Transform newTile = Instantiate(tilePrefab, CoordToPosition(x, y), Quaternion.Euler(Vector3.right*90)); // 将瓦片绕x旋转90°，转为四元数
				newTile.parent = tiles;
				newTile.localScale = Vector3.one * tileContentPercent * tileScale;

				tileMap[x, y] = newTile.GetComponent<Tile>();
			}
	}

	// 生成障碍物
	public void GenerateObstacles()
	{
		string parentName = "Obstacles";
		if (transform.Find(parentName)) // 销毁以前的所有格子
			DestroyImmediate(transform.Find(parentName).gameObject);

		Transform obstacles = new GameObject(parentName).transform;
		obstacles.parent = transform;

		int obstacleCount = (int)(curMap.n*curMap.m*curMap.obstaclePercent), curObstacleCount = 0;
		// float height = obstaclePrefab.localScale.y*0.5f;

		obstacleMap = new bool[curMap.n, curMap.m];
		for (int i=0; i<obstacleCount; ++i)
		{
			Coord coord = GetRandomCoord();
			if (coord!=curMap.spawnPoint)
			{
				obstacleMap[coord.x, coord.y] = true;
				curObstacleCount ++;

				// 如果不能生成，则不生成。所以障碍数并非严格等于obstacleCount
				if (MapIsAccessible(obstacleMap, curObstacleCount))
					GenerateObstacle(coord, obstacles, Mathf.Lerp(curMap.minObstacleHeight, curMap.maxObstacleHeight, (float)rng.NextDouble()));
				else
				{
					obstacleMap[coord.x, coord.y] = false;
					curObstacleCount --;
				}
			}
		}

		GenerateShuffledEmptyCoords();
	}

	//
	public void GenerateShuffledEmptyCoords()
	{
		// 替代或与curMap.seed一起使用，生成与地图无关的随机数，如敌人生成位置
		// Random 在不设置种子时，每次运行都返回不同结果
		int newSeed = Random.Range(0, 2147483647);

		int n=curMap.n, m=curMap.m;
		allEmptyCoords = new List<Coord>();
		for (int x=0; x<n; ++x)
			for (int y=0; y<m; ++y)
				if (!obstacleMap[x,y])
					allEmptyCoords.Add(new Coord(x, y));
		shuffledEmptyCoords = new Queue<Coord>(MyTools.RandomShuffle(allEmptyCoords.ToArray(), curMap.seed^newSeed));
	}

	void GenerateObstacle(Coord coord, Transform parent, float height)
	{
		Transform newObstacle = Instantiate(obstaclePrefab, CoordToPosition(coord.x, coord.y, height/2), Quaternion.identity);
		newObstacle.parent = parent;
		newObstacle.localScale = new Vector3(1, height, 1) * tileScale; // 1 -> obstacleContentPercent

		// 修改材质颜色
		Renderer obsRenderer = newObstacle.GetComponent<Renderer>();
		Material newMaterial = new Material(obsRenderer.sharedMaterial);
		float colorPercent = (float)coord.y / curMap.m;
		newMaterial.color = Color.Lerp(curMap.foregroundColor, curMap.backgroundColor, colorPercent);
		obsRenderer.sharedMaterial = newMaterial;
	}
	#endregion

	#region Utility
	static int[] Way = new int[5]{1, 0, -1, 0, 1};
	bool MapIsAccessible(bool[,] map, int curObstacleCount) // 判断当前图是否不连通。map[i,j]=1表示有障碍
	{
		int n=map.GetLength(0), m=map.GetLength(1);
		bool[,] visit = new bool[n, m];
		Queue<Coord> queue = new Queue<Coord>();

		int count = 1;
		queue.Enqueue(curMap.spawnPoint);
		visit[curMap.spawnPoint.x, curMap.spawnPoint.y] = true;

		while (queue.Count > 0)
		{
			Coord tile = queue.Dequeue();
			for (int i=0; i<4; ++i)
			{
				int xn=tile.x+Way[i], yn=tile.y+Way[i+1];
				if (xn>=0 && xn<curMap.n && yn>=0 && yn<curMap.m && !visit[xn, yn] && !map[xn, yn])
				{
					++count;
					visit[xn, yn] = true;
					queue.Enqueue(new Coord(xn, yn));
				}
			}
		}
		return count+curObstacleCount == curMap.n*curMap.m;
	}

	public Vector3 CoordToPosition(int x,int y)
	{
		// 0为地图中心
		return new Vector3(x-curMap.n*0.5f+0.5f, 0, y-curMap.m*0.5f+0.5f) * tileScale; // 注意不能整除2，当n为奇数时会导致舍入，从而偏离中心
	}

	public Vector3 CoordToPosition(int x,int y,float h) // 附加生成位置的高度
	{
		return new Vector3(x-curMap.n*0.5f+0.5f, h, y-curMap.m*0.5f+0.5f) * tileScale;
	}

	public Coord PositionToCoord(Vector3 pos)
	{
		int x = Mathf.RoundToInt(pos.x/tileScale + (curMap.n-1)*0.5f),
			y = Mathf.RoundToInt(pos.z/tileScale + (curMap.m-1)*0.5f);
		x = Mathf.Clamp(x, 0, curMap.n-1);
		y = Mathf.Clamp(y, 0, curMap.m-1);
		return new Coord(x, y);
	}

	public Tile PositionToTile(Vector3 pos)
	{
		int x = Mathf.RoundToInt(pos.x/tileScale + (curMap.n-1)*0.5f),
			y = Mathf.RoundToInt(pos.z/tileScale + (curMap.m-1)*0.5f);
		x = Mathf.Clamp(x, 0, curMap.n-1);
		y = Mathf.Clamp(y, 0, curMap.m-1);
		return tileMap[x, y];
	}

	public Coord GetRandomCoord()
	{
		Coord coord = shuffledAllCoords.Dequeue();
		shuffledAllCoords.Enqueue(coord); // 出队后放回队尾
		return coord;
	}

	public Coord GetRandomEmptyCoord()
	{
		Coord coord = shuffledEmptyCoords.Dequeue();
		shuffledEmptyCoords.Enqueue(coord); // 出队后放回队尾
		return coord;
	}

	public Tile GetRandomEmptyTile()
	{
		Coord coord = shuffledEmptyCoords.Dequeue();
		shuffledEmptyCoords.Enqueue(coord); // 出队后放回队尾
		return tileMap[coord.x, coord.y];
	}
	#endregion

	[System.Serializable]
	public class Map // 地图设置
	{
		[Header ("地图信息")]
		public Coord spawnPoint; // 玩家生成位置。该点不能有障碍
		public int n, m; // size of map

		[Header ("障碍物生成")]
		public int seed = 10;

		[Range(0, 1)] public float obstaclePercent; // 障碍物数量占比
		public float minObstacleHeight, maxObstacleHeight; // 障碍物高度范围
		public Color foregroundColor, backgroundColor;
	}
}

[System.Serializable]
public struct Coord // Coordinate 坐标
{
	public int x, y;
	public Coord(int _x,int _y) {x=_x; y=_y;}

	public static bool operator == (Coord a, Coord b) {
		return a.x==b.x && a.y==b.y;
	}
	public static bool operator != (Coord a, Coord b) {
		return a.x!=b.x || a.y!=b.y;
	}
	public override bool Equals(object obj) {
		return obj is Coord coord ? this==coord : false;
	}
	public override int GetHashCode()
		=> new {x, y}.GetHashCode();
}
