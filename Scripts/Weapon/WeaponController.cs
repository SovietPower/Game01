using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
	Transform weaponHolder; // 持枪位置。每创建一个武器x时，将其放到WeaponHolder对象下，看下position是否合适（x的position需均为0，调整x的graphics使其合适，不能调整weapon和weaponHold本身）。

	public Weapon startingWeapon;
	public Weapon[] allAvailableWeapons;

	int curIndex;
	Weapon curWeapon;
	List<Weapon> curWeapons;

	int shootCount;

	// 拷贝一些武器参数，减少对curWeapon的引用
	public Crosshair crosshair; // 瞄准准星

	// UI
	Text weaponUI;
	Player player;

	void Awake()
	{
		weaponHolder = transform.Find("Weapon Holder");

		curWeapons = new List<Weapon>();

		// weaponUI = GameObject.FindObjectOfType<Text>();
		weaponUI = GameObject.FindGameObjectWithTag("Respawn").GetComponent<Text>();
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
	}

	void Start()
	{
		// if (startingWeapon != null)
		// 	EquipWeapon(startingWeapon);
		foreach (Weapon weapon in allAvailableWeapons)
			NewWeapon(weapon);
	}

	void Update()
	{
		string temp = "HP: "+player.HP+"/"+player.maxHP+" "+curWeapon.name;
		if (curWeapon.isReloading)
			temp += " Reloading";
		else
			temp += " "+ curWeapon.magRemaining + "/" + curWeapon.magSize;
		weaponUI.text = temp;
	}

	#region Weapon
	public void NewWeapon(Weapon weapon)
	{
		Weapon newWeapon = Instantiate(weapon, weaponHolder.position, weaponHolder.rotation);
		newWeapon.transform.parent = weaponHolder;
		newWeapon.Initialize();

		curWeapons.Add(newWeapon);
		EquipWeapon(curWeapons.Count - 1);
	}

	public void EquipWeapon(int index)
	{
		if (curWeapon != null)
			DisableWeapon(curWeapon);

		curIndex = index;
		curWeapon = curWeapons[index];
		EnableWeapon(curWeapon);

		// 拷贝部分武器参数
		crosshair = curWeapon.crosshair;
	}

	public void PreWeapon()
	{
		if (curWeapons.Count > 1)
			EquipWeapon(curIndex==0 ? curWeapons.Count-1: curIndex-1);
	}

	public void NextWeapon()
	{
		if (curWeapons.Count > 1)
			EquipWeapon(curIndex==curWeapons.Count-1 ? 0 : curIndex+1);
	}

	// 更换武器及其对象的可见性
	void EnableWeapon(Weapon weapon)
	{
		weapon.gameObject.SetActive(true);
		weapon.crosshair.gameObject.SetActive(true);
	}

	// 更换武器及其对象的可见性
	void DisableWeapon(Weapon weapon)
	{
		weapon.gameObject.SetActive(false);
		weapon.crosshair.gameObject.SetActive(false);
	}

	// 销毁武器及其对象
	void DestroyWeapon(Weapon weapon)
	{
		Destroy(weapon.crosshair);
		Destroy(weapon);
	}
	#endregion

	#region Fire
	public void Shoot(int playerSC)
	{
		// if (curWeapon != null)

		// 控制武器是否自动
		if (curWeapon.isAutomatic)
			curWeapon.Attack();
		else if (shootCount < playerSC)
		{
			shootCount = playerSC;
			curWeapon.Attack();
		}
	}
	#endregion

	#region Operation
	// 令武器朝指定方向旋转
	public void LookAt(Vector3 point)
	{
		// 仅在鼠标位置与人物距离足够远时旋转。太近将不能正确瞄准
		if ((point - transform.position).sqrMagnitude > 1.3f && curWeapon != null)
			curWeapon.transform.LookAt(point);
	}

	public void Reload()
	{
		if (curWeapon != null)
			curWeapon.Reload();
	}
	#endregion

	#region Else
	public float weaponHolderHeight {
		get {
			return weaponHolder.position.y;
		}
	}
	#endregion
}
