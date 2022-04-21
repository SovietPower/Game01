using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	protected new Rigidbody rigidbody;
	protected TrailRenderer trailRenderer;

	protected int projectileID; // 子弹ID，用于查询字典获取配置
	protected string projectileName; // 子弹名称，用于查询字典获取配置

	[Header("子弹移动")]
	public float moveSpeed = 10f;
	Vector3 moveDirection = Vector3.forward;

	// 子弹存在时间，与moveSpeed共同决定最远距离
	protected float lifetime = 4f;

	// 发射时，子弹弹道偏转角度范围
	protected float minBallisticAngle = -60f;
	protected float maxBallisticAngle = 60f;

	[Header("子弹攻击")]
	// 伤害
	public int damage;
	// 攻击间隔
	// public float firePeriod = 0.2f;
	// 是否自动（可连续射击）
	// public bool isAutomatic = true;

	[Header("子弹效果")]
	// 击退效果（击退力）
	public float repulseForce = 0f;

	[Header("视觉效果")]
	// 伤害数值显示
	public string floatDamageName; // 用于在池中查找floatDamage
	// 命中特效
	public string hitVFXName;
	// 是否需要（且可用）根据命中角度旋转特效
	public bool solidHitVFX;

	#region LifeCycle
	// 子类获取数据后，更新组件信息。注意需要在Awake而不是Start，Start可能会比OnEnable晚
	protected virtual void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		if (TryGetComponent<TrailRenderer>(out TrailRenderer temp))
			trailRenderer = temp;
	}

	// 注意不要在OnEnable设置某些东西（如音效），因为池创建时会调用一次
	protected virtual void OnEnable()
	{
		if (trailRenderer != null)
			trailRenderer.Clear();
	}

	// ! 必须在子类的 Start 中调用。传入子弹名称初始化信息。
	protected virtual void Initialize(string name)
	{
		projectileName = name;

		if (!ProjectileDataManager.projectileData.TryGetValue(projectileName, out ProjectileData data))
			Debug.LogError("projectileData could NOT find projectile: "+projectileName);
		else
		{
			projectileID = data.id;

			floatDamageName = data.floatDamageName;
			hitVFXName = data.hitVFXName;
			solidHitVFX = data.solidHitVFX;
		}
	}

	#endregion

	#region Move
	public void MoveHorizontally()
		=> transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

	public void MoveDirectly()
		=> transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

	// 适用于已修改好方向的（rotation），如制导，可直接向右
	public void MoveRightwards()
		=> transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);

	// 简单的直线移动
	protected IEnumerator MoveDirectlyCoroutine()
	{
		while (gameObject.activeSelf)
		{
			MoveDirectly();
			yield return null;
		}
	}

	// 跟踪指定目标移动
	// 注：制导导弹，需在开始制导时保证图片朝向向右（否则localScale.x = -1）（从x正半轴开始选择）。旋转值正确时，Vector3.right 即为正确运动方向。
	protected IEnumerator HomingCoroutine(GameObject target)
	{
		Vector3 targetDirection;
		// 弹道弧度
		float ballisticAngle = Random.Range(minBallisticAngle, maxBallisticAngle);
		// 等待短暂时间后，再开始制导
		float wait = 0.4f;

		while (gameObject.activeSelf)
		{
			if (wait > 0)
				wait -= Time.deltaTime;
			else if (target != null && target.activeSelf)
			{
				targetDirection = target.transform.position - transform.position;

				// Rotate to target
				// var angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg; // 弧度转角度
				transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg, Vector3.forward);

				// 设置弹道轨迹弧度
				transform.rotation *= Quaternion.Euler(0f, 0f, ballisticAngle);
			}
			else
			{
				// 重新寻找target

			}
			// Move straight
			MoveRightwards();

			yield return null;
		}
		// 禁用后，需重置子弹旋转值，不过在对象池取元素时已设置
		// transform.rotation = Quaternion.identity;
	}

	// 波浪形移动
	// public IEnumerator MoveSinCoroutine(float frequency, float amplitude)
    // {
    //     while (gameObject.activeSelf)
    //     {
    //         transform.position = transform.position + transform.up * Mathf.Sin(Time.time * frequency) * amplitude;

    //         yield return null;
    //     }
    // }
	#endregion

	#region Generate
	// 替代OnEnable设置子弹的方向、音效、动作。注意该函数在OnEnable后才调用
	// 注意，每当生成子弹时，都需调用Generate()初始化（而不是OnEnable()）
	public virtual void Generate()
	{
		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;
		// AudioManager.Instance.PlayRepeatSFX(audioData);
	}
	#endregion

	#region Hit
	// 简单的攻击效果
	void HitEnemy(Enemy enemy)
	{
		enemy.TakeDamage(this);
	}

	void HitEnemy(Enemy enemy, Vector3 contactPoint)
	{
		enemy.TakeDamage(this, contactPoint, transform.forward);

		// 减速
		// if (repulse > 0f) // 需侧面命中，而不是从下或上方命中
			// enemy.Repel(moveDirection.x>0f, repulse, repulseDuration);
		// 击退
		// if (repulseForce > 0f)
		// 	enemy.RepelWithForce(repulseForce * Vector2.left); // todo 取圆心向外的方向
	}

	const string enemyStr = "Enemy";
	const string obstacleStr = "Obstacle";
	const string playerStr = "Player";

	// 碰撞：障碍Obstacle
	// 注：敌人使用NavMesh作为物理碰撞检测，不需要实际碰撞体。使用hitbox(trigger)与子弹和玩家的hitbox(trigger)进行碰撞检测
	void OnCollisionEnter(Collision collision)
	{
		// if (collision.gameObject.TryGetComponent<Obstacle>(out Obstacle obstacle))
		if (collision.gameObject.CompareTag(obstacleStr))
		{
			// VFX
			// var contactPoint = collision.GetContact(0);
			// VFXPool.Get(hitVFXName, contactPoint.point, solidHitVFX ? Quaternion.LookRotation(contactPoint.normal) : Quaternion.identity);

			// HitObstacle()

			gameObject.SetActive(false);
		}
	}

	// 接触：敌人Enemy、角色
	void OnTriggerEnter(Collider collider)
	{
		if (collider.CompareTag(enemyStr))
		{
			Enemy enemy = collider.GetComponentInParent<Enemy>();
			// VFX
			var contactPoint = collider.bounds.ClosestPoint(transform.position);
			// VFXPool.Get(hitVFXName, contactPoint, solidHitVFX ? Quaternion.LookRotation(moveDirection*-1) : Quaternion.identity); // 特效偏转角为子弹移动向量*-1。

			HitEnemy(enemy, contactPoint);

			gameObject.SetActive(false);
		}
		// 无友伤 // 应设置在发射后短暂时间内，不与玩家接触(Invoke)
		// else if (collider.CompareTag(playerStr))
		// {
		// 	// VFX
		// 	// var contactPoint = collision.GetContact(0);
		// 	// GameObject obj = Instantiate(hitVFX, contactPoint.point, Quaternion.LookRotation(contactPoint.normal));
		// 	GameObject obj = Instantiate(hitVFX, collider.transform.position, Quaternion.identity);
		// 	// PoolManager.Get(hitVFX.name, contactPoint.point, Quaternion.LookRotation(contactPoint.normal));

		// 	Player player = collider.GetComponentInParent<Player>();
		// 	// player.TakeDamage(damage);
			// Debug.Log($"HIT! Player");

			// gameObject.SetActive(false);
		// }
		// else
		// 	Debug.Log($"HIT222! {collider}");

	}
	#endregion

}

	// if (collision.gameObject.TryGetComponent<Enemy>(out Enemy enemy))
	// {
	// 	// VFX
	// 	var contactPoint = collision.GetContact(0);
	// 	// 需判断是否需要（且支持）旋转特效 solidHitVFX
	// 	VFXPool.Get(hitVFXName, contactPoint.point, solidHitVFX ? Quaternion.LookRotation(contactPoint.normal) : Quaternion.identity);

	// 	HitEnemy(enemy, contactPoint);

	// 	gameObject.SetActive(false);
	// }
	// void HitEnemy(Enemy enemy, ContactPoint2D contactPoint)
	// {
	// 	enemy.TakeDamage(this);

	// 	// 削韧
	// 	enemy.ReduceToughness(toughnessReduce);
	// 	// 减速
	// 	if (Mathf.Abs(contactPoint.normal.x)>0.2f && repulse > 0f) // 需侧面命中，而不是从下或上方命中
	// 		enemy.Repel(contactPoint.normal.x<0f, repulse, repulseDuration);
	// 	// 击退
	// 	if (repulseForce > 0f)
	// 		enemy.RepelWithForce(repulseForce * Vector2.left); // todo 取圆心向外的方向
	// }
