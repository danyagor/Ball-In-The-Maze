using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
	[SerializeField] private Vector3 offset;
	[SerializeField] private float moveSmooth;
	
	public Transform Target { get; set; }
	public Vector3 MapCameraPosition { get; set; }
	public bool CameraMapActive { get; set; }

	private void FixedUpdate()
	{
		if (!CameraMapActive)
		{
			if (Target)
			{
				// Smooth look at target
				Quaternion q = transform.rotation;
				transform.LookAt(Target);
				transform.rotation = Quaternion.Lerp(q, transform.rotation, moveSmooth);
				transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0, 0);

				// Smooth follow the object
				transform.position = Vector3.Lerp(transform.position,
					new Vector3(Target.position.x, 
						Target.position.y, 
						Target.position.z) + offset, moveSmooth);
			}
		}
		else
		{
			// Transition to the camera map position
			transform.position = Vector3.Lerp(transform.position,
					new Vector3(
						MapCameraPosition.x, 
						MapCameraPosition.y, 
						MapCameraPosition.z), 
					moveSmooth);
			transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(90, 0, 0), 0.3f);
		}
	}
}
