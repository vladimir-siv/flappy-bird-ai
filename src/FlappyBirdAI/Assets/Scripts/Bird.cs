using System;
using UnityEngine;

public class Bird : MonoBehaviour
{
	[SerializeField] private float JumpForce = 10f;
	[SerializeField] private double JumpDelta = 250.0;

	private Rigidbody body;

	private DateTime jumpTime;
	private bool jumped = false;

	private void Start()
	{
		body = GetComponent<Rigidbody>();
	}

	public void Update()
	{
		if (jumped)
		{
			var delta = (DateTime.Now - jumpTime).TotalMilliseconds;
			jumped = delta < JumpDelta;
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			Jump();
		}
	}

	public void Jump()
	{
		if (jumped) return;
		body.velocity = new Vector3(0f, JumpForce, 0f);
		jumped = true;
		jumpTime = DateTime.Now;
	}

	public void Terminate()
	{
		Debug.Log("You have been Terminated.");
		Destroy(gameObject);
	}
}
