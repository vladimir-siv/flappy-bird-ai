using UnityEngine;
using GrandIntelligence;

public sealed class Agent
{
	private readonly float[] inputCache = new float[5];
	private Bird bird = null;

	public BasicBrain Brain { get; set; }

	public void AssignBird(Bird bird)
	{
		this.bird = bird;
		bird.Terminated += BirdTerminated;
	}

	public void Think()
	{
		var pipe = Pipes.Peek();

		inputCache[0] = (bird.transform.position.y - 25.5f) / 9f;
		inputCache[1] = bird.body.velocity.y / 10f;

		if (pipe != null)
		{
			inputCache[2] = (pipe.transform.position.x - 20.5f) / 14f;
			inputCache[3] = (pipe.UpperY - 25.5f) / 9f;
			inputCache[4] = (pipe.LowerY - 25.5f) / 9f;
		}
		else
		{
			inputCache[2] = 1f;
			inputCache[3] = 1f;
			inputCache[4] = 0f;
		}

		Brain.NeuralNetwork.Input.Transfer(inputCache);
		Brain.NeuralNetwork.Eval();
		var out1 = Brain.NeuralNetwork.Output[0u];
		var out2 = Brain.NeuralNetwork.Output[1u];

		if (out1 > out2) bird.Jump();
	}

	private void BirdTerminated()
	{
		var time = (float)bird.TimeSinceSpawned.TotalSeconds;
		var reward = Mathf.Pow(time / 100f, 4f);

		var pipe = Pipes.Peek();
		var pipeCenter = (pipe.UpperY + pipe.LowerY) / 2f;
		var penalty = Mathf.Pow((bird.transform.position.y - pipeCenter) / 100f, 4f);

		Brain.EvolutionValue = Mathf.Max(reward - penalty, 1e-10f);
	}
}
