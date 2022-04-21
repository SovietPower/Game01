using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ! 注意size, prefab需在相应Pool的Inspector界面修改
// Pool没有继承MonoBehaviour，所以需加[System.Serializable]字段才可将Pool类的可序列化字段暴露出来
[System.Serializable]
public class Pool
{
	public int Size => size;
	public int RuntimeSize => totalSize;
	public GameObject Prefab => prefab;

	// 初始大小
	[SerializeField] int size = 3;
	// 总大小
	int totalSize = 0;
	// 生成对象的预制体
	[SerializeField] protected GameObject prefab;
	// 为生成的对象指定父元素，便于管理
	Transform parent;

	// 与栈实现池相比，队列可以随意扩容？
	Queue<GameObject> queue;

	public void Initialize(Transform parent)
	{
		this.parent = parent;
		// this.prefab = prefab;
		queue = new Queue<GameObject>();

		for (int i = 0; i < size; i++)
			queue.Enqueue(Copy());
	}

	// 新建对象
	GameObject Copy()
	{
		this.totalSize++;

		GameObject copy = GameObject.Instantiate(prefab, parent);
		copy.SetActive(false);
		return copy;
	}

	// 获取可用对象
	GameObject GetAvailableObject()
	{
		GameObject availableObj = (queue.Count > 0 && !queue.Peek().activeSelf) ? queue.Dequeue() : Copy();
		queue.Enqueue(availableObj); // 出队则入队，否则Release会很麻烦

		return availableObj;
	}

	// 获取可用对象
	public GameObject Get()
	{
		GameObject obj = GetAvailableObject();

		obj.SetActive(true);

		return obj;
	}

	// 获取可用对象，并设置状态
	public GameObject Get(Vector3 position)
	{
		GameObject obj = GetAvailableObject();

		obj.transform.position = position;
		obj.SetActive(true);

		return obj;
	}

	// 获取可用对象，并设置状态
	public GameObject Get(Vector3 position, Quaternion rotation)
	{
		GameObject obj = GetAvailableObject();

		obj.transform.position = position;
		obj.transform.rotation = rotation;
		obj.SetActive(true);

		return obj;
	}

	// 获取可用对象，并设置状态
	public GameObject Get(Vector3 position, Quaternion rotation, Vector3 localScale)
	{
		GameObject obj = GetAvailableObject();

		obj.transform.position = position;
		obj.transform.rotation = rotation;
		obj.transform.localScale = localScale;
		obj.SetActive(true);

		return obj;
	}

	// 获取可用对象
	public GameObject GetInactive()
	{
		return GetAvailableObject();
	}

	// 获取可用对象，并设置状态
	public GameObject GetInactive(Vector3 position)
	{
		GameObject obj = GetAvailableObject();

		obj.transform.position = position;

		return obj;
	}

	// 获取可用对象，并设置状态
	public GameObject GetInactive(Vector3 position, Quaternion rotation)
	{
		GameObject obj = GetAvailableObject();

		obj.transform.position = position;
		obj.transform.rotation = rotation;

		return obj;
	}

	// 获取可用对象，并设置状态
	public GameObject GetInactive(Vector3 position, Quaternion rotation, Vector3 localScale)
	{
		GameObject obj = GetAvailableObject();

		obj.transform.position = position;
		obj.transform.rotation = rotation;
		obj.transform.localScale = localScale;

		return obj;
	}

	// 释放对象，放回池
	// public void Release(GameObject obj)
	// {
	// 	obj.SetActive(false);
	// 	// queue.Enqueue(obj);
	// }
}
