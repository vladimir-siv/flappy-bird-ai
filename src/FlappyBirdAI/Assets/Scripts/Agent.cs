using UnityEngine;
using GrandIntelligence;

public static class NeuralBrain
{
	public static NeuralBuilder Prototype { get; }

	static NeuralBrain()
	{
		Prototype = new NeuralBuilder(Shape.As2D(1u, 5u));
		Prototype.FCLayer(8u, ActivationFunction.ELU);
		Prototype.FCLayer(2u, ActivationFunction.Sigmoid);
	}

	public static void Randomize(BasicBrain brain)
	{
		using (var randomize = Device.Active.Prepare("randomize"))
		using (var it = new NeuralIterator())
		{
			randomize.Set('U');
			randomize.Set(-1.0f, 0);
			randomize.Set(+1.0f, 1);

			for (var param = it.Begin(brain.NeuralNetwork); param != null; param = it.Next())
			{
				randomize.Set(param.Memory);
				API.Wait(API.Invoke(randomize.Handle));
			}
		}
	}
}

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

	public void Think(Pipe nearest)
	{
		inputCache[0] = (bird.transform.position.y - 25.5f) / 9f;
		inputCache[1] = bird.body.velocity.y / 10f;

		if (nearest != null)
		{
			inputCache[2] = (nearest.transform.position.x - 20.5f) / 14f;
			inputCache[3] = (nearest.UpperY - 25.5f) / 9f;
			inputCache[4] = (nearest.LowerY - 25.5f) / 9f;
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
