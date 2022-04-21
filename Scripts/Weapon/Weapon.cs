using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	public Projectile projectile;
	public Transform shell;
	public Crosshair crosshairPrefab; // 瞄准准星（prefab）

	Transform muzzle; // 枪口位置
	Transform shellPoint;
	MuzzleFlash muzzleFlash;

	[Header ("武器属性")] // don't modify in UGUI
	public bool isAutomatic;

	protected float period = 1f;
	float nextShotTime;

	public int magSize, magRemaining;

	protected float tacReloadTime, fullReloadTime; // 子弹未全空、全空时装填时间
	[Header ("状态")]
	public bool isReloading;

	[Header ("效果")]
	protected bool haveBackRecoil = false, haveRotRecoil = false;
	// 向后的后坐力
	protected Vector3 recoilDistance = Vector3.forward * 0.2f; // 枪因后坐力导致的位移
	protected float recoilBackTime = 0.1f; // 枪因后坐力恢复位移所需时间，要小于开火周期（连射没事，但要小于period）
	// 向某方向的后坐力
	float curRecoilAngle;
	protected float recoilAngle;
	protected float recoilRotTime = 0.1f;

	[Header ("外观")]
	public Crosshair crosshair; // 瞄准准星

	[Header ("音效")]
	public AudioClip shootAudio; // 直接在此处赋值
	public AudioClip reloadAudio; // 直接在此处赋值

	// 其它
	static int IDForMEC = 0;
	protected string tagForMEC;

	#region LifeCycle
	protected virtual void Awake()
	{
		muzzle = transform.Find("Muzzle");
		shellPoint = transform.Find("Shell Point");
		muzzleFlash = GetComponent<MuzzleFlash>();

		// 实例化附加对象（可能并不是子对象）
		crosshair = Instantiate(crosshairPrefab);

		tagForMEC = "Weapon"+(IDForMEC++);
	}

	protected void OnDisable()
	{
		Timing.KillCoroutines(tagForMEC);

		// 注意结束状态
		if (isReloading)
			isReloading = false;
	}

	Vector3 recoilBackDampV;
	float recoilRotDampV;
	void LateUpdate() // 将在LookAt后执行
	{
		// 后坐力恢复
		if (haveBackRecoil && transform.localPosition.sqrMagnitude > 0.001f)
			transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilBackDampV, recoilBackTime);
		if (haveRotRecoil && curRecoilAngle > 0.1f)
		{
			curRecoilAngle = Mathf.SmoothDamp(curRecoilAngle, 0f, ref recoilRotDampV, recoilRotTime);
			transform.localEulerAngles += Vector3.left * curRecoilAngle; // 向上。其它方向需修改Vector3
		}
	}
	#endregion

	#region Fire
	public virtual void Attack()
	{
		if (CanShoot())
			SingleShoot();
	}

	// 每轮射击的效果（每颗子弹只触发一次）
	void FireEffect()
	{
		magRemaining --;

		// 射击音效在子类中实现
		// AudioManager.Instance.PlayRepeatSFX(shootAudio);

		WeaponPool.Get(shell.name, shellPoint.position, shellPoint.rotation);
		muzzleFlash.Activate();

		// 枪后坐力
		if (haveBackRecoil)
			transform.localPosition -= recoilDistance;
		if (haveRotRecoil)
			curRecoilAngle = Mathf.Min(curRecoilAngle+recoilAngle, 45f);
	}

	void FireProjectile()
	{
		Projectile p = ProjectilePool.Get(projectile.name, muzzle.position, muzzle.rotation).GetComponent<Projectile>();
		p.Generate();

	}

	// 发射时偏转指定角度
	void FireProjectile(Vector3 rotation)
	{
		Projectile p = ProjectilePool.Get(projectile.name, muzzle.position, muzzle.rotation).GetComponent<Projectile>();
		p.transform.Rotate(rotation, Space.Self);
		p.Generate();
	}

	public bool CanShoot()
	{ // 单轮多连发武器，需保证弹夹容量为连发数倍数
		if (!isReloading)
		{
			if (magRemaining > 0)
			{
				if (Time.time > nextShotTime)
				{
					nextShotTime = Time.time + period;
					return true;
				}
				return false;
			}
			Reload();
		}
		return false;
	}

	// 单发
	protected void SingleShoot()
	{
		FireEffect();
		FireProjectile();
	}

	// 多连发。参数：单次连发数，发射间隔。
	protected IEnumerator<float> BurstShoot(int shootCount, float shootPeriod)
	{
		for (; shootCount>0; shootCount--)
		{
			SingleShoot();
			yield return Timing.WaitForSeconds(shootPeriod); // 注意不能提前设置 Timing.WaitForSeconds
		}
	}

	// 在指定散布半径的锥形中发射
	// Vector3 shellPointRotation;
	protected void DisperseShoot(float radius)
	{
		FireEffect();
		FireProjectile(MyTools.RandomInsideCone(radius) * 180f);
	}
	protected void DisperseShoot(int shootCount, float radius)
	{
		FireEffect();
		for (; shootCount>0; shootCount--)
			FireProjectile(MyTools.RandomInsideCone(radius) * 180f);
	}
	#endregion

	#region Operation
	// 初始化
	public void Initialize()
	{
		magRemaining = magSize;
	}

	public void Reload()
	{
		if (!isReloading && magRemaining < magSize)
		{
			isReloading = true;
			AudioManager.Instance.PlaySFX(reloadAudio);

			Timing.RunCoroutine(ReloadCo(), tagForMEC);
		}
	}

	IEnumerator<float> ReloadCo()
	{
		float reloadTime = magRemaining > 0 ? tacReloadTime : fullReloadTime, reloadSpeed = 1f / reloadTime;

		for (float percent=0f; percent < 1f; )
		{
			percent += Time.deltaTime * reloadSpeed;

			yield return 0f;
		}

		Debug.Assert(isReloading == true);
		isReloading = false;
		magRemaining = magSize;
	}
	#endregion
}
