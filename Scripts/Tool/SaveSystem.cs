using System;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
	const string folderPath = "E:\\u3d\\a\\MyData";

	#region JSON

	public static void SaveByJson(string fileName, object data)
	{
		fileName += ".json";
		var json = JsonUtility.ToJson(data, true);
		var path = Path.Combine(folderPath, fileName);

		try
		{
			File.WriteAllText(path, json);

			#if UNITY_EDITOR
			Debug.Log($"Successfully saved data to {path}.");
			#endif
		}
		catch (System.Exception e)
		{
			Debug.LogError($"Failed to save data to {path}.\nError:{e}");
		}
	}
	public static void SaveByJsonWithArray(string fileName, object[] data)//buxing
	{
		foreach (var item in data)
			Debug.Log($"wuwu {item}");
		var json = JsonUtility.ToJson(data);
		var path = Path.Combine(folderPath, fileName);

		try
		{
			File.WriteAllText(path, json);

			#if UNITY_EDITOR
			Debug.Log($"Successfully saved data to {path}.");
			#endif
		}
		catch (System.Exception e)
		{
			Debug.LogError($"Failed to save data to {path}.\nError:{e}");
		}
	}
	public static T LoadFromJson<T>(string fileName)
	{
		fileName += ".json";
		var path = Path.Combine(folderPath, fileName);
		try
		{
			var json = File.ReadAllText(path);
			var data = JsonUtility.FromJson<T>(json);
			return data;
		}
		catch (System.Exception e)
		{
			Debug.LogError($"Failed to load data from {path}.\nError:{e}");
			return default;
		}
	}
	public static void DeleteSaveFile(string fileName)
	{
		var path = Path.Combine(folderPath, fileName);
		try
		{
			File.Delete(path);
		}
		catch (System.Exception e)
		{
			Debug.LogError($"Failed to delete {path}.\nError:{e}");
		}
	}

	#endregion
}
