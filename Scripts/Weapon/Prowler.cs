using MEC;
using UnityEngine;

namespace WEAPON
{
	public class Prowler : Weapon
	{
		int shootNumber; // 连发数
		float shootPeriod; // 连发间隔

		protected override void Awake()
		{
			base.Awake();

			// 属性
			isAutomatic = true;
			period = 0.7f;

			magSize = 24;
			tacReloadTime = 1.9f;
			fullReloadTime = 2.4f;

			// 效果
			haveBackRecoil = true; haveRotRecoil = false;
			recoilDistance = Vector3.forward * 0.2f;
			recoilBackTime = 0.1f;

			// 特殊属性
			shootNumber = 3;
			shootPeriod = 0.07f;
		}

		public override void Attack()
		{
			if (CanShoot())
			{
				AudioManager.Instance.PlayRepeatSFX(shootAudio);

				Timing.RunCoroutine(BurstShoot(shootNumber, shootPeriod).CancelWith(gameObject), tagForMEC);
			}
		}
	}
}
