using System.Collections.Generic;
using UnityEngine;

public class Pipe : MonoBehaviour
{
	[SerializeField] private float Velocity = 2.5f;

	private Transform upperPipe;
	private Transform lowerPipe;

	public float Height => GetComponent<BoxCollider>().size.y;

	public float UpperHeight
	{
		get
		{
			return upperPipe.localScale.y;
		}
		set
		{
			var diff = value - upperPipe.localScale.y;
			upperPipe.MoveYBy(-diff / 2f);
			upperPipe.RescaleY(value);
		}
	}
	public float LowerHeight
	{
		get
		{
			return lowerPipe.localScale.y;
		}
		set
		{
			var diff = value - lowerPipe.localScale.y;
			lowerPipe.MoveYBy(+diff / 2f);
			lowerPipe.RescaleY(value);
		}
	}

	public float UpperY => upperPipe.position.y - upperPipe.localScale.y / 2f;
	public float LowerY => lowerPipe.position.y + lowerPipe.localScale.y / 2f;

	private void Awake()
	{
		upperPipe = transform.GetChild(0);
		lowerPipe = transform.GetChild(1);
	}

	private void Start()
	{
		var body = GetComponent<Rigidbody>();
		body.velocity = new Vector3(-Velocity, 0f, 0f);
		Pipes.Enqueue(this);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "PipeBorder")
		{
			Destroy(gameObject);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Bird")
		{
			Pipes.Dequeue(this);
		}
	}
}

public static class Pipes
{
	private static readonly Queue<Pipe> Collection = new Queue<Pipe>();

	public static void Clear() => Collection.Clear();

	public static Pipe Peek()
	{
		if (Collection.Count == 0) return null;
		return Collection.Peek();
	}

	public static void Enqueue(Pipe pipe)
	{
		Collection.Enqueue(pipe);
	}
	
	public static void Dequeue(Pipe pipe)
	{
		if (Peek() == pipe) Collection.Dequeue();
	}
}
