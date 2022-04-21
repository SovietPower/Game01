using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
	public static T Instance { get; private set; }

	// 子类 override Awake() 时，需调用 base.Awake()
	protected virtual void Awake()
	{
		if (Instance == null)
			Instance = this as T;
		else if (Instance != this)
			Destroy(gameObject);
	}
}
