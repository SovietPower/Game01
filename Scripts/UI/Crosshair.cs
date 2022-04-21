using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
	public LayerMask targetMask;

	SpriteRenderer spriteRenderer;
	Color highlightColor, originalColor;

	Vector3 rotation;

	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		originalColor = spriteRenderer.color;
		highlightColor = Color.red; // new Color(255f, 0f, 45f);

		rotation = Vector3.forward * 40f;
	}

	void Start()
	{
		// Cursor.visible = false;
	}

	void Update()
	{
		// transform.Rotate(rotation * Time.deltaTime);
	}

	// is called on every frame.
	public void DetectTargets(Ray ray)
	{
		if (Physics.Raycast(ray, 100, targetMask))
		{
			transform.Rotate(rotation * Time.deltaTime * 5f);
			spriteRenderer.color = highlightColor;
		}
		else
		{
			transform.Rotate(rotation * Time.deltaTime);
			spriteRenderer.color = originalColor;
		}
	}

	// public void DetectTargets(Ray ray)
	// {
	// 	if (Physics.Raycast(ray, 100, targetMask))
	// 		spriteRenderer.color = highlightColor;
	// 	else
	// 		spriteRenderer.color = originalColor;
	// }
}
