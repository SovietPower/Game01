using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MYUI
{
	public class ImageRotation : MonoBehaviour
	{
		public Sprite[] sprites;
		public float rotationTime = 5f;

		int lastSprite = -1;
		Image curImage, hiddenImage; // hiddenImage(Image2) 在 curImage(Image1) 的上层

		void Start()
		{
			curImage = transform.Find("Image1").GetComponent<Image>();
			hiddenImage = transform.Find("Image2").GetComponent<Image>();
			RandomRotate();
		}

		public void RandomRotate()
		{
			int index = Random.Range(0, sprites.Length);
			if (index == lastSprite)
			{
				if (index != 0) index--;
				else index = sprites.Length - 1;
			}
			lastSprite = index;

			Timing.RunCoroutine(ImageFadeIn(sprites[index], 1.7f).CancelWith(gameObject));
			Invoke(nameof(RandomRotate), rotationTime);
		}

		// 淡入一张图片
		IEnumerator<float> ImageFadeIn(Sprite sprite, float time)
		{
			hiddenImage.color = Color.white;
			hiddenImage.sprite = curImage.sprite;
			curImage.sprite = sprite;

			float speed = 1/time;
			for (float percent=0f; percent<1f; )
			{
				percent += Time.deltaTime * speed;
				hiddenImage.color = Color.Lerp(Color.white, Color.clear, percent);

				yield return 0f;
			}
		}
	}
}
