using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Player : HPSystem
{
	[SerializeField] Player1Input input;

	new Rigidbody rigidbody;
	Camera mainCamera;

	[Header("信息")]
	public int restoreHP = 0;
	public float HPRestorePeriod = 1f;
	CoroutineHandle restoreHPCoroutine;

	[Header("状态")]
	public bool isImmune;
	public float immuneTime = 2f;
	Material skinMaterial;
	Color originalSkinColor;

	[Header("移动")]
	public float maxMoveSpeed = 7f; // 自身运动最大速度（不考虑外部速度，如传送带）
	Vector3 moveSpeed;

	// public float acceleration = 8f; // 在外力作用后恢复正常运动或静止的加速度 // 不使用加速度，也不改变刚体速度。使用每帧位移。
	// public float deceleration = 4f; // 在外力作用后恢复静止的减速度

	[Header("动作状态")]
	public bool isIdle;
	public bool isRunning, isFiring;

	// 武器
	WeaponController weaponController;
	int shootCount; // 控制非自动枪

	// 道具
	// ItemController

	// 其它
	Plane groundPlane;

	MapGenerator mapGenerator;

	static int IDForMEC = 0;
	protected string tagForMEC;

	#region LifeCycle
	protected override void Awake()
	{
		base.Awake();

		mainCamera = Camera.main;
		rigidbody = GetComponent<Rigidbody>();
		skinMaterial = GetComponent<Renderer>().material;
		originalSkinColor = skinMaterial.color;

		weaponController = GetComponent<WeaponController>();

		mapGenerator = FindObjectOfType<MapGenerator>();

		haveHurtAnim = false; haveDeadAnim = false;

		tagForMEC = "Player"+(IDForMEC++);
	}

	void Start()
	{
		// groundPlane = new Plane(Vector3.up, Vector3.up * weaponController.weaponHolderHeight);
		groundPlane = new Plane(Vector3.up, Vector3.up * 0.9f); // ! 如果需要修改WeaponHolder或玩家的高度，需修改groundPlane的高度（如放到Update中）

		// 游戏中使用到的变量
		// UpdateAugments();
	}

	void Update()
	{
		FaceToMouse();

		// Shoot
		if (isFiring)
			weaponController.Shoot(shootCount);

		// 越界检查
		if (transform.position.y < -3f)
		{
			TakeDamage(30);
			mapGenerator.ResetPlayerPosition();
		}
	}

	// FixedUpdate 在固定的时间间隔执行，不受游戏帧率的影响，适合处理处理物理逻辑，如Rigidbody。按键等适合在Update。
	void FixedUpdate()
	{
		UpdateMovement();
	}

	protected override void OnEnable()
	{
		base.OnEnable();

		// 输入
		input.EnablePlayInput();

		// ! 注意每当修改玩家按键表时，都需在此处订阅事件
		// 注意，不同对象的Move是不同的函数！在对象Disable或Destroy时，必须取消订阅所有事件，否则会调用已不可用的对象的函数
		input.onMove += Move;
		input.onStopMove += StopMove;
		input.onFire += Fire;
		input.onStopFire += StopFire;
		input.onReload += weaponController.Reload;
		input.onPreviousWeapon += weaponController.PreWeapon;
		input.onNextWeapon += weaponController.NextWeapon;
	}

	void OnDisable()
	{
		Timing.KillCoroutines(tagForMEC);

		input.DisableAllInputs();

		// ! 注意每当修改玩家按键表时，都需在此处取消订阅事件
		input.onMove -= Move;
		input.onStopMove -= StopMove;
		input.onFire -= Fire;
		input.onStopFire -= StopFire;
		input.onReload -= weaponController.Reload;
		input.onPreviousWeapon -= weaponController.PreWeapon;
		input.onNextWeapon -= weaponController.NextWeapon;
	}
	#endregion

	#region Control
	// 当玩家每次出生时，调用该函数
	// void ResetPlayer()
	// {
	// 	mapGenerator.ResetPlayerPosition();
	// }

	// 使角色朝向鼠标，并设置鼠标准星
	void FaceToMouse()
	{
		Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()); // 从相机向鼠标画线，取与平面的交点，即可获得正确的鼠标朝向
		float rayDistance;
		if (groundPlane.Raycast(ray, out rayDistance))
		{
			Vector3 point = ray.GetPoint(rayDistance);
			// 人物旋转
			transform.LookAt(new Vector3(point.x, transform.position.y, point.z));

			// 武器旋转
			weaponController.LookAt(point);

			// 设置准星
			weaponController.crosshair.transform.position = point;
			weaponController.crosshair.DetectTargets(ray);

			// Debug.DrawLine(ray.origin, point, Color.red);
		}
	}

	// 使角色看向某点
	void LookAt(Vector3 point) // 这里的点是与平面的交点，高度y是地面高度，但应与角色高度相同
	{
		transform.LookAt(new Vector3(point.x, transform.position.y, point.z));
	}
	#endregion

	#region Fire
	void Fire()
	{
		isFiring = true;
		shootCount ++;
	}

	void StopFire()
	{
		isFiring = false;
	}
	#endregion

	#region Health
	public void BecomeImmune()
	{
		isImmune = true;
		skinMaterial.color = Color.white;
		Invoke(nameof(StopImmune), immuneTime); // 不会频繁触发，使用Invoke
	}

	public void StopImmune()
	{
		isImmune = false;
		skinMaterial.color = originalSkinColor;
	}

	public override void TakeDamage(int damage)
	{
		base.TakeDamage(damage);

		BecomeImmune();
		if (gameObject.activeSelf)
		{
			if (restoreHP > 0 && !healDone) // healDone=0 时不重复启用Coroutine
			{
				if (restoreHPCoroutine != null)
					Timing.KillCoroutines(restoreHPCoroutine);
				restoreHPCoroutine = Timing.RunCoroutine(HealOverTimeCoroutine(HPRestorePeriod, restoreHP));
			}
		}
	}

	#endregion

	#region Move
	void UpdateMovement() // 通过位移而不是直接更改速度，在受力时需要恢复速度？
	{
		rigidbody.MovePosition(rigidbody.position + moveSpeed*Time.fixedDeltaTime);
		// transform.Translate(moveSpeed*Time.fixedDeltaTime); // todo to be tested
	}

	// 按键按下时，会调用该函数（通过input.onMove保存的委托）
	// 注意一般游戏中，即使向斜方向移动，玩家单方向的移速也不会有降低，即input不应使用正交化的，而直接用1,0,-1。
	void Move(Vector2 input)
	{
		Vector3 moveInput = new Vector3(input.x>0?1:input.x<0?-1:0, 0, input.y>0?1:input.y<0?-1:0); // 注意3D默认是y上下，x是左右，z是前后
		isRunning = true;

		moveSpeed = moveInput * maxMoveSpeed;
	}

	// 按键松开时，会调用该函数（通过input.onStopMove保存的委托）
	void StopMove()
	{
		isRunning = false;
		moveSpeed = Vector3.zero;
	}

	// IEnumerator MoveCoroutine(float acceleration, Vector3 moveVelocity)
	// {
	// 	Vector3 delta = acceleration * Time.deltaTime * (moveVelocity - rigidbody.velocity).normalized;
	// 	while ((moveVelocity - rigidbody.velocity).magnitude > 0.2f)
	// 	{
	// 		rigidbody.velocity = rigidbody.velocity + delta;
	// 		yield return null;
	// 	}
	// 	rigidbody.velocity = moveVelocity;
	// }

	// IEnumerator StopMoveCoroutine(float deceleration)
	// {
	// 	Vector3 delta = deceleration * Time.deltaTime * (-rigidbody.velocity).normalized;
	// 	while (rigidbody.velocity.magnitude > 0.2f)
	// 	{
	// 		rigidbody.velocity = rigidbody.velocity + delta;
	// 		yield return null;
	// 	}
	// 	rigidbody.velocity = Vector2.zero;
	// }
	#endregion

	#region Operation
	public void Collect(Collectable item)
	{
		print("Got!");
	}
	#endregion

	#region State

	#endregion
}
