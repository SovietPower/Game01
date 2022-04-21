using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
	public GameObject muzzleFlash;
	public Sprite[] flashSprites;
	public SpriteRenderer[] spriteRenderers;

	float flashTime = 0.07f;

	// 其它
	int spriteIndex, spriteCount;

	void Start()
	{
		Deactivate();

		spriteIndex = 0;
		spriteCount = flashSprites.Length;
	}

	public void Activate()
	{
		foreach (SpriteRenderer renderer in spriteRenderers)
			renderer.sprite = flashSprites[spriteIndex];
		spriteIndex ++;
		if (spriteIndex == spriteCount)
			spriteIndex = 0;

		muzzleFlash.SetActive(true);

		Invoke(nameof(Deactivate), flashTime);
	}

	public void Deactivate()
	{
		muzzleFlash.SetActive(false);
	}
}
