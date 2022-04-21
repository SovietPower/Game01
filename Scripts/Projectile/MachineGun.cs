using UnityEngine;

namespace PROJECTILE
{
	public class MachineGun : Projectile
	{
		float minDeltaY = -0.08f;
		float maxDeltaY = 0.08f;

		protected void Start()
		{
			Initialize("Machine Gun");
		}

		public override void Generate()
		{
			base.Generate();

			transform.position = new Vector3(transform.position.x, transform.position.y+Random.Range(minDeltaY, maxDeltaY), transform.position.z);
			rigidbody.velocity = moveSpeed * transform.forward;// transform.rotation;
			// StartCoroutine(MoveDirectlyCoroutine());
		}

	}
}
