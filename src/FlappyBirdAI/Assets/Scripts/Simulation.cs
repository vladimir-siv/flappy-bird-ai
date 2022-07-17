using System.Collections.Generic;
using GrandIntelligence;

public class Simulation
{
	private List<Agent> agents = null;
	private DarwinBgea evolution = null;

	public void Begin(List<Agent> agents)
	{
		this.agents = agents;

		var firstGen = new Population((uint)agents.Count);
		for (var i = 0; i < agents.Count; ++i)
		{
			var brain = new BasicBrain(NeuralBrain.Prototype);
			NeuralBrain.Randomize(brain);
			firstGen.Add(brain);
		}

		evolution = new DarwinBgea
		(
			firstGen,
			Selection.RandFit(1u),
			BasicBrain.Mating(firstGen.Size, ((BasicBrain)firstGen[0u]).NeuralNetwork.Params),
			generations: 1000u,
			mutation: 15.0f
		);
	}

	public void Start()
	{
		PopulateAgentsWithBrains();
	}

	public void BirdTerminated(int birdsLeft)
	{
		if (birdsLeft == 0)
		{
			evolution.Cycle();
			PopulateAgentsWithBrains();
		}
	}

	private void PopulateAgentsWithBrains()
	{
		for (var i = 0; i < agents.Count; ++i)
		{
			agents[i].Brain = (BasicBrain)evolution.Generation[(uint)i];
		}
	}

	public void End()
	{
		NeuralBrain.Prototype?.Dispose();
		evolution?.Dispose();
	}
}
