using UnityEngine;

// 水平巡逻
// [CreateAssetMenu(menuName = "My/Patrol Path X")]
public class PatrolPathX : MonoBehaviour
{
	public float start, end;

	void Reset()
	{
		start = 0;
		end = 1;
	}
}
