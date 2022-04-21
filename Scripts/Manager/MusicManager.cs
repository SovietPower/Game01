using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
	int curBGM;
	public AudioClip[] BGMs;

	void Start()
	{
		curBGM = Random.Range(0, BGMs.Length) - 1;
		PlayNextBGM();
	}

	void PlayNextBGM()
	{
		curBGM ++;
		if (curBGM == BGMs.Length)
			curBGM = 0;
		AudioManager.Instance.PlayBGM(BGMs[curBGM]);
		Invoke(nameof(PlayNextBGM), BGMs[curBGM].length - 4f);
	}
}
