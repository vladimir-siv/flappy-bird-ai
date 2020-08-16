using UnityEngine;

public static class Extension
{
	public static void MoveYBy(this Transform transform, float y)
	{
		transform.localPosition = new Vector3
		(
			transform.localPosition.x,
			transform.localPosition.y + y,
			transform.localPosition.z
		);
	}

	public static void RescaleY(this Transform transform, float y)
	{
		transform.localScale = new Vector3
		(
			transform.localScale.x,
			y,
			transform.localScale.z
		);
	}
}
