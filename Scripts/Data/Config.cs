using System.Collections;
using UnityEngine;

namespace DATA
{
	public class PlayerConfig
	{
		// audio
		public int SFXVolume, BGMVolume; // 0~100，AudioManager中为0~1
		// graphics
		public int graphics, resolution;
		public bool fullscreen;
	}

	public class PlayerData
	{
		public int coins;

		public int skillPoints;

		public int maxScore;
	}

	// ! Config类有更高运行优先级
	public class Config : Singleton<Config>
	{
		public PlayerConfig playerConfig;// {get; private set;}
		public PlayerData playerData;

		protected override void Awake()
		{
			base.Awake();

			playerConfig = SaveSystem.LoadFromJson<PlayerConfig>("PlayerConfig");
			// playerData = SaveSystem.LoadFromJson<PlayerData>("PlayerData");
		}

		public void Save()
		{
			SaveSystem.SaveByJson("PlayerConfig", playerConfig);
			SaveSystem.SaveByJson("PlayerData", playerData);
		}
	}
}
