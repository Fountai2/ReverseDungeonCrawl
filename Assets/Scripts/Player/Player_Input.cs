using UnityEngine;
using System.Collections;

public class Player_Input : MonoBehaviour {

	private Transform thisTransform;
	private GameObject thisObject;
	private CharacterController thisController;
	
	
	void Start () {
		thisObject = gameObject;
		thisTransform = this.GetComponent<Transform>();	
		thisController = this.GetComponent<CharacterController>();
	}
	
	
	void Update () {
		if (Input.GetMouseButtonUp(0))
		{
			/*
			Plane xzPlane = new Plane(Vector3.up,thisTransform.position);
			Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			float rayHitDistance;
			if (xzPlane.Raycast(mouseRay, out rayHitDistance))
			{
				Vector3 target = mouseRay.GetPoint(rayHitDistance);	
				
				thisObject.SendMessage("SetTarget",target);
			}
			*/
			
			RaycastHit hit;
			Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(mouseRay, out hit))
			{
				Debug.Log("Hit: " + hit.collider.tag);
				if (hit.collider.tag != "Block")
				{
					Vector3 target = mouseRay.GetPoint(hit.distance);
					
					thisObject.SendMessage("SetTarget", target);
				}
			}
		}
	}
}
