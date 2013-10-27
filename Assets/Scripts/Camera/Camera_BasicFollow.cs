using UnityEngine;
using System.Collections;

public class Camera_BasicFollow : MonoBehaviour {

	public Transform thisTarget;
	private Transform thisTransform;
	
	
	void Start () 
	{
		thisTransform = this.GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (thisTarget != null)
		{
			thisTransform.position = new Vector3(thisTarget.position.x, thisTransform.position.y, thisTarget.position.z-6.5f);	
		}
	}
}
