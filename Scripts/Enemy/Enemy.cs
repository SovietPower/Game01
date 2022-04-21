using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class Enemy : HPSystem
{
	protected new Rigidbody rigidbody;
	protected NavMeshAgent navMeshAgent;

	[Header("信息")]
	protected Transform target;
	protected bool hasTarget;
	protected float findTargetRate = 0.3f;

	protected int inherentHOT = 0;
	protected int inherentDOT = 0;

	public float immuneTime = 0.1f;

	// 状态
	public enum State {Idle, Chasing, Patroling, Attacking};
	protected State curState;

	[Header("攻击")]
	public int damage;
	public float attackPeriod;
	public float attackRange;
	public float attackSpeed; // 攻击动作速度
	protected float nextAttackTime;
	protected float attackRangeSqr; // attackRange*attackRange

	// 与目标相距 attackRange+offset-delta 时，就可停止追踪，并发动攻击
	// 如对于 Boxer，在 offset=radius+targetRadius 时，攻击即可恰好命中敌人，但需要更近 delta 距离提高命中率。
	protected float attackRangeOffset, attackRangeDelta;

	[Header("移动")]
	public float maxMoveSpeed = 3f; // 可能需要一个originMaxMoveSpeed，以便实现减速
	public float angularSpeed, acceleration;

	[Header("巡逻")]
	// 巡逻方式不定，由子类决定
	[SerializeField] protected PatrolPath patrolPath;

	// [Header("特效")]
	// 命中特效
	// [SerializeField] GameObject hitVFX; // todo replace with EnemyDataManager
	// 伤害数值显示
	// public string floatDamageName = "Float Damage Red";
	// 死亡特效
	// [SerializeField] GameObject deathVFX;

	// 其它
	public bool onDeathRegES; // onDeath是否已在EnemySpawned中注册

	static int IDForMEC = 0;
	protected string tagForMEC;

	#region LifeCycle
	protected override void Awake()
	{
		base.Awake();
		navMeshAgent = GetComponent<NavMeshAgent>();
		UpdateNavMeshAgent();

		haveHurtAnim = false; haveDeadAnim = false;

		tagForMEC = "Enemy"+(IDForMEC++);
	}

	protected override void OnEnable()
	{
		base.OnEnable();

		curState = State.Idle; // 可在子类中更改初始状态
		navMeshAgent.enabled = true; // 如果在Disable时Stop掉未EndAttack的线程，会导致navMeshAgent.enabled为false
		TryFindTarget();

		// StartCoroutine(OnEverySecond());
	}

	protected virtual void OnDisable()
	{
		Timing.KillCoroutines(tagForMEC);
	}
	#endregion

	#region Attack
	protected void TryRangeAttack() // 远程攻击（有一定距离即可）。相对：melee
	{
		if (hasTarget && Time.time > nextAttackTime)
		{
			if ((target.position - transform.position).sqrMagnitude < attackRangeSqr) // 尽量不要开根号
			{
				nextAttackTime = Time.time + attackPeriod;
				Timing.RunCoroutine(RangeAttackCo(), tagForMEC);
			}
		}
	}

	protected IEnumerator<float> RangeAttackCo()
	{
		BeginAttack();
		Vector3 originalPos = transform.position;
		Vector3 dirToTarget = (target.position - transform.position).normalized;
		// Vector3 attackPos = target.position - dirToTarget*(attackRange+attackRangeOffset-attackRangeDelta);
		Vector3 attackPos = target.position - dirToTarget*(attackRangeOffset);

		float r = 0; // progressRate 攻击动作进度
		while (r <= 1)
		{
			r  += Time.deltaTime * attackSpeed;
			// 需要为攻击动作的进度选择一个自变量为rate的插值函数
			// 因为动画是对称的，选择一个对称函数 y=4(-x^2+x)
			float interpolation = 4*(-r*r + r);
			transform.position = Vector3.Lerp(originalPos, attackPos, interpolation);

			yield return 0f;
		}
		EndAttack();
	}

	// 接触伤害（注意不是Enter）
	void OnTriggerStay(Collider collider)
	{
		if (collider.CompareTag("Player"))
			AttackPlayer(collider.GetComponentInParent<Player>());
	}

	protected virtual void AttackPlayer(Player player)
	{
		if (!player.isImmune)
		{
			// VFX
			// GameObject obj = Instantiate(hitVFX, collider.transform.position, Quaternion.identity);
			// PoolManager.Get(hitVFX.name, contactPoint.point, Quaternion.LookRotation(contactPoint.normal));

			player.TakeDamage(damage);
		}
	}

	protected void TryFindTarget()
	{
		GameObject targetObj = GameObject.FindGameObjectWithTag("Player");
		if (targetObj != null)
		{
			target = targetObj.transform;
			HPSystem targetHPS = target.GetComponent<HPSystem>();
			targetHPS.OnDeath += OnTargetDeath;

			hasTarget = true;
			curState = State.Chasing;

			attackRangeOffset = GetComponent<CapsuleCollider>().radius + target.GetComponent<CapsuleCollider>().radius;
			attackRangeDelta = Mathf.Min(attackRange/2, 1f);
			UpdateAugments(); // 最后执行
		}
	}

	protected IEnumerator<float> FindTargetCo()
	{
		while (hasTarget)
		{
			// 避免navMeshAgent被禁用时，仍会调用SetDestination导致错误
			if (curState == State.Chasing && navMeshAgent.isOnNavMesh)
			{
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				navMeshAgent.SetDestination(target.position - dirToTarget*(attackRangeOffset+attackRange-attackRangeDelta)); // 只追踪到这条线上一定距离外的点
				// 这种设计会导致敌人在最远处先RangeAttack，然后靠近再RangeAttack
			}
			yield return Timing.WaitForSeconds(findTargetRate);
		}
	}
	#endregion

	#region Else
	// 一般用来处理天生的(永久)的每秒会发生的事，如自带的DOT、HOT。附加的OT效果（可能并不是永久）使用额外的函数实现。
	// protected virtual IEnumerator OnEverySecond()
	// {
	// 	WaitForSeconds second = new WaitForSeconds(1);
	// 	while(alive)
	// 	{
	// 		yield return second;

	// 		if (inherentDOT != 0) TakeDamage(inherentDOT);
	// 		if (inherentHOT != 0) RestoreHP(inherentHOT);
	// 		toughness = Mathf.Max(1f, toughness-toughnessRecovery);
	// 	}
	// }

	// ! 注意每当更新该对象某些数据时，需调用此函数！
	public void UpdateAugments()
	{
		float temp = attackRange + attackRangeOffset;
		attackRangeSqr = temp * temp;
	}

	// 更新NavMeshAgent的参数
	public void UpdateNavMeshAgent()
	{
		navMeshAgent.speed = maxMoveSpeed;
		navMeshAgent.angularSpeed = angularSpeed;
		navMeshAgent.acceleration = acceleration;
	}
	#endregion

	#region Event
	protected virtual void OnTargetDeath() // 可在子类中更改
	{
		hasTarget = false;
		curState = State.Idle;
	}
	#endregion

	#region Health
	// public override void TakeDamage(int damage)
	// {
	// 	if (Time.time > nextHurtTime)
	// 	{
	// 		nextHurtTime = Time.time + immuneTime;
	// 		base.TakeDamage(damage);
	// 	}
	// }
	#endregion

	#region Patrol
	// protected Vector3 orientationLeft = new Vector3(1, 1, 1);
	// protected Vector3 orientationRight = new Vector3(-1, 1, 1);

	// 简单更新朝向，不会改变速度
	// protected virtual void ChangeOrientation(bool flag) // flag=0: left flag=1: right
	// {
	// 	if (flag)
	// 		transform.localScale = orientationRight;
	// 	else
	// 		transform.localScale = orientationLeft;
	// }

	// 判断是否更新朝向
	// protected void MoveTowards(float now, float target)
	// {
	// 	if ((now < target) ^ moveOrientation)
	// 	{
	// 		moveOrientation ^= true;
	// 		ChangeOrientation(moveOrientation);
	// 	}
	// }

	// 沿 PatrolPathX 巡逻
	// protected virtual void PatrolX()
	// {
	// 	// 误差要大些，避免速度过快
	// 	if (Mathf.Abs(transform.position.x - targetX) > 0.5f)
	// 		MoveTowards(transform.position.x, targetX);
	// 	else // 到达指定位置。注意不是在此处修改朝向，因为可能受外力
	// 	{
	// 		if (targetX == patrolPathX.start)
	// 			targetX = patrolPathX.end;
	// 		else
	// 			targetX = patrolPathX.start;
	// 	}
	// }

	// 沿 PatrolPath 2D巡逻，适合飞行类敌人
	// protected virtual IEnumerator RandomlyMoveCoroutine(PatrolPath patrolPath)
	// {
	// 	Vector2 start = patrolPath.points[0];
	// 	Vector2 end = patrolPath.points[1];
	// 	Vector2 target = start;

	// 	while (gameObject.activeSelf)
	// 	{
	// 		// 还未到达指定位置
	// 		if (Vector2.Distance(transform.position, target) > Mathf.Epsilon)
	// 		{
	// 			// 继续移动
	// 			transform.position = Vector2.MoveTowards(transform.position, target, maxMoveSpeed*Time.deltaTime);
	// 		}
	// 		else
	// 		{
	// 			if (target.x == start.x)
	// 				target = end;
	// 			else
	// 				target = start;
	// 			Debug.Log($"patroling {target}");
	// 		}

	// 		yield return null;
	// 	}
	// }
	#endregion

	#region State
	// 开始攻击动作
	protected void BeginAttack()
	{
		curState = State.Attacking;
		navMeshAgent.enabled = false;
	}

	// 结束攻击动作
	protected void EndAttack()
	{
		navMeshAgent.enabled = true;
		curState = State.Chasing;
	}

	// // 更新水平速度：在减速时间结束后恢复最大速度。
	// protected void FixedUpdateSpeed()
	// {
	// 	if (repulseDuration > 0f)
	// 		repulseDuration -= Time.fixedDeltaTime;
	// 	if (Mathf.Abs(maxMoveSpeed - rigidbody.velocity.x) > 0.1f && repulseDuration <= 0f)
	// 	{
	// 		if (moveOrientation)
	// 			rigidbody.velocity = new Vector2(maxMoveSpeed, rigidbody.velocity.y);
	// 		else
	// 			rigidbody.velocity = new Vector2(-maxMoveSpeed, rigidbody.velocity.y);
	// 	}
	// }

	// // 若武器有击退力，则附加。
	// public void RepelWithForce(Vector2 force)
	// {
	// 	rigidbody.AddForce(force, ForceMode2D.Impulse);
	// }

	// public override void Die()
	// {
	// 	base.Die();

	// 	// EnemyManager.Instance.Remove(gameObject);
	// 	SpriteEvent se = Instantiate(deathVFX, transform.position, Quaternion.identity).GetComponent<SpriteEvent>();
	// 	se.color = Color.yellow;
	// }
	#endregion

}

	// 更新速度（若未到达指定速度，则加速）
	// protected void UpdateSpeed()
	// {
	// 	if (moveOrientation)
	// 	{
	// 		if (maxMoveSpeed - rigidbody.velocity.x > 0.1f)
	// 		{
	// 			rigidbody.velocity = new Vector2(Mathf.Min(rigidbody.velocity.x+acceleration*Time.fixedDeltaTime, maxMoveSpeed), rigidbody.velocity.y);
	// 		}
	// 	}
	// 	else
	// 	{
	// 		if (rigidbody.velocity.x + maxMoveSpeed > 0.1f)
	// 		{
	// 			rigidbody.velocity = new Vector2(Mathf.Max(rigidbody.velocity.x-acceleration*Time.fixedDeltaTime, -maxMoveSpeed), rigidbody.velocity.y);
	// 		}
	// 	}
	// }

	// protected void RandomlyMove()
	// {
	// 	// 还未到达指定位置
	// 	if (Mathf.Abs(transform.position.x - targetX) > 1e-2)
	// 	{
	// 		// 继续移动
	// 		transform.position = new Vector3(MoveTowards(transform.position.x, targetX, maxMoveSpeed*Time.deltaTime), transform.position.y);
	// 		if (transform.position.y < 0.2f && transform.position.y > -0.5f)
	// 			Debug.Log($"y: {transform.position.y}");
	// 	}
	// 	else // 到达指定位置。注意不是在此处修改朝向，因为可能受外力
	// 	{
	// 		if (targetX == patrolPathX.start)
	// 			targetX = patrolPathX.end;
	// 		else
	// 			targetX = patrolPathX.start;
	// 	}
	// }

	// protected virtual IEnumerator RandomlyMoveCoroutine(PatrolPathX patrolPathX)
	// {
	// 	float start = patrolPathX.start;
	// 	float end = patrolPathX.end;
	// 	float target = start, tmp;

	// 	while (gameObject.activeSelf)
	// 	{
	// 		// 还未到达指定位置
	// 		if (Mathf.Abs(transform.position.x - target) > 1e-2)
	// 		{
	// 			// 继续移动
	// 			tmp = MoveTowards(transform.position.x, target, maxMoveSpeed*Time.deltaTime);
	// 			transform.position = new Vector3(tmp, transform.position.y);
	// 			// if (transform.position.y < 1)
	// 			// 	Debug.Log($"y: {transform.position.y}");
	// 		}
	// 		else // 到达指定位置。注意不是在此处修改朝向，因为可能受外力
	// 		{
	// 			if (target == start)
	// 				target = end;
	// 			else
	// 				target = start;
	// 		}

	// 		yield return null;
	// 	}
	// }