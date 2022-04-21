using DATA;
using AUDIO;
using System.Collections;
using UnityEngine;

// ! 拥有较高优先级
// 将其Dont Destroy On Load。这样在切换场景时音乐不会打断，设置也不会改变
// 但需要跟踪Player的位置，所以切换场景后要告诉AudioManager Player已生成，以便更新playerT（在OnNewLevel时调用AudioManager的UpdatePlayer）。
// todo 应为OnNewGame not OnNewLevel，即只有场景切换才需要。
// 所以Dont Destroy On Load的对象，不能通过添加委托调用，只能在对应位置调用该对象的函数。
public class AudioManager : PersistentSingleton<AudioManager>
{
	const float MIN_PITCH = 0.95f;
	const float MAX_PITCH = 1.05f;

	AudioSource SFXPlayer;
	int BGMPlayerID;
	AudioSource BGMPlayer;
	AudioSource[] BGMPlayers;

	// 在 MainMenu 中加载配置
	float SFXVolume = 1f, BGMVolume = 0.3f;

	Transform audioListener, playerT;

	AudioLibrary library;

	#region LifeCycle
	protected override void Awake()
	{
		base.Awake();

		SFXPlayer = NewAudioSource("SFXPlayer");

		BGMPlayers = new AudioSource[2];
		for (int i=0; i<2; i++)
			BGMPlayers[i] = NewAudioSource("BGMPlayer " + (i+1));

		// audioListener = FindObjectOfType<AudioListener>().transform;
		Player player = FindObjectOfType<Player>();
		if (player != null) playerT = player.transform;

		library = GetComponent<AudioLibrary>();
	}

	void Update()
	{
		// 默认播放位置为玩家所在位置
		// audioListener也跟踪玩家位置（不附在Player上，避免同Player一起被disable）
		if (playerT != null)
			transform.position = playerT.position;
	}

	// 最好在足够延时后调用，避免玩家未生成？应该不会。
	public void UpdatePlayer()
	{
		playerT = FindObjectOfType<Player>().transform;
	}

	AudioSource NewAudioSource(string name)
	{
		GameObject newAudioSource = new GameObject(name);
		newAudioSource.transform.parent = transform;
		return newAudioSource.AddComponent<AudioSource>();
	}
	#endregion

	#region Config
	public float GetSFXVolume() => SFXVolume;
	public float GetBGMVolume() => BGMVolume;

	public void SetSFXVolume(float volume) => SFXVolume = volume;

	public void SetBGMVolume(float volume)
	{
		BGMVolume = volume;
		BGMPlayers[0].volume = volume; BGMPlayers[1].volume = volume;
	}

	#endregion

	#region Play
	// Used for single or UI SFX
	public void PlaySFX(AudioClip audio)
	{
		SFXPlayer.PlayOneShot(audio, SFXVolume);
	}

	// Used for repeat-play SFX
	public void PlayRepeatSFX(AudioClip audio)
	{
		SFXPlayer.pitch = Random.Range(MIN_PITCH, MAX_PITCH);
		SFXPlayer.PlayOneShot(audio, SFXVolume);
	}

	// Play random repeat-play SFX
	public void PlayRandomSFX(AudioClip[] audio)
	{
		PlayRepeatSFX(audio[Random.Range(0, audio.Length)]);
	}

	// Used for single or UI SFX
	public void PlaySFX(AudioClip audio, Vector3 pos)
	{
		AudioSource.PlayClipAtPoint(audio, pos, SFXVolume);
	}

	// Used for repeat-play SFX
	public void PlayRepeatSFX(AudioClip audio, Vector3 pos)
	{
		SFXPlayer.pitch = Random.Range(MIN_PITCH, MAX_PITCH);
		AudioSource.PlayClipAtPoint(audio, pos, SFXVolume);
	}

	// Play random repeat-play SFX
	public void PlayRandomSFX(AudioClip[] audio, Vector3 pos)
	{
		PlayRepeatSFX(audio[Random.Range(0, audio.Length)], pos);
	}

	// Used for single or UI SFX
	public void PlaySFX(AudioClip audio, float volume)
	{
		SFXPlayer.PlayOneShot(audio, volume * SFXVolume);
	}

	// Used for repeat-play SFX
	public void PlayRepeatSFX(AudioClip audio, float volume)
	{
		SFXPlayer.pitch = Random.Range(MIN_PITCH, MAX_PITCH);
		SFXPlayer.PlayOneShot(audio, volume * SFXVolume);
	}

	// Play random repeat-play SFX
	public void PlayRandomSFX(AudioClip[] audio, float volume)
	{
		PlayRepeatSFX(audio[Random.Range(0, audio.Length)], volume);
	}

	// Used for single or UI SFX
	public void PlaySFX(AudioClip audio, Vector3 pos, float volume)
	{
		AudioSource.PlayClipAtPoint(audio, pos, volume * SFXVolume);
	}

	// Used for repeat-play SFX
	public void PlayRepeatSFX(AudioClip audio, Vector3 pos, float volume)
	{
		SFXPlayer.pitch = Random.Range(MIN_PITCH, MAX_PITCH);
		AudioSource.PlayClipAtPoint(audio, pos, volume * SFXVolume);
	}

	// Play random repeat-play SFX
	public void PlayRandomSFX(AudioClip[] audio, Vector3 pos, float volume)
	{
		PlayRepeatSFX(audio[Random.Range(0, audio.Length)], pos, volume);
	}

	// 淡入播放BGM。不使用AudioData。
	public void PlayBGM(AudioClip audio, float fadeDuration = 4f)
	{
		BGMPlayerID = 1 - BGMPlayerID;
		BGMPlayer = BGMPlayers[BGMPlayerID];

		BGMPlayer.clip = audio;
		BGMPlayer.Play();

		StartCoroutine(BGMCrossFade(fadeDuration));
	}
	#endregion

	#region PlayFromLibrary
	// Used for single or UI SFX in AudioLibrary
	public void PlaySFXInLibrary(string id, GroupType groupType)
	{
		AudioClip audio = library.GetClip(id, groupType);
		if (audio != null)
			PlaySFX(audio);
		else
			Debug.LogError($"No Audio for {id} {groupType}");
	}

	// Used for single or UI SFX in AudioLibrary
	public void PlaySFXInLibrary(string id, GroupType groupType, float volume)
	{
		AudioClip audio = library.GetClip(id, groupType);
		if (audio != null)
			PlaySFX(audio, volume);
		else
			Debug.LogError($"No Audio for {id} {groupType}");
	}

	// Used for single or UI SFX in AudioLibrary
	public void PlaySFXInLibrary(string id, GroupType groupType, Vector3 pos)
	{
		AudioClip audio = library.GetClip(id, groupType);
		if (audio != null)
			PlaySFX(audio, pos);
		else
			Debug.LogError($"No Audio for {id} {groupType}");
	}

	// Used for single or UI SFX in AudioLibrary
	public void PlaySFXInLibrary(string id, GroupType groupType, Vector3 pos, float volume)
	{
		AudioClip audio = library.GetClip(id, groupType);
		if (audio != null)
			PlaySFX(audio, pos, volume);
		else
			Debug.LogError($"No Audio for {id} {groupType}");
	}
	#endregion

	#region Utility
	// 两首BGM的淡入淡出切换。用时为time
	IEnumerator BGMCrossFade(float time)
	{
		float speed = 1f/time;
		for (float percent=0f; percent<1f; )
		{
			percent += Time.deltaTime * speed;
			BGMPlayer.volume = Mathf.Lerp(0f, BGMVolume, percent);
			BGMPlayers[1 - BGMPlayerID].volume = Mathf.Lerp(BGMVolume, 0f, percent);

			yield return null;
		}
	}
	#endregion

}
	// Used for single or UI SFX
	// public void PlaySFX(AudioData audio)
	// {
	// 	SFXPlayer.PlayOneShot(audio.audioClip, audio.volume);
	// }

	// // Used for repeat-play SFX
	// public void PlayRepeatSFX(AudioData audio)
	// {
	// 	SFXPlayer.pitch = Random.Range(MIN_PITCH, MAX_PITCH);
	// 	// PlaySFX(audio);
	// 	SFXPlayer.PlayOneShot(audio.audioClip, audio.volume);
	// }

	// public void PlayRandomSFX(AudioData[] audio)
	// {
	// 	PlayRepeatSFX(audio[Random.Range(0, audio.Length)]);
	// }

// [System.Serializable] public class AudioData
// {
// 	public AudioClip audioClip;
// 	public float volume;
// }
