using UnityEngine;
using GrandIntelligence;

public enum Distribution
{
	Uniform = 0,
	Normal = 1
}

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

	public static void Randomize(this BasicBrain brain, float min, float max, Distribution distribution)
	{
		using (var randomize = Device.Active.Prepare("randomize"))
		using (var it = new NeuralIterator())
		{
			char dist = 'U';

			switch (distribution)
			{
				case Distribution.Uniform: dist = 'U'; break;
				case Distribution.Normal: dist = 'N'; break;
			}

			randomize.Set(dist);
			randomize.Set(min, 0);
			randomize.Set(max, 1);

			for (var param = it.Begin(brain.NeuralNetwork); param != null; param = it.Next())
			{
				randomize.Set(param.Memory);
				API.Wait(API.Invoke(randomize.Handle));
			}
		}
	}
}
