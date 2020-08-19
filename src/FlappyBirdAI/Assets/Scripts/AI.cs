using System;
using System.Collections.Generic;

using GrandIntelligence;

public static class GILibrary
{
	private static bool Initialized = false;

	public static void Init()
	{
		if (Initialized) return;
		Initialized = true;
		GICore.Init(new Spec(GrandIntelligence.DeviceType.Cpu));
	}

	public static void Release()
	{
		if (!Initialized) return;
		Initialized = false;
		Agents.Release();
		GICore.Release();
	}
}

public static class Evolution
{
	private static DateTime StartTime;

	public static void Begin()
	{
		StartTime = DateTime.Now;
	}

	public static float Progress()
	{
		var time = (float)(DateTime.Now - StartTime).TotalMilliseconds;
		return time;
		//return Mathf.Pow(time, 4f) + 1f;
	}
}

public static class Agents
{
	private static List<Agent> agents = null;
	private static DarwinBgea evolution = null;
	private static NeuralBuilder prototype = null;

	public static NeuralBuilder Prototype => prototype;

	public static void Create(int agentCount)
	{
		if (agents != null) return;

		prototype = new NeuralBuilder(5u);
		prototype.FCLayer(8u, ActivationFunction.ELU);
		prototype.FCLayer(2u, ActivationFunction.Sigmoid);

		agents = new List<Agent>(agentCount);

		var first = new Population((uint)agentCount);

		for (var i = 0; i < agentCount; ++i)
		{
			var agent = new Agent();
			agents.Add(agent);
			first.Add(agent.Brain);
		}

		evolution = new DarwinBgea
		(
			first,
			Selection.RandFit(1u),
			BasicBrain.Mating(first.Size, ((BasicBrain)first[0u]).NeuralNetwork.Params),
			generations: 1000u,
			mutation: 10.0f
		);
	}
	public static void Release()
	{
		prototype.Dispose();
		prototype = null;

		evolution.Dispose();
		evolution = null;

		agents = null;
	}

	public static void AssignBird(Bird bird, int index)
	{
		agents[index].Bird = bird;
	}

	public static void Think()
	{
		var pipe = Pipes.Peek();
		for (var i = 0; i < agents.Count; ++i)
			agents[i].Think(pipe);
	}

	public static void Cycle()
	{
		evolution.Cycle();
		
		for (var i = 0; i < agents.Count; ++i)
		{
			agents[i].Brain = (BasicBrain)evolution.Generation[(uint)i];
		}
	}
}

public sealed class Agent
{
	private float[] inputCache = new float[5];

	private BasicBrain brain = null;
	public BasicBrain Brain
	{
		get => brain;
		set => brain = value;
	}

	private Bird bird = null;
	public Bird Bird
	{
		get
		{
			return bird;
		}
		set
		{
			if (value == bird) return;
			bird = value;
			if (bird == null) return;
			bird.Terminated += BirdTerminated;
		}
	}

	public Agent()
	{
		Brain = new BasicBrain(Agents.Prototype);
		using (var randomize = Device.Active.Prepare("randomize"))
		using (var it = new NeuralIterator())
		{
			randomize.Set('U');
			randomize.Set(-1.0f, 0);
			randomize.Set(+1.0f, 1);

			for (var param = it.Begin(Brain.NeuralNetwork); param != null; param = it.Next())
			{
				randomize.Set(param.Memory);
				API.Wait(API.Invoke(randomize.Handle));
			}
		}
	}

	public void Think(Pipe nearest)
	{
		if (Bird == null) return;

		inputCache[0] = (Bird.transform.position.y - 25.5f) / 9f;
		inputCache[1] = Bird.body.velocity.y / 10f;

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

		if (out1 > out2) Bird.Jump();
	}

	private void BirdTerminated(Bird bird)
	{
		if (bird != this.bird) return;
		Bird = null;
		Brain.EvolutionValue = Evolution.Progress();
	}
}
