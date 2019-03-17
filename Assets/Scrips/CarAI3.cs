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
				path = coord.secondPath;
				if (path != null) {
					once = false;
				}
			}else if ((once || coord.thirdPath.Count != path.Count) && this.gameObject.name == "ArmedCar (4)") {
				path = coord.thirdPath;
				if (path != null) {
					once = false;
				}
			}

			if (searchingfortarget) {
				TestDrive ();

			}
		}


		/// <summary>
		/// BELOW IS STUFF USED FOR FOLLOWING THE PATH
		/// </summary>

		float steering;
		float accelleration;
		float reverse = 0f;
		float brake;
		float distanceToWalls;
		float sensorAngle = 50f;
		bool boogie = false;
		bool collisionDetected = false;
		float notRlySteering;

		public void TestDrive(){
			float angle = CalculateAngle (lockedNode);
			float angleToTarget = CalculateAngle (targetNode);

			//left and right sensor, the angle "50" and "-50" needs to be calculated, straight sensor is just to measure how close to a wall we are
			RaycastHit rightFrontSensor;
			RaycastHit leftFrontSensor;
			RaycastHit rightWallSensor;
			RaycastHit leftWallSensor;
			RaycastHit straightSensor;

			//20 could be calculated to form the best kind of steering, angle goes from -180 to +180, 

			//94/angle^1.24f = antal gridX ;)

			if (Mathf.Abs (angle) < 90f && Mathf.Abs(angle) > 10) {
				distanceToWalls = 10 * (250 / Mathf.Pow(Mathf.Abs(angle), 1.2f));
				sensorAngle = Mathf.Abs(angle)+5;
				if (sensorAngle > 40f) {
					sensorAngle = 40f;
				}
			} else if(Mathf.Abs(angle) < 10){
				distanceToWalls = distanceToTarget;
				sensorAngle = 5f;
			}else {
				distanceToWalls = 10;
				sensorAngle = 40f;

			}

			if (distanceToWalls < 10) {
				distanceToWalls = 10;
			} else if (distanceToWalls > distanceToNext) {
				distanceToWalls = distanceToNext;
			}
			Physics.Raycast(transform.position, Quaternion.AngleAxis (90f, transform.up) * transform.forward, out rightWallSensor);
			Physics.Raycast(transform.position, Quaternion.AngleAxis (-90f, transform.up) * transform.forward, out leftWallSensor);

			Physics.Raycast(transform.position+transform.right, Quaternion.AngleAxis (sensorAngle, transform.up) * transform.forward, out rightFrontSensor);
			Physics.Raycast(transform.position-transform.right, Quaternion.AngleAxis (-sensorAngle, transform.up) * transform.forward, out leftFrontSensor);

			Physics.Raycast(transform.position, transform.forward, out straightSensor);

			Debug.DrawRay (transform.position+transform.right, Quaternion.AngleAxis (sensorAngle, transform.up) * (transform.forward*distanceToWalls), Color.red);
			Debug.DrawRay (transform.position-transform.right, Quaternion.AngleAxis (-sensorAngle, transform.up) * (transform.forward*distanceToWalls), Color.red);

			Debug.DrawRay (transform.position, Quaternion.AngleAxis (90f, transform.up) * (transform.forward*5.2f), Color.yellow);
			Debug.DrawRay (transform.position, Quaternion.AngleAxis (-90f, transform.up) * (transform.forward*5.2f), Color.yellow);

			Debug.DrawRay (transform.position, transform.forward * distanceToTarget, Color.black);

			float steeringDependency = 30f;
			if (m_Car.CurrentSpeed < 35f) {
				steeringDependency = 5f;
			}

			//steering changed if walls are nearby or such, here alot of work can be done, mainly trying to figue out how it should work :D
			if (angleToTarget > angle && rightWallSensor.distance < leftWallSensor.distance && rightWallSensor.distance < 5.2f && (!(notRlySteering > -0.15f) || boogie)) {
				steering = -0.2f;
				notRlySteering = -0.2f;
			} else if (angleToTarget < angle && leftWallSensor.distance < rightWallSensor.distance && leftWallSensor.distance < 5.2f && (!(notRlySteering < 0.15f) || boogie)) {
				steering = 0.2f;
				notRlySteering = 0.2f;
			} else if (angle < 0 && rightFrontSensor.distance < distanceToWalls && rightFrontSensor.distance < leftFrontSensor.distance && distanceToTarget < 10*6f) {
				steering = -0.7f;
				notRlySteering = -0.7f;
			} else if (angle > 0 && leftFrontSensor.distance < distanceToWalls && rightFrontSensor.distance > leftFrontSensor.distance && distanceToTarget < 10*6f) {
				steering = 0.7f;
				notRlySteering = 0.7f;
			} else if (rightFrontSensor.distance < leftFrontSensor.distance && rightFrontSensor.distance > distanceToWalls && Mathf.Abs (angle) < 5) {
				steering = 0f;
				notRlySteering = 0f;
			} else if (leftFrontSensor.distance < rightFrontSensor.distance && leftFrontSensor.distance > distanceToWalls && Mathf.Abs (angle) < 5) {
				steering = 0f;
				notRlySteering = 0f;
			} else if (angle > 0 && rightFrontSensor.distance < distanceToWalls * 0.75f && steering > 0.1f && rightFrontSensor.distance < leftFrontSensor.distance) {
				steering = -0.1f;
				notRlySteering = -0.1f;
			} else if (angle < 0 && leftFrontSensor.distance < distanceToWalls * 0.75f && steering < -0.1f && rightFrontSensor.distance > leftFrontSensor.distance) {
				steering = 0.1f;
				notRlySteering = 0.1f;
			}else{
				notRlySteering = angle / 30f;
				steering = angle / steeringDependency;
			}


			//minimum distance is either the distance to the first node "we cant see" or the distance to a wall in front of us.
			float minDistance = distanceToTarget;
			if (straightSensor.distance < distanceToTarget) {
				minDistance = straightSensor.distance;
			}

			//calculated based on test data from TestBrakes, 
			accelleration = 1f;
			//here angles and other stuff could be measured but this works fine.
			/*if (m_Car.CurrentSpeed > 20f) {
				accelleration = 0f;
				reverse = 0f;
			} else {
				accelleration = 1f;
				reverse = 0f;
			}
			if (Mathf.Abs (angleToTarget) > 120 && stepLocked < 5) {
				collisionDetected = true;
			}*/
			//For collision
			/*
			if(m_Car.CurrentSpeed < 5f && ( straightSensor.distance < 4f || rightFrontSensor.distance < 4f || leftFrontSensor.distance < 4f ) && collisionDetected == false){
				collisionDetected = true;
			}
			if(collisionDetected){
				accelleration = 0f;
				reverse = -1f;
				steering = 0f;
				if(angle > 5f){
					steering = -1f;
				}
				if(angle < -5f){
					steering = 1f;
				}
				if(straightSensor.distance > 5f && rightFrontSensor.distance > 5f && leftFrontSensor.distance > 5f && Mathf.Abs(angleToTarget) < 120 ){
					collisionDetected = false;
				}
			}
			*/

			m_Car.Move (steering, 1f, 0f, 0f);
		}

		[SerializeField]
		int numberOfStepsThatMustBeSeen = 10;

		[SerializeField]
		bool showPath;

		float distanceToTarget = 100f;
		float distanceToNext = 100f;

		int stepLocked = 1;
		int stepTarget = 2;
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
