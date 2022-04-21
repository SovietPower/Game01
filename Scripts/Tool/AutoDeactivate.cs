using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDeactivate : MonoBehaviour
{
	public bool destroyGameObject = true;
	public float lifetime = 3f;

	WaitForSeconds waitForSeconds;

	static int IDForMEC = 0;
	string tagForMEC;

	// MonoBehaviour 运行周期：Awake -> OnEnable -> Reset -> Start -> FixedUpdate
	void Awake()
	{
		waitForSeconds = new WaitForSeconds(lifetime);

		tagForMEC = "AutoDeactivate"+(IDForMEC++);
	}

	void OnEnable()
	{
		// StartCoroutine(DeactivateCoroutine());
		// Timing.RunCoroutine 会有bug。在池中第一次被创建、设为disable时，Coroutine并没有被取消，会一直运行直到lifetime结束，所以必须在OnDisable中取消
		Timing.RunCoroutine(DeactivateCoroutine().CancelWith(gameObject), tagForMEC); // 注意Cancel
	}

	void OnDisable()
	{
		Timing.KillCoroutines(tagForMEC);
	}

	IEnumerator<float> DeactivateCoroutine()
	{
		yield return Timing.WaitForSeconds(lifetime);
		// Timing.WaitForSeconds 返回一个表示时刻的float，与UnityEngine.WaitForSeconds不同，不可提前赋值

		if (destroyGameObject)
			Destroy(gameObject);
		else
			gameObject.SetActive(false);
	}
}
