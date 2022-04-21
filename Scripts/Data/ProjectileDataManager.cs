using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDataManager : MonoBehaviour
{
	static int count;
	public static Dictionary<string, ProjectileData> projectileData;

	void Awake()
	{
		// 初始化每个dict
		projectileData = new Dictionary<string, ProjectileData>();

		// 初始化所有子弹
		InitializeProjectile("Machine Gun", "Float Damage Red", true, "Hit Particle Basic");
		InitializeProjectile("Homing Missile", "Float Damage Red", false, "Hit Large 3");
	}

	void InitializeProjectile(string name, string floatDamage, bool solidHitVFX, string hitVFX)
	{
		++count;

		ProjectileData data = new ProjectileData() {
			id = count,
			name = name,

			floatDamageName = floatDamage,
			hitVFXName = hitVFX,
			solidHitVFX = solidHitVFX,
		};

		projectileData.Add(name, data);
	}

}

[System.Serializable]
class ProjectileDataGroup
{

}