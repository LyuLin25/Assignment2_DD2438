using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI4 : MonoBehaviour
    {
		private CarController m_Car; // the car controller we want to use

		public GameObject terrain_manager_game_object;
		TerrainManager terrain_manager;

		public GameObject[] friends;
		public GameObject[] enemies;

		public GameObject leader;
		public GameObject virtualLeaderObject;
		public string side;
		public GameObject debugBox;

		Vector3 referenceForward;

		Vector3 forwardDirection;

		GameObject virtualLeader;
		GameObject box;
		List<VirtualLeader> virtualFrames;

		int frameCount = 3;

		float distanceOffset;
		float defaultDistanceOffset = 20f;
		float angle = 90;
		float forwardAngle = 70;
		private void Start()
		{
			// get the car controller
			m_Car = GetComponent<CarController>();
			terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
			virtualFrames = new List<VirtualLeader> ();
			virtualLeader = Instantiate (virtualLeaderObject, leader.transform.position, Quaternion.identity); 
			if (side == "Left") {
				angle = -angle;
				forwardAngle = -forwardAngle;
			}
			// note that both arrays will have holes when objects are destroyed
			// but for initial planning they should work
			friends = GameObject.FindGameObjectsWithTag("Player");
			enemies = GameObject.FindGameObjectsWithTag("Enemy");
			distanceOffset = defaultDistanceOffset;
			referencePosition = virtualLeader.transform.position;
			forwardDirection = transform.forward;
			box = Instantiate (debugBox, referencePosition, Quaternion.identity);
			// Plan your path here
			// ...
		}
		Vector3 referencePosition;	
		private void Update()
		{


			if (leader.name != "ReplayCar (2)") {
				virtualFrames.Add (new VirtualLeader (leader.transform.position, leader.GetComponent<CarAI4> ().forwardDirection));
			} else {
				virtualFrames.Add (new VirtualLeader (leader.transform.position, leader.transform.forward));
			}

			if (virtualFrames.Count > frameCount) {
				float testAngle = Vector3.Angle (transform.forward, leader.transform.forward);
				Vector3 testCross = Vector3.Cross (leader.transform.forward, transform.forward);
				if(testCross.y > 0){
					testAngle = -testAngle;
				}
				float angleToLeader = CalculateAngleFromVirtual (virtualFrames[0], leader.transform.position);

				virtualLeader.transform.position = virtualFrames [0].position;
				forwardDirection = virtualFrames [0].direction;

				Vector3 obstacleOffset = virtualFrames [frameCount].direction;
				obstacleOffset *= defaultDistanceOffset+8;
				obstacleOffset = Quaternion.AngleAxis (-forwardAngle, transform.up) * obstacleOffset;
				Vector3 obstaclePosition = obstacleOffset + virtualFrames [frameCount].position; 

				obstacleOffset = -virtualFrames [frameCount].direction;
				obstacleOffset *= defaultDistanceOffset;
				obstacleOffset = Quaternion.AngleAxis (angle, transform.up) * obstacleOffset;
				Vector3 testPos = obstacleOffset + virtualFrames [frameCount].position;

				Vector3 offset = -virtualFrames [0].direction;
				Vector3 test = -virtualFrames[0].direction;
				offset *= distanceOffset;
				offset = Quaternion.AngleAxis (angle, transform.up) * offset;
				referencePosition = offset + virtualLeader.transform.position;
				virtualFrames.RemoveAt (0);

			
				test *= defaultDistanceOffset+5;
				test = Quaternion.AngleAxis (angle, transform.up) * test;
				Vector3 testPosition = test + virtualLeader.transform.position;

				if (testAngle > 20 && side == "Left") {
					distanceOffset = 3f;
				} else if (testAngle < -20 && side == "Right") {
					distanceOffset = 3f;
				} else if (FreeOfObstacles (obstaclePosition) && FreeOfObstacles(testPosition) && FreeOfObstacles(testPos)) {
					distanceOffset = defaultDistanceOffset;
				} else {
					distanceOffset = 0f;
				}
				box.transform.position = referencePosition + new Vector3(0,5,0);
			}
		}
		private void LateUpdate(){
			Drive ();
		}
		bool collisionDetected = false;
		void Drive(){
			float angleToRefPos = CalculateAngle (referencePosition);
			float angleToLeader = CalculateAngle (leader.transform.position);
			float distanceToRefPos = Vector3.Distance (transform.position, referencePosition);
			float distanceToLeader = Vector3.Distance (transform.position, leader.transform.position);
			Vector3 direction = (referencePosition - transform.position).normalized;
			float steeringDependency = 60f;
			if (distanceToRefPos > 25) {
				steeringDependency = 120f;
			}

			float steering = angleToRefPos / steeringDependency;
			float accelleration = 1f;
			float brake = 0f;
			bool refPosIsInFront = Vector3.Dot(direction, transform.forward) > 0f;

			float possibleDistance = 0.0087f * (m_Car.CurrentSpeed * m_Car.CurrentSpeed) + 0.18f * m_Car.CurrentSpeed - 8.45f;

			if (refPosIsInFront && possibleDistance < distanceToRefPos) {
				accelleration = 1f;
				brake = 0f;
			} else if(m_Car.CurrentSpeed > 5){
				accelleration = 0f;
				brake = -1f;
			}

			if (distanceToLeader < distanceToRefPos && !refPosIsInFront) {
				brake = 0f;
				accelleration = 0f;
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

		bool FreeOfObstacles(Vector3 position){
			if (terrain_manager.myInfo.traversability [terrain_manager.myInfo.get_i_index (position.x), terrain_manager.myInfo.get_j_index (position.z)] == 1f) {
				return false;
			} else {
				return true;
			}
		}

		float CalculateAngleFromVirtual(VirtualLeader virtualCar, Vector3 target){
			Vector3 targetDirection = target - virtualCar.position;
			float angle = Vector3.Angle (targetDirection, virtualCar.direction);
			Vector3 test = Vector3.Cross (targetDirection, virtualCar.direction);
			if(test.y > 0){
				angle = -angle;
			}
			return angle;
		}

		float CalculateAngle(Vector3 target){
			Vector3 targetDirection = target - transform.position;
			float angle = Vector3.Angle (targetDirection, transform.forward);
			Vector3 test = Vector3.Cross (targetDirection, transform.forward);
			if(test.y > 0){
				angle = -angle;
			}
			return angle;
		}
	}
	public class VirtualLeader{
		public Vector3 position;
		public Vector3 direction;

		public VirtualLeader(Vector3 pos, Vector3 dir){
			this.position = pos;
			this.direction = dir;
		}
	}
}
