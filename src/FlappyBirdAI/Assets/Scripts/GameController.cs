using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
	[SerializeField] private GameObject Bird = null;
	[SerializeField] private GameObject Pipe = null;
	[SerializeField] private Text GenerationText = null;

	[SerializeField] private int BirdCount = 100;

	[SerializeField] private float PipeCenterTolerance = 60f;
	[SerializeField] private float PipeMiddleSpaceMin = 3f;
	[SerializeField] private float PipeMiddleSpaceMax = 4f;
	[SerializeField] private float PipeRespawnMinTimeout = 1.75f;
	[SerializeField] private float PipeRespawnMaxTimeout = 3f;

	private int birdsLeft;
	private DateTime pipeSpawnTime;
	private float pipeRespawnTimeout;
	private bool saveRequest;

	private void OnBirdTerminated()
	{
		if (--birdsLeft > 0) return;
		Agents.Cycle();
		SceneManager.LoadScene("Main");
	}

	private void Start()
	{
		GILibrary.Init();
		Agents.Create(BirdCount);

		birdsLeft = BirdCount;

		for (var i = 0; i < BirdCount; ++i)
		{
			var bird = Instantiate(Bird).GetComponent<Bird>();
			bird.Terminated += OnBirdTerminated;
			Agents.AssignBird(bird, i);
		}

		pipeRespawnTimeout = 0f;
		pipeSpawnTime = DateTime.Now;

		Pipes.Clear();
		Evolution.Begin();

		GenerationText.text = $"Generation: {Agents.CurrentGeneration}";
	}

	private void Update()
	{
		if (saveRequest ^ Input.GetKeyDown(KeyCode.S))
		{
			saveRequest = !saveRequest;
			if (saveRequest) Agents.SaveExisting();
		}

		SpawnPipe();
		Agents.Think();
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

	private void OnApplicationQuit()
	{
		GILibrary.Release();
	}
}
