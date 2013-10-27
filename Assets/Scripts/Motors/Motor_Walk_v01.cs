using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Motor_Walk_v01 : MonoBehaviour {
	
	private Transform thisTransform;
	private CharacterController thisController;
	
	private Vector3 momentum;
	private Vector3 moveDirection;
	private Vector3 thisDirection;
	private Vector3 targetDirection;
	private Vector3 startPoint;
	private Vector3 endPoint;
	private Vector3 moveVector;
	private Vector3 alternateTarget;
	private Vector3 currentTarget;
	private Vector3 lastDirection;
	
	private float currentTargetTimer = 0;
	private float currentTargetInterval = 0.1f;
	
	private float startTime;
	
	private Vector3 verticalOffset = Vector3.zero;
	
	private float checkAngleSmall = 45f;
	private float checkAngleLarge = 90f;
	private float checkDistance = 2f;
	private float backStepDistance = 1f;
	
	public float speed = 10f; // in Meters per Second
	private float easeDistance = 1.5f; // in Meters
	private float turnSpeed = 7f;
		
	private bool hasNewMoveTarget = false;
	
	private List<Vector3> pathNodeList = new List<Vector3>();

	public int pathLength = 3;
	
	public int rayCastTotal = 0;
	
	
	void Start()
	{
		thisTransform = this.GetComponent<Transform>();	
		thisController = this.GetComponent<CharacterController>();
		verticalOffset = new Vector3(0,thisController.radius,0);
		
		startPoint = thisTransform.position;
		endPoint = thisTransform.position;
		moveVector = thisTransform.forward;
		lastDirection = thisTransform.forward;
	}
	
	
	
	void Update()
	{
		if (Vector3.Distance(endPoint,thisTransform.position) > 0.3f)
		{
			if (hasNewMoveTarget)
			{
				startPoint = thisTransform.position;
				hasNewMoveTarget = false;	
			}
			
			
			UpdateMovement();
			UpdateRotation();
			
		}
	}

	
	
	private void UpdateMovement()
	{
		if (currentTargetTimer <= 0)
		{
			currentTarget = GetCurrentTarget();
			currentTargetTimer = currentTargetInterval;
		}
		else
		{
			currentTargetTimer -= Time.deltaTime;	
		}
		
		
		moveVector = (currentTarget - thisTransform.position).normalized;
		
		float adjustedSpeed = AdjustSpeed();
		
		thisController.Move(moveVector * adjustedSpeed * Time.deltaTime);
		
		thisTransform.position = new Vector3(thisTransform.position.x,startPoint.y,thisTransform.position.z);
	}
	
	
	
	private Vector3 GetCurrentTarget()
	{
		Vector3 newTarget = thisTransform.position;
		
		if (pathNodeList.Count != 0)
		{
			for (int i=pathNodeList.Count-1; i>=0; i--)
			{
				Vector3 testDirection = ((new Vector3(pathNodeList[i].x,0,pathNodeList[i].z))-(new Vector3(thisTransform.position.x,0,thisTransform.position.z))).normalized;
				float testDistance = Vector3.Distance(pathNodeList[i],thisTransform.position);
				
				rayCastTotal++;
				RaycastHit hit;
				if (!Physics.SphereCast(thisTransform.position + verticalOffset, thisController.radius, testDirection, out hit, testDistance))
				{
					newTarget = (pathNodeList[i]);
					break;
				}
			}
		}
		
		return newTarget;
	}
	
	
	private void CreatePath()
	{
		pathNodeList.Clear();
		
		CheckNode(thisTransform.position + verticalOffset);
		
	}
	
	
	private void CheckNode(Vector3 _node)
	{
		if (pathNodeList.Count > 10)
		{
			endPoint = pathNodeList[pathNodeList.Count-1];
			return;
		}
		
		Vector3 flatNode = new Vector3(_node.x,endPoint.y,_node.z);
		
		Vector3 targetDirection = (endPoint - flatNode).normalized;
		float targetDistance = Vector3.Distance(endPoint,flatNode);
		
		rayCastTotal++;
		RaycastHit hit;
		if (Physics.SphereCast(_node, thisController.radius, targetDirection, out hit,  targetDistance))
		{
			Vector3 newNode = hit.point - (targetDirection*backStepDistance);
			pathNodeList.Add(newNode);
			CheckAngles(newNode,targetDirection,checkAngleSmall);
		}
		else
		{
			pathNodeList.Add(endPoint);
		}
	}
	
	
	private void CheckAngles(Vector3 _point, Vector3 _direction, float _angle)
	{
		Vector3 angleOneDirection = Quaternion.Euler(0,_angle,0) * _direction;
		Vector3 angleTwoDirection = Quaternion.Euler(0,-_angle,0) * _direction;
		
		bool didAngleOneHit = false;
		bool didAngleTwoHit = false;
		
		
		rayCastTotal++;
		RaycastHit hitOne;
		if (Physics.SphereCast(_point, thisController.radius, angleOneDirection, out hitOne, checkDistance))
		{
			didAngleOneHit = true;
		}
		
		rayCastTotal++;
		RaycastHit hitTwo;
		if (Physics.SphereCast(_point, thisController.radius, angleTwoDirection, out hitTwo,  checkDistance))
		{
			didAngleTwoHit = true;
		}
		
		
		if (!didAngleOneHit)
		{
			Vector3 newNode = _point + (angleOneDirection * checkDistance);
			pathNodeList.Add(newNode);
			CheckNode(newNode);
		}
		else if (!didAngleTwoHit)
		{
			Vector3 newNode = _point + (angleTwoDirection * checkDistance);
			pathNodeList.Add(newNode);
			CheckNode(newNode);
		}
		else
		{
			if (_angle == checkAngleSmall)
			{
				CheckAngles(_point,_direction,checkAngleLarge);
			}
			else
			{
				endPoint = new Vector3(_point.x,startPoint.y,_point.z);
				pathNodeList.Add(endPoint);
			}
		}
	
		
	}
	
	
	
	void OnDrawGizmos()
	{
		foreach (var point in pathNodeList)
		{
			Gizmos.DrawWireSphere(point,0.25f);	
		}
	}
	
	
	
	private void SetTarget(Vector3 _target)
	{
		endPoint = _target;
		
		CreatePath();
		
		hasNewMoveTarget = true;
	}
	
	
	private float AdjustSpeed()
	{
		float adjustedEaseDistance = easeDistance;
		if (Vector3.Distance(startPoint,endPoint) < easeDistance)
		{
			adjustedEaseDistance = Vector3.Distance(startPoint,currentTarget)*0.5f;	
		}
		
		
		float adjustedSpeed = speed;
		if (Vector3.Distance(startPoint,thisTransform.position) < adjustedEaseDistance)
		{
			// speed up
			float currentEasePercent = Vector3.Distance(startPoint,thisTransform.position)/adjustedEaseDistance;
			adjustedSpeed = EaseOut((speed*0.5f),speed,0,currentEasePercent,1f);
		}
		else if (Vector3.Distance(endPoint,thisTransform.position) < adjustedEaseDistance)
		{
			// slow down
			float currentEasePercent = Vector3.Distance(thisTransform.position,endPoint)/adjustedEaseDistance;
			adjustedSpeed = EaseLinear((speed*0.1f),speed,0,currentEasePercent,1f);
		}
		
		return (adjustedSpeed);
	}
	
	
	private void UpdateRotation()
	{
		
		Vector3 flatMoveDirection = new Vector3(moveVector.x, 0, moveVector.z).normalized;
		if (flatMoveDirection.magnitude == 0)
		{
			flatMoveDirection = lastDirection;	
		}
		Vector3 flatTransformForward = new Vector3(thisTransform.forward.x, 0, thisTransform.forward.z).normalized;
		thisTransform.forward = Vector3.RotateTowards(flatTransformForward, flatMoveDirection, turnSpeed * Mathf.Deg2Rad, 1000);
	}
	
	
	
	
	
	
	public void SetSpeed(float _speed)
	{
		speed = _speed;	
	}
		
	
	
	//**************************************************
	// BEGIN EASING FUNCTIONS
	//**************************************************
	
	
	private float EaseIn(float _startValue, float _endValue, float _startTime, float _currentTime, float _duration)
	{
		float t = (_currentTime-_startTime)/_duration;
		
		float difference = _endValue-_startValue;
		
		float easedValue = difference * (t*t*t) + _startValue;
		
		return easedValue;
	}
	
	private float EaseOut(float _startValue, float _endValue, float _startTime, float _currentTime, float _duration)
	{
		float t = (_currentTime-_startTime)/_duration - 1f;
	
		float difference = _endValue-_startValue;
		
		float easedValue = difference * (t*t*t+1f) + _startValue;
		
		return easedValue;
	}
	
	private float EaseLinear(float _startValue, float _endValue, float _startTime, float _currentTime, float _duration)
	{
		float t = (_currentTime-_startTime)/_duration;
		
		float difference = _endValue-_startValue;
		
		float easedValue = difference * t + _startValue;
		
		return easedValue;
	}
	
	
	
	
	
	
	
	
}
