using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour {

	
	private Vector3 currentTarget;
	private Transform targetTransform;
	private Transform thisTransform;
	
	
	void Start () 
	{
		thisTransform = this.transform;
		targetTransform = GameObject.FindGameObjectWithTag("Player").transform;
		StartCoroutine("WaitToChooseState",1f);
	}
	
	
	
	IEnumerator WaitToChooseState(float _time)
	{
		yield return new WaitForSeconds(_time);
		if (Vector3.Distance(thisTransform.position,targetTransform.position) < 9f)
		{
			ChaseTarget();
		}
		else
		{
			Wander();
		}
	}
	
	
	private void ChaseTarget()
	{
		this.SendMessage("SetSpeed", 8f);
		this.SendMessage("SetTarget", targetTransform.position);
		StartCoroutine("WaitToChooseState",0.3f);
	}
	
	private void Wander()
	{
		this.SendMessage("SetSpeed", 4f);
		Vector3 target = ChooseTarget();
		this.SendMessage("SetTarget", target);
		StartCoroutine("WaitToChooseState",1f);
	}
	
	
	private Vector3 ChooseTarget()
	{
		float randomChance = Random.value;
		
		if (randomChance > 0.6f)
		{
			float angle = Random.Range(0.1f,180f);
			
			float randomSide = Random.value;
			if (randomSide < 0.5)
				angle = angle*-1;
			
			Vector3 direction = Quaternion.Euler(0,angle,0) * Vector3.forward;
			
			float distance = Random.Range(2f,10f);
			
			Vector3 newTarget = thisTransform.position + (direction*distance);
			currentTarget = newTarget;
			
			return newTarget;
		}
		else
		{
			return currentTarget;
		}
		
		
	}
	
}
