using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace MyTest
{
	public class TEST2 : MonoBehaviour
	{
		public RectTransform parent;
		public RectTransform image;
		public Canvas canvas;
		public Camera world, ui;
		public GameObject obj;

		void Update()
		{
			// Vector2 pos = Mouse.current.position.ReadValue();
			Vector2 pos = new Vector3(-10.5f, 0, -7.5f);
			pos = Camera.main.WorldToScreenPoint(pos);

			// var screen = world.WorldToScreenPoint(Camera.main.WorldToScreenPoint(obj.transform.position));
			// screen.z = (canvas.transform.position - ui.transform.position).magnitude;
			// var position = ui.ScreenToWorldPoint(screen);
			// image.position = position;

			Vector2 res;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), pos, null, out res);
			image.anchoredPosition = res;

			print(pos);
		}


	}
	// public class TEST2 : MonoBehaviour
	// {
	// 	public float z;
	// 	public float radius;
	// 	static int IDForMEC = 0;
	// 	string tagForMEC;

	// 	void Start()
	// 	{
	// 		// StartCoroutine(Co(3f));
	// 		// StartCoroutine(Co(5f));
	// 		// StartCoroutine(Co(7f));
	// 		tagForMEC = "TEST2"+(IDForMEC++);

	// 		Timing.RunCoroutine(Co2(3f).CancelWith(gameObject));
	// 		Timing.RunCoroutine(Co2(5f).CancelWith(gameObject));
	// 		Timing.RunCoroutine(Co2(7f).CancelWith(gameObject));

	// 		// Timing.RunCoroutine(Co2(3f).CancelWith(gameObject), tagForMEC);
	// 		// Timing.RunCoroutine(Co2(5f).CancelWith(gameObject), tagForMEC);
	// 		// Timing.RunCoroutine(Co2(7f).CancelWith(gameObject), tagForMEC);
	// 	}

	// 	void OnDisable()
	// 	{
	// 		// StopAllCoroutines();
	// 		// Timing.KillCoroutines(tagForMEC);
	// 	}

	// 	IEnumerator Co(float time)
	// 	{
	// 		yield return new WaitForSeconds(time);
	// 		print("qwq");
	// 	}

	// 	IEnumerator<float> Co2(float time)
	// 	{
	// 		yield return Timing.WaitForSeconds(time);
	// 		print("qwq");
	// 	}
	// }
}