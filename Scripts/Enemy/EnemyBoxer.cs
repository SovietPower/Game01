using MEC;
using UnityEngine;

public class EnemyBoxer : Enemy
{
	protected override void OnEnable()
	{
		base.OnEnable();
		Timing.RunCoroutine(FindTargetCo(), tagForMEC);
	}

	void Update()
	{
		TryRangeAttack();
	}
}
