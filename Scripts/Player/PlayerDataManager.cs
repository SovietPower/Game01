using DATA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DATA
{
	public class PlayerDataManager : Singleton<PlayerDataManager>
	{
		PlayerData playerData;

		protected override void Awake()
		{
			base.Awake();

			playerData = Config.Instance.playerData;
		}

		void OnDisable()
		{
			Config.Instance.playerData = playerData;
		}
	}
}
