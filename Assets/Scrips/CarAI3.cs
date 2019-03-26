using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Node = CoordinatorScript.Node;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI3 : MonoBehaviour
    {
		private CarController m_Car; // the car controller we want to use

		public GameObject terrain_manager_game_object;
		TerrainManager terrain_manager;
		public GameObject coordinator;
		CoordinatorScript coord;
		public List<Node> path;

		//targetNode = node to look at and see if we can travel to it. 
		Node targetNode;
		//lockedNode = node in sight and where to aim our car
		Node lockedNode;
		bool searchingfortarget = false;
		//Called once at start
		private void Awake()
		{

			// get the car controller
			m_Car = GetComponent<CarController>();
			terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager> ();
			coord = coordinator.GetComponent<CoordinatorScript>();	
			//coord.StartDriving ();	
			StartCoroutine ("UpdateTarget");
		}

		bool once = true;
		private void FixedUpdate()
		{
			if ((once || coord.firstPath.Count != path.Count) && this.gameObject.name == "ArmedCar (2)") {
				path = coord.firstPath;
				if (path != null) {
					once = false;
				}
			}else if ((once || coord.secondPath.Count != path.Count) && this.gameObject.name == "ArmedCar (3)") {
				path = coord.thirdPath;
				if (path != null) {
					once = false;
				}
			}else if ((once || coord.thirdPath.Count != path.Count) && this.gameObject.name == "ArmedCar (4)") {
				path = coord.secondPath;
				if (path != null) {
					once = false;
				}
			}

			if (searchingfortarget) {
				Drive ();
			}
		}


		/// <summary>
		/// BELOW IS STUFF USED FOR FOLLOWING THE PATH
		/// </summary>

		bool start = true;
		bool collisionDetected = false;
		public void Drive(){
			Vector3 referencePosition = lockedNode.position;
			float angleToRefPos = CalculateAngle (lockedNode);
			float steering = angleToRefPos / 45f;
			float accelleration = 1f;
			float brake = 0f;

			if (start && Physics.Raycast (transform.position, transform.forward, 20)) {
				accelleration = 0f;
				brake = -1f;
				steering = 0f;
			} else {
				start = false;
			}
			if(m_Car.CurrentSpeed < 5f && ((Physics.Raycast(transform.position+transform.right, transform.forward, 10) || Physics.Raycast(transform.position-transform.right, transform.forward, 10)) && collisionDetected == false)){
				collisionDetected = true;
			}
			if(collisionDetected){
				accelleration = 0f;
				brake = -1f;
				steering = -steering;
				if (steering < 0) {
					steering = -1;
				}
				if (steering > 0) {
					steering = 1;
				}

				if(!Physics.Raycast(transform.position, transform.forward, 10)){
					collisionDetected = false;
				}
			}
			m_Car.Move (steering, accelleration, brake, 0f);
		}

		[SerializeField]
		int numberOfStepsThatMustBeSeen = 10;

		[SerializeField]
		bool showPath;

		float distanceToTarget = 100f;
		float distanceToNext = 100f;

		int stepLocked = 0;
		int stepTarget = 1;
		//tries to update target every 0.1 sec
		IEnumerator UpdateTarget(){
			while (path == null) {
				yield return null;
			}

			lockedNode = path [stepLocked];
			targetNode = path [stepTarget];

			searchingfortarget = true;

			while (stepTarget < path.Count) {

				distanceToTarget = Mathf.Sqrt ((transform.position.x - targetNode.position.x) * (transform.position.x - targetNode.position.x) + (transform.position.z - targetNode.position.z) * (transform.position.z - targetNode.position.z));
				distanceToNext = Mathf.Sqrt((transform.position.x - path[stepLocked].position.x) * (transform.position.x - path[stepLocked].position.x) + (transform.position.z - path[stepLocked].position.z) * (transform.position.z - path[stepLocked].position.z));

				Vector3 targetDirection = targetNode.position - transform.position;

				//sensor from car position to possible target
				//is true if the sensor is interrupted by anything inbetween
				bool sensor = Physics.Raycast (transform.position, targetDirection, distanceToTarget);

				//when sensor is false we say we "see" the target and tries to "see" next target in path.
				if(distanceToTarget < 40f && !sensor){
					stepTarget++;
					if (stepTarget == path.Count) {
						lockedNode = targetNode;
						break;
					}
					targetNode = path [stepTarget];
				} 
				//when either we "see" 4 blocks ahead or distance to the next locked target is lower then 13 we update lockedNode
				if ((distanceToNext < 18f && stepTarget - stepLocked > 0) || (stepTarget - stepLocked > numberOfStepsThatMustBeSeen && distanceToNext < 40f)) {
					lockedNode = path [stepLocked];
					stepLocked++;
				}
				yield return null;
			}
		}

		//Calculates the angle between target and vehicle direction.
		float CalculateAngle(Node Target){
			Vector3 targetDirection = Target.position - transform.position;
			float angle = Vector3.Angle (targetDirection, transform.forward);
			Vector3 test = Vector3.Cross (targetDirection, transform.forward);
			if(test.y > 0){
				angle = -angle;
			}
			return angle;
		}
    }
}
