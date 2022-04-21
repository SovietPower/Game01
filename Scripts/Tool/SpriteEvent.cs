using UnityEngine;

public class SpriteEvent : MonoBehaviour
{
	public Color color;

	SpriteRenderer spriteRenderer;

	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	void ChangeColor()
	{
		spriteRenderer.color = color;
	}

	void End()
	{
		gameObject.SetActive(false);
	}
}
