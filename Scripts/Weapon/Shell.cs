using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
	new Rigidbody rigidbody;

	public float forceMin;
	public float forceMax;

	float lifeTime = 2.5f;
	float fadeTime = 1.5f; // 淡出用时

	// 前它
	static int IDForMEC = 0;
	string tagForMEC;

	void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();

		tagForMEC = "Shell"+(IDForMEC++);
	}

	void OnEnable()
	{
		float force = Random.Range(forceMin, forceMax);
		rigidbody.AddForce(transform.right * force);
		rigidbody.AddTorque(Random.insideUnitSphere * force);

		// StartCoroutine(Fade());
		Timing.RunCoroutine(Fade().CancelWith(gameObject));
	}

	IEnumerator<float> Fade()
	{
		yield return Timing.WaitForSeconds(lifeTime);

		float speed = 1 / fadeTime;
		Material mat = GetComponent<Renderer>().material;
		Color originalColor = mat.color;
		for (float percent=0f; percent<1; )
		{
			percent += Time.deltaTime * speed;
			mat.color = Color.Lerp(originalColor, Color.clear, percent);

			yield return 0;
		}

		gameObject.SetActive(false);
	}
}
