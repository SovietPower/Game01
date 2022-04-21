using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyTools
{
	public static void Swap<T> (ref T x, ref T y)
	{ // 注意必须加 ref 才可传递引用
		T temp = x;
		x = y; y = temp;
	}

	public static T[] RandomShuffle<T> (T[] array, int seed)
	{
		System.Random seq = new System.Random(seed);
		for (int i=0,l=array.Length; i+1<l; ++i)
		{
			int randomIndex = seq.Next(i, l);
			Swap(ref array[randomIndex], ref array[i]);
		}
		return array;
	}

	// 在锥形中获取随机方向（若获取旋转角度，需*180f。
	public static Vector3 RandomInsideCone(float radius) // 半径越小，越收束
	{
		float radradius = radius * Mathf.Deg2Rad * 0.5f; // Mathf.PI / 360;
		float z = Random.Range(Mathf.Cos(radradius), 1);
		float t = Random.Range(0, Mathf.PI * 2);
		// return transform.TransformDirection((new Vector3(Mathf.Sqrt(1 - z * z) * Mathf.Cos(t), Mathf.Sqrt(1 - z * z) * Mathf.Sin(t), z)).normalized);
		return new Vector3(Mathf.Sqrt(1 - z * z) * Mathf.Cos(t), Mathf.Sqrt(1 - z * z) * Mathf.Sin(t), z);
	}

	// 在锥形中获取随机方向（若获取旋转角度，需*180f。
	public static Vector3 RandomInsideCone2(float z) // z越大，半径越小，越收束
	{
		//  Generate a random XY point inside a circle:
		Vector3 direction = Random.insideUnitCircle;
		direction.z = z; // circle is at Z units
		// direction = transform.TransformDirection( direction.normalized );
		return direction.normalized;
	}
}
