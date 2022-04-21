[System.Serializable]
public class ProjectileData
{
	public int id;
	public string name;

	// 基本数值直接在Object中改更方便。
	// 需要在对象池中获取的，使用 Data 中的名称。
	public float moveSpeed;
	// public int damage;

	// 视觉效果
	public string floatDamageName;
	public string hitVFXName; // todo string[]
	public bool solidHitVFX;


}
