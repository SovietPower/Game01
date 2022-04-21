using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Serializable]
public class Level
{
	public string[] enemies; // 要依次生成的敌人
	public float timeBetweenSpawns; // 敌人生成间隔

	public float HPFactor; // 总血量系数

	public int EnemyCount{
		get => enemies.Length;
	}
}

// todo 敌人在边界生成时，似乎可能无法导航
public class EnemySpawner : MonoBehaviour
{
	// 波数
	public Level[] levels;

	public int curLevelNumber;
	public Level curLevel;

	// 敌人
	int enemiesRemaining, enemiesSpawned, enemiesAlive; // 剩余未生成敌人数，当前生成数
	float nextSpawnTime = 0f; // 下次生成敌人时间

	// 流程
	MapGenerator map;
	bool playerDead;
	Player player;
	Transform playerT;

	public event UnityAction<int> OnNewLevel = delegate {};

	// 其它
	static int IDForMEC = 0;
	string tagForMEC;

	#region LifeCycle
	void Awake()
	{
		tagForMEC = "EnemySpawner"+(IDForMEC++);
	}

	void Start()
	{
		map = FindObjectOfType<MapGenerator>();

		playerDead = false;
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		playerT = player.transform;
		player.OnDeath += OnPlayerDeath;

		NextLevel();
	}

	void OnDisable()
	{
		Timing.KillCoroutines(tagForMEC);
	}

	void Update()
	{
		if (enemiesRemaining > 0 && Time.time > nextSpawnTime && !playerDead)
		{
			enemiesRemaining --;
			nextSpawnTime = Time.time + curLevel.timeBetweenSpawns;

			Timing.RunCoroutine(SpawnEnemy(), tagForMEC);
		}
	}
	#endregion

	#region Game
	void OnPlayerDeath()
	{
		playerDead = true;
	}

	void OnEnemyDeath()
	{
		enemiesAlive --;
		if (enemiesAlive == 0)
			Invoke(nameof(NextLevel), 1.5f);
	}

	public void NextLevel()
	{
		if (curLevelNumber >= levels.Length)
		{
			print("You Win");
			return;
		}
		nextSpawnTime = Time.time + 2f;

		curLevel = levels[curLevelNumber];
		curLevelNumber ++;

		enemiesSpawned = 0;
		enemiesAlive = enemiesRemaining = curLevel.enemies.Length;

		OnNewLevel.Invoke(curLevelNumber); // 是加1后的levelNumber

		AudioManager.Instance.UpdatePlayer();
	}
	#endregion

	#region DevMode
	[ContextMenu ("Skip Level")]
	public void SkipLevel() // 慎用，可能会有bug
	{
		StopAllCoroutines();
		foreach (Enemy enemy in FindObjectsOfType<Enemy>())
			enemy.Die();
		NextLevel();
	}

	#endregion

	#region Spawn
	float nextBesidePlayerTime;
	float besidePlayerMinT = 3f, besidePlayerMaxT = 8f; // 每隔多久在玩家所在格生成敌人

	IEnumerator<float> SpawnEnemy()
	{
		// 敌人生成预警：tile闪烁
		float spawnDelay = 1.5f; // 敌人生成的预警时间
		float flashSpeed = 3; // tile 每秒闪烁次数

		Tile tile;
		if (Time.time > nextBesidePlayerTime)
		{
			nextBesidePlayerTime = Time.time + Random.Range(besidePlayerMinT, besidePlayerMaxT);
			tile = map.PositionToTile(playerT.position);
		}
		else
			tile = map.GetRandomEmptyTile();
		Material tileMat = tile.rendererMaterial;
		Color originalColor = tile.originalColor; // 不能用material.color，可能是正在闪烁时的颜色
		Color flashColor = Color.red;

		for (float timer=0f; timer<spawnDelay; )
		{
			timer += Time.deltaTime;
			tileMat.color = Color.Lerp(originalColor, flashColor, Mathf.PingPong(timer*flashSpeed, 1f));
			// PingPong 为一个从0到1、1到0变化的周期函数

			yield return 0f;
		}
		tileMat.color = originalColor; // 注意循环结束时不一定恰好是周期结束

		Enemy newEnemy = EnemyPool.Get(curLevel.enemies[enemiesSpawned++]).GetComponent<Enemy>(); // 最好不要用transform移动启用NavMeshAgent的对象
		if (!newEnemy.GetComponent<NavMeshAgent>().isOnNavMesh)
			Debug.LogWarning($"!isOnNavMesh11 ??? {newEnemy.transform.position}");
		newEnemy.GetComponent<NavMeshAgent>().Warp(tile.position + Vector3.up);
		if (!newEnemy.GetComponent<NavMeshAgent>().isOnNavMesh)
			Debug.LogWarning($"!isOnNavMesh22 ??? {newEnemy.transform.position}"); // todo still happens somethings

		if (!newEnemy.onDeathRegES) // 注意只能注册一次，否则会被执行多次
		{
			newEnemy.onDeathRegES = true;
			newEnemy.OnDeath += OnEnemyDeath;
		}

		AdjustEnemyAttr(newEnemy);

		// newEnemy.GetComponent<NavMeshAgent>().enabled = false;
		// newEnemy.transform.position = tile.position + Vector3.up;
		// newEnemy.GetComponent<NavMeshAgent>().enabled = true;
	}

	void AdjustEnemyAttr(Enemy enemy)
	{
		enemy.maxHP = (int)(enemy.maxHP * curLevel.HPFactor);
		enemy.HP = (int)(enemy.HP * curLevel.HPFactor);
	}
	#endregion
}
