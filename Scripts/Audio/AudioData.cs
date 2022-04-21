using UnityEngine;

[CreateAssetMenu(menuName = "My/Audio Data")]
[System.Serializable]
public class AudioData : ScriptableObject
{
	public AudioClip audioClip;
	public float volume = 1f;
}
