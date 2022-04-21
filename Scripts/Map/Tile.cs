using UnityEngine;

public class Tile : MonoBehaviour
{
	public Vector3 position;

	public Material rendererMaterial;
	public Color originalColor;

	void Awake()
	{
		position = transform.position;

		rendererMaterial = GetComponent<Renderer>().material;
		originalColor = rendererMaterial.color;
	}
}
