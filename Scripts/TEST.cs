using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MyTest
{
	[ExecuteInEditMode]
	public class TEST : MonoBehaviour
	{
		public float z;
		public float radius;

		void Update()
		{
			Run();
		}

		// void OnDisable()
		// {
		// 	StopAllCoroutines();
		// }

		public void Run()
		{
			for (int i=0; i<12; ++i)
				RandomInsideCone(radius);
		}

		public Vector3 RandomInsideCone(float radius)
		{
			//(sqrt(1 - z^2) * cosϕ, sqrt(1 - z^2) * sinϕ, z)
			float radradius = radius * Mathf.PI / 360;
			float z = Random.Range(Mathf.Cos(radradius), 1);
			float t = Random.Range(0, Mathf.PI * 2);

			var direction = new Vector3(Mathf.Sqrt(1 - z * z) * Mathf.Cos(t), Mathf.Sqrt(1 - z * z) * Mathf.Sin(t), z);
			Debug.Log($"{direction}");
			Ray r = new Ray( transform.position, direction );
			RaycastHit hit;
			Debug.DrawLine(transform.position, transform.position + direction*20f, Color.green);
			if( Physics.Raycast( r, out hit ) ) {
				Debug.DrawLine( transform.position, hit.point , Color.red);
			}

			return new Vector3(Mathf.Sqrt(1 - z * z) * Mathf.Cos(t), Mathf.Sqrt(1 - z * z) * Mathf.Sin(t), z);
		}

		public Vector3 ShootRay(float z)
		{
			//  Generate a random XY point inside a circle:
			Vector3 direction = Random.insideUnitCircle;
			direction.z = z; // circle is at Z units
			direction = transform.TransformDirection( direction.normalized );

			//Raycast and debug
			Ray r = new Ray( transform.position, direction );
			RaycastHit hit;
			Debug.DrawLine(transform.position, transform.position + direction*20f, Color.green);
			if( Physics.Raycast( r, out hit ) ) {
				Debug.DrawLine( transform.position, hit.point , Color.red);
			}

			return direction;
		}
	}

	#if UNITY_EDITOR
	[CustomEditor (typeof(TEST))]
	public class TESTEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			if (DrawDefaultInspector())
			{
				MyTest.TEST t = target as MyTest.TEST; // 获取Editor在处理的目标
				t.Run();
			}
		}
	}
	#endif
}