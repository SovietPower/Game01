using MEC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 不在HPSystem中使用RunCoroutine。因为tagForMEC需至少在Player或Enemy类中定义。
public class HPSystem : MonoBehaviour
{
	[Header("生命")]
	public int HP;
	public int maxHP = 100;
	public int shield, maxShield;
	public bool isDead = true;

	[Header("动画")]
	// protected Animator animator;
	protected bool haveHurtAnim, haveDeadAnim;

	Color materialColor;
	public GameObject deathVFXRepulse, deathVFXBlast;

	// 事件
	public event Action OnDeath = delegate {};
	public event Action OnShieldBroken = delegate {};

	// 其它

	#region LifeCycle
	protected virtual void Awake()
	{
		// 注意 out 会*新建*一个局部变量！
		// if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
		// 	spriteRenderer = sr;
		// else
		// 	spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
		// if (TryGetComponent<Animator>(out Animator anim))
		// 	animator = anim;
		// else
		// 	animator = transform.GetChild(0).GetComponent<Animator>();

		materialColor = GetComponent<Renderer>().material.color;

		// deathVFX = Instantiate(_deathVFX);
		// deathVFX.SetActive(false);
		// deathVFX.GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color;
	}

	protected virtual void OnEnable()
	{
		HP = maxHP; shield = maxShield;
		isDead = false;

		healDone = true;
		// if (haveDeadAnim)
		// 	animator.SetBool("Dead", false);
	}
	#endregion

	#region Animation
	// void FlashRed()
	// {
	// 	spriteRenderer.color = Color.red;
	// 	// CancelInvoke(nameof(ResetColor)); // 不使用多次取消
	// 	++invokeCount;
	// 	Invoke(nameof(ResetColor), time);
	// }

	// void ResetColor()
	// {
	// 	if (--invokeCount == 0)
	// 		spriteRenderer.color = originalColor;
	// }

	#endregion

	#region HP
	[ContextMenu ("Die")] // 右键对象的该脚本可直接运行该函数
	public virtual void Die()
	{
		HP = 0; isDead = true;
		gameObject.SetActive(false);
		OnDeath.Invoke();

		// 使用 Pool Get 死亡特效
		GameObject obj = VFXPool.Get(deathVFXBlast.name, transform.position, Quaternion.identity);
		obj.GetComponent<Renderer>().material.color = materialColor;
	}
	public virtual void Die(Vector3 hitPoint, Vector3 hitDirection)
	{
		HP = 0; isDead = true;
		gameObject.SetActive(false);
		OnDeath.Invoke();

		// 使用 Pool Get 死亡特效
		GameObject obj = VFXPool.Get(deathVFXRepulse.name, transform.position, Quaternion.FromToRotation(Vector3.forward, hitDirection));
		obj.GetComponent<Renderer>().material.color = materialColor;
	}

	// 护盾抵挡
	const string strShieldFloatDamage = "Shield Float Damage";
	protected virtual int DamageShield(int damage)
	{
		if (shield > damage)
		{
			UIPool.Get(strShieldFloatDamage, transform.position).transform.GetChild(0).GetComponent<Text>().text = damage.ToString();

			shield -= damage;
			damage = 0;
		}
		else
		{
			UIPool.Get(strShieldFloatDamage, transform.position).transform.GetChild(0).GetComponent<Text>().text = shield.ToString();

			damage -= shield;
			shield = 0;
			OnShieldBroken.Invoke();
		}
		return damage;
	}

	// 受攻击
	const string strHPFloatDamage = "HP Float Damage";
	public virtual void TakeDamage(int damage)
	{
		if (shield > 0)
			damage = DamageShield(damage);
		if (damage > 0)
		{
			UIPool.Get(strHPFloatDamage, transform.position).transform.GetChild(0).GetComponent<Text>().text = damage.ToString();

			if ((HP-=damage) <= 0)
				Die();
		}
	}

	// 受攻击
	public virtual void TakeDamage(int damage, Vector3 hitPoint, Vector3 hitDirection)
	{
		if (shield > 0)
			damage = DamageShield(damage);
		if (damage > 0)
		{
			UIPool.Get(strHPFloatDamage, hitPoint).transform.GetChild(0).GetComponent<Text>().text = damage.ToString();

			if ((HP-=damage) <= 0)
				Die(hitPoint, hitDirection);
		}
	}

	// 受攻击 附加伤害数值显示
	public virtual void TakeDamage(int damage, string floatDamageName)
	{
		// 注意因为动画是修改position，所以需要一个父元素调整基本position
		UIPool.Get(floatDamageName, transform.position).transform.GetChild(0).GetComponent<Text>().text = damage.ToString();

		this.TakeDamage(damage);
	}

	// 受攻击 参数为子弹（击退效果需在 Projectile 处实现）
	public virtual void TakeDamage(Projectile projectile)
	{
		// this.TakeDamage(projectile.damage, projectile.floatDamageName);
		this.TakeDamage(projectile.damage);
	}

	// 受攻击 参数为子弹（击退效果需在 Projectile 处实现）
	public virtual void TakeDamage(Projectile projectile, Vector3 hitPoint, Vector3 hitDirection)
	{
		this.TakeDamage(projectile.damage, hitPoint, hitDirection);
		// this.TakeDamage(projectile.damage, hitPoint, hitDirection, projectile.floatDamageName);
	}

	// 恢复
	public virtual void RestoreHP(int value)
	{
		HP = Mathf.Min(HP+value, maxHP);
	}

	// 恢复 附加伤害数值显示
	public virtual void RestoreHP(int value, string floatDamageName)
	{
		HP = Mathf.Min(HP+value, maxHP);
	}
	#endregion

	#region HOT_DOT
	protected bool healDone;
	// HOT
	protected virtual IEnumerator<float> HealOverTimeCoroutine(float waitTime, int value)
	{
		healDone = false;
		// 在受伤时启用，满血时不使用
		while (HP < maxHP)
		{
			yield return Timing.WaitForSeconds(waitTime);

			RestoreHP(value);
		}
		healDone = true;
	}

	// HOT 附加伤害数值显示
	protected virtual IEnumerator<float> HealOverTimeCoroutine(float waitTime, int value, string floatDamageName)
	{
		// 在受伤时启用，满血时不使用
		while (HP < maxHP)
		{
			yield return Timing.WaitForSeconds(waitTime);

			RestoreHP(value, floatDamageName);
		}
	}

	// DOT
	protected virtual IEnumerator<float> DamageOverTimeCoroutine(float waitTime, int value)
	{
		while (HP > 0)
		{
			yield return Timing.WaitForSeconds(waitTime);

			TakeDamage(value);
		}
	}

	// DOT 附加伤害数值显示
	protected virtual IEnumerator<float> DamageOverTimeCoroutine(float waitTime, int value, string floatDamageName)
	{
		while (HP > 0)
		{
			yield return Timing.WaitForSeconds(waitTime);

			TakeDamage(value, floatDamageName);
		}
	}
	#endregion
}
