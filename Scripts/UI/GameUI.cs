using MEC;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
	Image gameOverBg;
	GameObject gameOverUI;

	RectTransform newLevelBanner; // RectTransform是Transform用于2d的子类，多了Anchors和pivot
	TextMeshProUGUI  newLevelBannerTitle;
	TextMeshProUGUI  newLevelBannerEnemy;

	EnemySpawner enemySpawner;

	void Awake()
	{
		gameOverBg = transform.Find("BGs/Game Over BG").GetComponent<Image>(); // 注意不是递归查，需要指明路径
		gameOverUI = transform.Find("Game Over UI").gameObject;

		newLevelBanner = transform.Find("New Level Banner") as RectTransform;
		newLevelBannerTitle = newLevelBanner.Find("Title").GetComponent<TextMeshProUGUI >();
		newLevelBannerEnemy = newLevelBanner.Find("Enemy").GetComponent<TextMeshProUGUI >();

		enemySpawner = FindObjectOfType<EnemySpawner>();
		enemySpawner.OnNewLevel += OnNewLevel;

		FindObjectOfType<Player>().OnDeath += OnPlayerDeath; // todo OnGameOver not OnDeath
	}

	// void Start()
	// {
	// }

	#region Game
	void OnNewLevel(int levelNumber)
	{
		newLevelBannerTitle.text = "- Level "+levelNumber+" -";
		newLevelBannerEnemy.text = "Enemies: "+enemySpawner.curLevel.EnemyCount;

		Timing.RunCoroutine(MoveInAndOut(newLevelBanner, 1.7f, 1.7f, new Vector2(0, -270f), new Vector2(0, -40f)).CancelWith(newLevelBanner.gameObject));
		// 可能需要先Kill之前的Coroutine，但正常情况下不会有这个问题
	}

	void OnPlayerDeath()
	{
		OnGameOver();
	}

	void OnGameOver()
	{
		Timing.RunCoroutine(Fade(gameOverBg, Color.clear, Color.black, 1f).CancelWith(gameOverBg.gameObject));
		gameOverUI.SetActive(true);
	}
	#endregion

	#region UIInput
	public void StartNewGame()
	{
		SceneManager.LoadScene("Test");
	}

	public void ToMainMenu()
	{
		SceneManager.LoadScene("Main Menu");
	}

	#endregion

	#region Utility
	// (3D)将一个对象从某位置移动到另一位置（沿直线，一般为水平或竖直），等待waitTime后返回原位置。
	IEnumerator<float> MoveInAndOut(Transform transform, float speed, float waitTime, Vector3 from, Vector3 to)
	{
		for (float percent=0f; percent>=0f; )
		{
			percent += Time.deltaTime * speed;
			if (percent >= 1f)
			{
				percent = 1f;
				speed *= -1;
				yield return Timing.WaitForSeconds(waitTime);
			}
			transform.position = Vector3.Lerp(from, to, percent);

			yield return 0f;
		}
	}

	// (2D)
	IEnumerator<float> MoveInAndOut(RectTransform transform, float speed, float waitTime, Vector2 from, Vector2 to)
	{
		for (float percent=0f; percent>=0f; )
		{
			percent += Time.deltaTime * speed;
			if (percent >= 1f)
			{
				percent = 1f;
				speed *= -1;
				yield return Timing.WaitForSeconds(waitTime);
			}
			// transform.position = Vector2.Lerp(from, to, percent);
			transform.anchoredPosition = Vector2.Lerp(from, to, percent);

			yield return 0f;
		}
	}

	// 某画面从颜色from到to的渐变，用时为time=1/speed
	IEnumerator<float> Fade(Image image, Color from, Color to, float speed)
	{
		for (float percent=0; percent<1; )
		{
			percent += Time.deltaTime * speed;
			image.color = Color.Lerp(from, to, percent);

			yield return 0f;
		}
	}
	#endregion
}
