using UnityEngine;

namespace WEAPON
{
	public class Wingman : Weapon
	{
		protected override void Awake()
		{
			base.Awake();

			// 属性
			isAutomatic = false;
			period = 0.45f;

			magSize = 8;
			tacReloadTime = 2f;
			fullReloadTime = 2.1f;

			// 效果
			haveBackRecoil = false; haveRotRecoil = true;
			recoilAngle = 40f;
			recoilRotTime = 0.3f;

			// 特殊属性
		}

		public override void Attack()
		{
			if (CanShoot())
			{
				AudioManager.Instance.PlaySFX(shootAudio);

				SingleShoot();
			}
		}
	}
}
