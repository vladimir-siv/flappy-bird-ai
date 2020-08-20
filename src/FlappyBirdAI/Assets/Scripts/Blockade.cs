using UnityEngine;

public class Blockade : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Bird"))
		{
			other.gameObject.GetComponent<Bird>().Terminate();
		}
	}
}
