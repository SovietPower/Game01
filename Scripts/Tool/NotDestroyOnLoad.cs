using UnityEngine;

public class NotDestroyOnLoad : MonoBehaviour
{
	void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
}
