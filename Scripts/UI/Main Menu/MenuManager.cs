using DATA;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MAINMENU
{
	public class MenuManager : MonoBehaviour
	{
		Transform canvas, mainMenuT, optionsMenuT;

		GameObject mainMenu, optionsMenu;

		int[] resolutions = {8000600, 10240768, 12800960, 16000900, 19201080}; // Weight Height
		Dropdown resolution;

		Toggle fullscreen;

		Slider graphics, musicVolume, SFXVolume;

		void Awake()
		{
			canvas = GameObject.FindGameObjectWithTag("Canvas").transform;

			mainMenuT = canvas.Find("Main Menu");
			optionsMenuT = canvas.Find("Options Menu");
			mainMenu = mainMenuT.gameObject;
			optionsMenu = optionsMenuT.gameObject;

			musicVolume = optionsMenuT.Find("Music Volume").GetComponent<Slider>();
			SFXVolume = optionsMenuT.Find("SFX Volume").GetComponent<Slider>();

			graphics = optionsMenuT.Find("Graphics").GetComponent<Slider>();
			resolution = optionsMenuT.Find("Resolution").GetComponent<Dropdown>();
			fullscreen = optionsMenuT.Find("Fullscreen").GetComponent<Toggle>();

			LoadConfig();
		}

		public void Play()
		{
			SceneManager.LoadScene("Test");
		}

		public void Quit()
		{
			Application.Quit();
		}

		public void ToOptionsMenu()
		{
			mainMenu.SetActive(false);
			optionsMenu.SetActive(true);
		}

		public void ToMainMenu()
		{
			LoadConfig();
			mainMenu.SetActive(true);
			optionsMenu.SetActive(false);
		}

		public void OptionsApply()
		{
			SaveConfig();
			ToMainMenu();
		}

		public void SetResolution()
		{
			int res = resolutions[resolution.value];
			Screen.SetResolution(res/10000, res%10000, false);
		}

		public void SetFullscreen()
		{
			bool isFull = fullscreen.isOn;
			// 当全屏时，强制为最大分辨率
			resolution.interactable = !isFull;

			if (isFull) // 使用最大分辨率
			{
				Resolution[] allResolutions = Screen.resolutions;
				Resolution maxResolution = allResolutions[allResolutions.Length - 1];
				Screen.SetResolution(maxResolution.width, maxResolution.height, true);
			}
			else // 恢复为分辨率设置
				SetResolution();
		}

		public void SetGraphics()
		{
			// not implemented yet
		}

		public void SetMusicVolume()
		{
			AudioManager.Instance.SetBGMVolume(musicVolume.value/100f);
		}

		public void SetSFXVolume()
		{
			AudioManager.Instance.SetSFXVolume(SFXVolume.value/100f);
		}

		// 加载原先的设置
		void LoadConfig()
		{
			PlayerConfig config = Config.Instance.playerConfig;

			resolution.value = config.resolution; SetResolution();
			fullscreen.isOn = config.fullscreen; SetFullscreen();
			graphics.value = config.graphics; SetGraphics();
			musicVolume.value = config.BGMVolume; SetMusicVolume();
			SFXVolume.value = config.SFXVolume; SetSFXVolume();
		}

		// 保存设置
		void SaveConfig()
		{
			PlayerConfig config = Config.Instance.playerConfig;

			config.resolution = resolution.value;
			config.fullscreen = fullscreen.isOn;
			config.graphics = (int)graphics.value;
			config.BGMVolume = (int)musicVolume.value;
			config.SFXVolume = (int)SFXVolume.value;

			Config.Instance.Save();
		}
	}
}