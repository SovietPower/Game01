using UnityEngine;

namespace WEAPON
{
	public class MachineGun : Weapon
	{
		// AudioClip shootNextAudio; // 在以后实现

		protected override void Awake() // 需在某些类的Start前执行
		{
			base.Awake();

			// 属性
			// 获取名为MachineGun的Weapon数据
			isAutomatic = true;
			period = 0.15f;

			magSize = 20;
			tacReloadTime = 2.1f;
			fullReloadTime = 2.7f;

			// 效果
			haveBackRecoil = true; haveRotRecoil = false;
			recoilDistance = Vector3.forward * 0.2f;
			recoilBackTime = 0.1f;

			// 特殊属性
		}

		public override void Attack()
		{
			if (CanShoot())
			{
				AudioManager.Instance.PlayRepeatSFX(shootAudio);

				SingleShoot();
			}
		}
	}
}
