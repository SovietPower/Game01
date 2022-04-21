using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

// 允许一个类能在运行时，实时通过 Inspector 面板更新参数，并立刻反馈。
// 当不展开相应脚本的面板时，不会运行。
[CustomEditor (typeof(MapGenerator))]
public class MapEditor : Editor
{
	public override void OnInspectorGUI()
	{
		// base.OnInspectorGUI(); // 持续更新

		// DrawDefaultInspector() 只在有参数更新时为true
		// GUILayout.Button(str) 创建按钮，且只在按钮按下时为true
		if (DrawDefaultInspector() || GUILayout.Button("Generate Map"))
		{
			MapGenerator map = target as MapGenerator; // 获取Editor在处理的目标
			map.Start();
			map.GenerateMap();
		}
	}
}
#endif
