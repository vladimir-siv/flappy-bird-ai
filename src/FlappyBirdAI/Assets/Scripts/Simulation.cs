using GrandIntelligence;

public class Simulation
{
	private Agent[] agents = null;
	private DarwinBgea evolution = null;

	public void Begin(Agent[] agents)
	{
		this.agents = agents;

		var prototype = new NeuralBuilder(Shape.As2D(1u, 5u));
		prototype.FCLayer(8u, ActivationFunction.ELU);
		prototype.FCLayer(2u, ActivationFunction.Sigmoid);

		var firstGen = new Population((uint)agents.Length);
		for (var i = 0; i < agents.Length; ++i)
		{
			var brain = new BasicBrain(prototype);
			brain.Randomize(-1.0f, 1.0f, Distribution.Uniform);
			firstGen.Add(brain);
		}

		evolution = new DarwinBgea
		(
			firstGen,
			Selection.RandFit(1u),
			BasicBrain.SequentialEvenCrossover(firstGen.Size, ((BasicBrain)firstGen[0u]).NeuralNetwork.Params),
			generations: 1000u,
			mutation: 15.0f
		);

		prototype.Dispose();
	}

	public void EpisodeStart()
	{
		for (var i = 0; i < agents.Length; ++i)
		{
			agents[i].Brain = (BasicBrain)evolution.Generation[(uint)i];
		}
	}

	public void BirdTerminated(int birdsLeft)
	{
		if (birdsLeft == 0)
		{
			evolution.Cycle();
		}
	}

	public void End()
	{
		evolution?.Dispose();
	}
}
