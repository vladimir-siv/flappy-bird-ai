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

	private void Awake()
	{
		upperPipe = transform.GetChild(0);
		lowerPipe = transform.GetChild(1);
	}

	private void Start()
	{
		var body = GetComponent<Rigidbody>();
		body.velocity = new Vector3(-Velocity, 0f, 0f);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "PipeBorder")
		{
			Destroy(gameObject);
		}
	}
}
