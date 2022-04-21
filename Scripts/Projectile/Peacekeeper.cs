
namespace PROJECTILE
{
	public class Peacekeeper : Projectile
	{
		protected void Start()
		{
			// Initialize("Peacekeeper");
		}

		public override void Generate()
		{
			base.Generate();
			rigidbody.velocity = moveSpeed * transform.forward;
		}
	}
}
