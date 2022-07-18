using UnityEngine;

public sealed class Agent
{
	private Bird bird = null;

	public void AssignBird(Bird bird)
	{
		this.bird = bird;
	}

	public void Think()
	{
		if (Random.Range(0, 100) < 2) bird.Jump();
	}
}
