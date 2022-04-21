using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
	new Collider collider;

	protected virtual void Awake()
	{
		if (GetComponent<BoxCollider>() != null)
			collider = GetComponent<BoxCollider>();
		else
			collider = GetComponent<CapsuleCollider>();

		Debug.Assert(collider != null);
	}

	void OnTriggerEnter(Collider collider)
	{
		if (collider.CompareTag("Player"))
		{
			collider.GetComponentInParent<Player>().Collect(this);
			gameObject.SetActive(false);
		}
	}
}
