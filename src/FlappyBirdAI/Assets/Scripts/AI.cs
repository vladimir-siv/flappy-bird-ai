using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
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
		var time = (float)(DateTime.Now - StartTime).TotalSeconds;
		return Mathf.Pow(time / 100f, 4f);
	}
}

public static class Agents
{
	private static List<Agent> agents = null;
	private static DarwinBgea evolution = null;
	private static NeuralBuilder prototype = null;

	public static NeuralBuilder Prototype
	{
		get
		{
			if (prototype == null)
			{
				prototype = new NeuralBuilder(5u);
				prototype.FCLayer(8u, ActivationFunction.ELU);
				prototype.FCLayer(2u, ActivationFunction.Sigmoid);
			}

			return prototype;
		}
	}
	public static BasicBrain Best { get; set; }

	public static uint CurrentGeneration => evolution?.CurrentGeneration ?? 0u;

	public static void Create(int agentCount)
	{
		if (agents != null) return;

		agents = new List<Agent>(agentCount);

		if (agentCount == 1) Preload(AgentManager.Chosen);
		if (agentCount <= 1) return;

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
			mutation: 15.0f
		);
	}
	public static void Release()
	{
		prototype?.Dispose();
		prototype = null;

		evolution?.Dispose();
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
		if (evolution == null) return;

		Best.Save(AgentManager.SavePath);

		evolution.Cycle();
		
		for (var i = 0; i < agents.Count; ++i)
		{
			agents[i].Brain = (BasicBrain)evolution.Generation[(uint)i];
		}
	}

	public static void SaveExisting()
	{
		var k = 0;

		foreach (var agent in agents)
		{
			if (agent.Bird != null)
			{
				agent.Brain.Save(AgentManager.UserPath(k++));
			}
		}

		Debug.Log("Current live agents saved.");
	}

	private static void Preload(string file)
	{
		var brain = new BasicBrain(Prototype);
		brain.Load(file);
		agents.Add(new Agent(brain));
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
			bird.Termination += BirdTermination;
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

	public Agent(BasicBrain brain)
	{
		if (brain == null) throw new ArgumentNullException(nameof(brain));
		Brain = brain;
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

	private void BirdTermination(Bird bird)
	{
		if (bird != this.bird) return;
		Bird = null;

		var pipe = Pipes.Peek();
		var pipeCenter = (pipe.UpperY + pipe.LowerY) / 2f;
		var penalty = Mathf.Pow((bird.transform.position.y - pipeCenter) / 100f, 4f);
		
		Brain.EvolutionValue = Mathf.Max(Evolution.Progress() - penalty, 1e-10f);
		Agents.Best = Brain;
	}
}

public static class AgentManager
{
	public const string Storage = @"D:\Desktop\projects\flappy-bird-ai\storage\";
	public static readonly string Chosen = @"chosen\bird.sav";
	public static readonly string Current = DateTime.Now.ToString("dd.MM.yyyy. HH-mm-ss");

	public static string SavePath => Path.Combine(Current, $"bird-gen{((int)Agents.CurrentGeneration):0000}.sav");

	public static string UserPath(int k) => Path.Combine(Current, $"user-bird-{k}.sav");

	static AgentManager()
	{
		Chosen = Path.Combine(Storage, Chosen);
		Current = Path.Combine(Storage, Current);

		if (!Directory.Exists(Current)) Directory.CreateDirectory(Current);
	}

	public static void Save(this BasicBrain brain, string file)
	{
		var sb = new StringBuilder();

		sb.AppendLine(brain.EvolutionValue.ToString());

		using (var it = new NeuralIterator())
		{
			for (var param = it.Begin(brain.NeuralNetwork); param != null; param = it.Next())
			{
				sb.Append($"{it.CurrentParam}:");
				var data = param.GetData();
				for (var i = 0; i < data.Length; ++i)
				{
					sb.Append($" {data[i]}");
				}
				sb.AppendLine();
			}
		}

		File.WriteAllText(file, sb.ToString());
	}

	public static void Load(this BasicBrain brain, string file)
	{
		var lines = File.ReadAllLines(file);

		var parameters = new float[lines.Length - 1][];

		for (var i = 1; i < lines.Length; ++i)
		{
			var split = lines[i].Split(':');

			var p = Convert.ToInt32(split[0]);

			var vals = split[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			parameters[p] = new float[vals.Length];

			for (var j = 0; j < vals.Length; ++j)
			{
				parameters[p][j] = Convert.ToSingle(vals[j]);
			}
		}

		using (var it = new NeuralIterator())
		{
			for (var param = it.Begin(brain.NeuralNetwork); param != null; param = it.Next())
			{
				param.Transfer(parameters[it.CurrentParam]);
			}
		}
	}
}
