using System.Collections.Generic;
using UnityEngine;

// ! 每个pool需在Inspector填入 prefab, size
// 可以声明不同名称的Pool[]，便于分类。但注意不同池的prefab.name不要相同。可使用更多的Manager。
public class VFXPool : MonoBehaviour
{
	// [SerializeField] Pool[] hitVFXPools;
	[SerializeField] Pool[] deathVFXPools;

	// 为 预制体（名称）-预制体池 建立映射，便于使用多个池
	static Dictionary<string, Pool> dict;

	const string pool_str = "Pool: ";

	void Awake()
	{
		dict = new Dictionary<string, Pool>();

		// ! 可以声明不同名称的Pool[]，便于分类。每个不同类的Pools，都需Initialize、CheckPoolSize
		Initialize(deathVFXPools);
	}

	void OnDestroy()
	{
		#if UNITY_EDITOR
		CheckPoolSize(deathVFXPools);
		#endif
	}

	void Initialize(Pool[] pools)
	{
		foreach (var pool in pools)
		{
		#if UNITY_EDITOR
			if (dict.ContainsKey(pool.Prefab.name))
			{
				Debug.LogError("Same prefab in multiple pools. Prefab: "+pool.Prefab.name);
				continue;
			}
		#endif

			dict.Add(pool.Prefab.name, pool);

			Transform poolParent = new GameObject(pool_str+pool.Prefab.name).transform;
			poolParent.parent = transform;
			pool.Initialize(poolParent);
		}
	}

	void CheckPoolSize(Pool[] pools)
	{
		foreach (var pool in pools)
			if (pool.RuntimeSize > pool.Size)
				Debug.LogWarning($"Pool {pool.Prefab.name} has a runtime size {pool.RuntimeSize} bigger than its initial size {pool.Size}!");
	}

	// 根据prefab.name，取出对象池中的一个元素
	public static GameObject Get(string name)
	{
	#if UNITY_EDITOR
		if (!dict.ContainsKey(name))
		{
			Debug.LogError("Pool Manager could NOT find prefab: "+name);
			return null;
		}
	#endif
		return dict[name].Get();
	}

	// 根据prefab.name，取出对象池中的一个元素，放置于指定位置
	public static GameObject Get(string name, Vector3 position)
	{
	#if UNITY_EDITOR
		if (!dict.ContainsKey(name))
		{
			Debug.LogError("Pool Manager could NOT find prefab: "+name);
			return null;
		}
	#endif
		return dict[name].Get(position);
	}

	// 根据prefab.name，取出对象池中的一个元素，放置于指定位置
	public static GameObject Get(string name, Vector3 position, Quaternion rotation)
	{
	#if UNITY_EDITOR
		if (!dict.ContainsKey(name))
		{
			Debug.LogError("Pool Manager could NOT find prefab: "+name);
			return null;
		}
	#endif
		return dict[name].Get(position, rotation);
	}

	// 根据prefab.name，取出对象池中的一个元素，放置于指定位置
	public static GameObject Get(string name, Vector3 position, Quaternion rotation, Vector3 localScale)
	{
	#if UNITY_EDITOR
		if (!dict.ContainsKey(name))
		{
			Debug.LogError("Pool Manager could NOT find prefab: "+name);
			return null;
		}
	#endif
		return dict[name].Get(position, rotation, localScale);
	}

	public static GameObject GetInactive(string name)
	{
	#if UNITY_EDITOR
		if (!dict.ContainsKey(name))
		{
			Debug.LogError("Pool Manager could NOT find prefab: "+name);
			return null;
		}
	#endif
		return dict[name].GetInactive();
	}

	public static GameObject GetInactive(string name, Vector3 position)
	{
	#if UNITY_EDITOR
		if (!dict.ContainsKey(name))
		{
			Debug.LogError("Pool Manager could NOT find prefab: "+name);
			return null;
		}
	#endif
		return dict[name].GetInactive(position);
	}

	public static GameObject GetInactive(string name, Vector3 position, Quaternion rotation)
	{
	#if UNITY_EDITOR
		if (!dict.ContainsKey(name))
		{
			Debug.LogError("Pool Manager could NOT find prefab: "+name);
			return null;
		}
	#endif
		return dict[name].GetInactive(position, rotation);
	}

	public static GameObject GetInactive(string name, Vector3 position, Quaternion rotation, Vector3 localScale)
	{
	#if UNITY_EDITOR
		if (!dict.ContainsKey(name))
		{
			Debug.LogError("Pool Manager could NOT find prefab: "+name);
			return null;
		}
	#endif
		return dict[name].GetInactive(position, rotation, localScale);
	}
}
