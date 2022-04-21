using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AUDIO
{
	public enum GroupType {Player, Enemy};

	public class AudioLibrary : MonoBehaviour
	{
		// 敌人音效
		public AudioGroup[] enemyAudioGroups;
		Dictionary<string, AudioClip[]> enemyDict;

		void Awake()
		{
			Initialize(enemyAudioGroups, enemyDict);
		}

		void Initialize(AudioGroup[] audioGroups, Dictionary<string, AudioClip[]> dict)
		{
			dict = new Dictionary<string, AudioClip[]>();
			foreach (AudioGroup group in audioGroups)
				dict[group.groupID] = group.clips;
		}

		public AudioClip GetClip(string id, GroupType groupType)
		{
			switch(groupType)
			{
				case GroupType.Enemy:
					return GetClipInDict(enemyDict, id);
			}
			return null;
		}

		AudioClip GetClipInDict(Dictionary<string, AudioClip[]> dict, string id)
		{
			if (dict.ContainsKey(id))
			{
				AudioClip[] clips = dict[id];
				return clips[Random.Range(0, clips.Length)];
			}
			return null;
		}

		[System.Serializable]
		public class AudioGroup
		{
			public string groupID;
			public AudioClip[] clips;
		}
	}
}
