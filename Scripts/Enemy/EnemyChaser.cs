using MEC;
using UnityEngine;

public class EnemyChaser : Enemy
{
	protected override void OnEnable()
	{
		base.OnEnable();
		Timing.RunCoroutine(FindTargetCo(), tagForMEC);
	}
}
