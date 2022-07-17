using System;
using UnityEngine;

public class Bird : MonoBehaviour
{
	[SerializeField] private float JumpForce = 5f;
	[SerializeField] private double JumpDelta = 250.0;

	private Rigidbody _body = null;
	public Rigidbody body
	{
		get
		{
			if (_body == null) _body = GetComponent<Rigidbody>();
			return _body;
		}
	}

	private DateTime jumpTime;
	private DateTime spawnTime;

	public TimeSpan TimeSinceSpawned => DateTime.Now - spawnTime;
	public event Action Terminated;

	private void Start()
	{
		jumpTime = DateTime.Now.AddMilliseconds(-JumpDelta);
		spawnTime = DateTime.Now;
	}

	public void Jump()
	{
		if ((DateTime.Now - jumpTime).TotalMilliseconds < JumpDelta) return;
		body.velocity = new Vector3(0f, JumpForce, 0f);
		jumpTime = DateTime.Now;
	}

	public void Terminate()
	{
		Terminated?.Invoke();
		Destroy(gameObject);
	}
}
