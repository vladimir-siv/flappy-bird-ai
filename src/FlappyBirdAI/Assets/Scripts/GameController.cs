using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
	[SerializeField] private GameObject Bird = null;
	[SerializeField] private GameObject Pipe = null;

	[SerializeField] private int BirdCount = 1;

	[SerializeField] private float PipeCenterTolerance = 60f;
	[SerializeField] private float PipeMiddleSpaceMin = 3f;
	[SerializeField] private float PipeMiddleSpaceMax = 4f;
	[SerializeField] private float PipeRespawnMinTimeout = 1.5f;
	[SerializeField] private float PipeRespawnMaxTimeout = 3f;

	private int birdsLeft;
	private DateTime pipeSpawnTime;
	private float pipeRespawnTimeout;

	private void OnBirdTerminated(Bird bird)
	{
		if (--birdsLeft > 0) return;
		SceneManager.LoadScene("Main");
	}

	private void Start()
	{
		birdsLeft = BirdCount;

		for (var i = 0; i < BirdCount; ++i)
		{
			Instantiate(Bird).GetComponent<Bird>().Terminated += OnBirdTerminated;
		}

		pipeRespawnTimeout = 0f;
		pipeSpawnTime = DateTime.Now;
	}

	private void Update()
	{
		SpawnPipe();
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
}
