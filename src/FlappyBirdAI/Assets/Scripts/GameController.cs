using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

using GrandIntelligence;

public class GameController : MonoBehaviour
{
	private static Simulation simulation = null;
	private static List<Agent> agents = null;
	private static List<Bird> birds = null;
	private static uint generation = 0u;

	[SerializeField] private GameObject Bird = null;
	[SerializeField] private GameObject Pipe = null;
	[SerializeField] private Text Prompt = null;

	[SerializeField] private int BirdCount = 100;

	[SerializeField] private float PipeCenterTolerance = 60f;
	[SerializeField] private float PipeMiddleSpaceMin = 3f;
	[SerializeField] private float PipeMiddleSpaceMax = 4f;
	[SerializeField] private float PipeRespawnMinTimeout = 1.75f;
	[SerializeField] private float PipeRespawnMaxTimeout = 3f;

	private int birdsLeft;
	private DateTime pipeSpawnTime;
	private float pipeRespawnTimeout;
	private DateTime startTime;

	private void OnBirdTerminated()
	{
		--birdsLeft;
		UpdatePrompt();
		simulation.BirdTerminated(birdsLeft);
		if (birdsLeft > 0) return;
		SceneManager.LoadScene("Main");
	}

	private void Start()
	{
		if (simulation == null)
		{
			GICore.Init(new Spec(GrandIntelligence.DeviceType.Cpu));

			birds = new List<Bird>(BirdCount);
			agents = new List<Agent>(BirdCount);

			for (var i = 0; i < BirdCount; ++i)
			{
				birds.Add(null);
				agents.Add(new Agent());
			}

			simulation = new Simulation();
			simulation.Begin(agents);
		}

		birdsLeft = BirdCount;

		for (var i = 0; i < BirdCount; ++i)
		{
			var bird = Instantiate(Bird).GetComponent<Bird>();
			birds[i] = bird;

			var index = i;
			bird.Terminated += () =>
			{
				birds[index] = null;
				OnBirdTerminated();
			};

			agents[i].AssignBird(bird);
		}

		pipeRespawnTimeout = 0f;
		pipeSpawnTime = DateTime.Now;

		startTime = DateTime.Now;

		Pipes.Clear();

		++generation;
		simulation.Start();

		UpdatePrompt();
	}

	private void Update()
	{
		SpawnPipe();
	}

	private void FixedUpdate()
	{
		var pipe = Pipes.Peek();
		for (var i = 0; i < agents.Count; ++i)
			if (birds[i] != null)
				agents[i].Think(pipe);

		UpdatePrompt();
	}

	public void SpawnPipe()
	{
		if ((DateTime.Now - pipeSpawnTime).TotalSeconds < pipeRespawnTimeout) return;
		var pipe = Instantiate(Pipe).GetComponent<Pipe>();
		pipeSpawnTime = DateTime.Now;

		pipeRespawnTimeout = Random.Range(PipeRespawnMinTimeout, PipeRespawnMaxTimeout);
		
		var pipeHeight = pipe.Height;
		var pipeCenter = pipeHeight / 2f + Random.Range(-PipeCenterTolerance / 2f, +PipeCenterTolerance / 2f) * pipeHeight / 100f;
		var pipeSpace = Random.Range(PipeMiddleSpaceMin, PipeMiddleSpaceMax);

		pipe.UpperHeight = pipeHeight - (pipeCenter + pipeSpace / 2f);
		pipe.LowerHeight = pipeCenter - pipeSpace / 2f;
	}

	private void UpdatePrompt()
	{
		Prompt.text = $"Generation: {generation} | Agents alive: {birdsLeft} | Time: {(DateTime.Now - startTime).TotalSeconds}s";
	}

	private void OnApplicationQuit()
	{
		simulation.End();
		GICore.Release();
	}
}
