using UnityEngine;

namespace WEAPON
{
	public class Peacekeeper : Weapon
	{
		int shootCount; // 总弹丸数
		float dispersion; // 散布半径

		protected override void Awake()
		{
			base.Awake();

			// 属性
			isAutomatic = false;
			period = 1f;

			magSize = 5;
			tacReloadTime = 2.5f;
			fullReloadTime = 3.5f;

			// 效果
			haveBackRecoil = true; haveRotRecoil = false;
			recoilDistance = Vector3.forward * 0.4f;
			recoilBackTime = 0.5f;

			// 特殊属性
			shootCount = 11;
			dispersion = 8f;
		}

		public override void Attack()
		{
			if (CanShoot())
			{
				AudioManager.Instance.PlayRepeatSFX(shootAudio);

				DisperseShoot(shootCount, dispersion);
			}
		}
	}
}
