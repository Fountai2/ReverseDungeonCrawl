using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Motor_Walk : MonoBehaviour {
	
	private enum PathfindingState
	{
		scatterCast,
		targetCast,
		evaluate,
		end
	}
	
	private PathfindingState pathState = PathfindingState.end;
	
	private Transform thisTransform;
	private CharacterController thisController;
	
	private float thisWidth;
	
	private Vector3 momentum;
	private Vector3 moveDirection;
	private Vector3 thisDirection;
	private Vector3 targetDirection;
	private Vector3 startPoint;
	private Vector3 endPoint;
	private Vector3 moveVector;
	private Vector3 lastVector;
	private Vector3 alternateTarget;
	private Vector3 currentTarget;
	private Vector3 lastTarget;
	
	private float currentTargetTimer = 0;
	private float currentTargetInterval = 0.1f;
	
	private float startTime;
	
	private Vector3 thisOffset = Vector3.zero;
	
	private float checkAngleSmall = 45f;
	private float checkAngleLarge = 90f;
	private float castCheckDistance = 2f;
	private float backStepDistance = 1f;
	
	public float speed = 10f; // in Meters per Second
	private float easeDistance = 1.5f; // in Meters
	private float turnSpeed = 9f;
	
	private List<Vector3> pathList = new List<Vector3>();
	
	private List<Vector3> scatterCastList = new List<Vector3>();
	private List<Vector3> targetCastList = new List<Vector3>();

	public int pathLength = 3;
	
	public int rayCastTotal = 0;
	
	
	
	void Start()
	{
		thisTransform = this.GetComponent<Transform>();	
		thisController = this.GetComponent<CharacterController>();
		thisWidth = thisController.radius*2f;
		thisOffset = new Vector3(0,(thisController.radius*1.5f),0);
		
		castCheckDistance = thisWidth * 4f;
		
		startPoint = thisTransform.position;
		endPoint = thisTransform.position;
		lastTarget = thisTransform.position;
		moveVector = thisTransform.forward;
		lastVector = thisTransform.forward;
		
	}
	
	
	
	void Update()
	{
		if (Vector3.Distance(endPoint,thisTransform.position) > 0.25f)
		{			
			if (pathState == PathfindingState.scatterCast)
			{
				ScatterCast(pathList[pathList.Count-1]);
			}
			else if (pathState == PathfindingState.targetCast)
			{
				TargetCast();	
			}
			else if (pathState == PathfindingState.evaluate)
			{
				Evaluate();	
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
	
	
	
	private void StartPath()
	{
		pathList.Clear();
		
		Vector3 targetDirection = ((endPoint + thisOffset) - (thisTransform.position + thisOffset)).normalized;
		float targetDistance = Vector3.Distance(endPoint,thisTransform.position);
		
		
		
		rayCastTotal++;
		RaycastHit hit;
		if (Physics.SphereCast(thisTransform.position + thisOffset, thisController.radius, targetDirection, out hit, targetDistance))
		{
			Vector3 testPoint = hit.point + (-targetDirection*thisWidth);
			testPoint = AdjustNode(testPoint);
			pathList.Add(testPoint); // ADDED NODE!!!
			pathState = PathfindingState.scatterCast;
			Debug.DrawLine(thisTransform.position + thisOffset, hit.point, Color.green, 4f);
		}
		else
		{
			Debug.DrawRay(thisTransform.position + thisOffset, (targetDirection * targetDistance), Color.green, 4f);
			pathList.Add(endPoint); // ADDED NODE!!!
			pathState = PathfindingState.end;
		}
	}
	
	private void ExtendPath()
	{
		
	}
	
	private void ScatterCast(Vector3 _node)
	{	
		
		scatterCastList.Clear();
		
		float angle = 45f;
		
		for (int i=0; i<8; i++)
		{
			Vector3 castDirection = Quaternion.Euler(0,angle*i,0) * Vector3.forward;	
			
			rayCastTotal++;
			RaycastHit hit;
			if (Physics.Raycast(_node, castDirection, out hit, castCheckDistance))
			{
				Debug.DrawLine(_node,hit.point, Color.yellow, 4f);
				Vector3 backedUpHitPoint = hit.point + (-castDirection*thisWidth);
				Debug.DrawRay(backedUpHitPoint, (Vector3.up * 10f), Color.red, 5f);
				if (!WeedOutPoint(backedUpHitPoint))
				{
					scatterCastList.Add(backedUpHitPoint);
				}
			}
			else
			{
				Debug.DrawRay(_node, (castDirection * castCheckDistance), Color.yellow, 4f);
				Vector3 rayEndPoint = _node + (castDirection * castCheckDistance);
				if (!WeedOutPoint(rayEndPoint))
				{
					scatterCastList.Add(rayEndPoint);
				}
			}
		}
		
		pathState = PathfindingState.targetCast;
	}
	
	
	private bool WeedOutPoint(Vector3 _point)
	{
		bool shouldBeWeeded = false;
		
		foreach (var node in pathList)
		{
			if (Vector3.Distance(_point,node) < thisWidth)
			{
				shouldBeWeeded = true;	
			}
		}
		
		return shouldBeWeeded;
	}
	
	
	private void TargetCast()
	{
		targetCastList.Clear();
		
		for (int i=0; i<scatterCastList.Count; i++)
		{
			Vector3 targetDirection = (endPoint - scatterCastList[i]).normalized;
			float targetDistance = Vector3.Distance(endPoint,scatterCastList[i]);
			
			rayCastTotal++;
			RaycastHit hit;
			if (Physics.Raycast(scatterCastList[i], targetDirection, out hit, targetDistance))
			{
				Debug.DrawLine(scatterCastList[i],hit.point,Color.blue,4f);
				Vector3 backedUpHitPoint = hit.point + (-targetDirection*thisWidth);
				if (!WeedOutPoint(backedUpHitPoint))
				{
					targetCastList.Add(backedUpHitPoint);
				}
			}
			else
			{
				Vector3 rayEndPoint = scatterCastList[i] + (targetDirection * targetDistance);
				Debug.DrawRay(scatterCastList[i], (targetDirection * targetDistance), Color.blue, 4f);
				if (!WeedOutPoint(rayEndPoint))
				{
					targetCastList.Add(rayEndPoint);
				}
			}
			
		}
		
		if (targetCastList.Count > 0)
		{
			pathState = PathfindingState.evaluate;
		}
		else
		{
			endPoint = pathList[pathList.Count-1];
			pathState = PathfindingState.end;
		}
		
	}
	
	private void Evaluate()
	{
		Vector3 nearestPoint = targetCastList[0];
		int nearestInt = 0;
		
		for (int i=0; i<targetCastList.Count; i++)
		{
			float nearestDistance = (nearestPoint-endPoint).sqrMagnitude;
			float testDistance = (targetCastList[i]-endPoint).sqrMagnitude;
			if (nearestDistance.CompareTo(testDistance) > 0)
			{
				nearestPoint = targetCastList[i];	
				nearestInt = i;
			}
		}
		
		pathList.Add(scatterCastList[nearestInt]); // Node Added!!!
		
		nearestPoint = AdjustNode(nearestPoint);
		pathList.Add(nearestPoint); // Node Added!!!
		
		
		pathState = PathfindingState.scatterCast;
		
	}
	
	
	
	private Vector3 AdjustNode(Vector3 _node)
	{
		Vector3 adjustedNode = _node;
		
		Debug.DrawRay(_node + thisOffset, (-Vector3.up * castCheckDistance), Color.magenta, 4f);
		
		rayCastTotal++;
		RaycastHit hit;
		if (Physics.Raycast(_node + thisOffset, -Vector3.up, out hit, castCheckDistance))
		{
			adjustedNode = hit.point + thisOffset;
		}
		
		return adjustedNode;
	}
	
	

	private Vector3 GetCurrentTarget()
	{
		Vector3 newTarget = thisTransform.position;
		
		if (pathList.Count != 0)
		{
			for (int i=pathList.Count-1; i>=0; i--)
			{
				Vector3 testDirection = ((new Vector3(pathList[i].x,0,pathList[i].z))-(new Vector3(thisTransform.position.x,0,thisTransform.position.z))).normalized;
				float testDistance = Vector3.Distance(pathList[i],thisTransform.position);
				
				rayCastTotal++;
				RaycastHit hit;
				if (!Physics.Raycast(thisTransform.position + thisOffset, testDirection, out hit, testDistance))
				{
					newTarget = (pathList[i]);
					break;
				}
			}
		}
		
		if (newTarget == thisTransform.position)
		{
			endPoint = thisTransform.position;
			pathState = PathfindingState.end;
		}
		
		return newTarget;
	}
	
	
	
	
	
	void OnDrawGizmos()
	{
		foreach (var point in pathList)
		{
			Gizmos.DrawWireSphere(point,0.25f);	
		}
		
	}
	
	
	
	private void SetTarget(Vector3 _target)
	{
		endPoint = _target;
		startPoint = thisTransform.position;
		StartPath();
		
	}
	
	
	
	private void UpdateRotation()
	{
		
		Vector3 flatMoveDirection = new Vector3(moveVector.x, 0, moveVector.z).normalized;
		if (flatMoveDirection.magnitude == 0)
		{
			flatMoveDirection = thisTransform.forward;	
		}
		Vector3 flatTransformForward = new Vector3(thisTransform.forward.x, 0, thisTransform.forward.z).normalized;
		thisTransform.forward = Vector3.RotateTowards(flatTransformForward, flatMoveDirection, turnSpeed * Mathf.Deg2Rad, 1000);
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
