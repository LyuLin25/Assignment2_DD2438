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
		GameObject virtualLeader;
		GameObject box;
		List<VirtualLeader> virtualFrames;

		int frameCount = 15;

		float distanceOffset;
		float defaultDistanceOffset = 22;
		float angle = 90;
		private void Start()
		{
			// get the car controller
			m_Car = GetComponent<CarController>();
			terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
			virtualFrames = new List<VirtualLeader> ();
			virtualLeader = Instantiate (virtualLeaderObject, leader.transform.position, Quaternion.identity); 
			if (side == "Left") {
				angle = -angle;
			}
			// note that both arrays will have holes when objects are destroyed
			// but for initial planning they should work
			friends = GameObject.FindGameObjectsWithTag("Player");
			enemies = GameObject.FindGameObjectsWithTag("Enemy");
			distanceOffset = defaultDistanceOffset;
			referencePosition = virtualLeader.transform.position;
			//box = Instantiate (debugBox, referencePosition, Quaternion.identity);
			// Plan your path here
			// ...
		}
		Vector3 referencePosition;	
		private void Update()
		{
			virtualFrames.Add (new VirtualLeader (leader.transform.position, leader.transform.forward));
			if (virtualFrames.Count > frameCount) {
				virtualLeader.transform.position = virtualFrames [0].position;

				Vector3 obstacleOffset = -virtualFrames [frameCount].direction;
				obstacleOffset *= defaultDistanceOffset+2;
				obstacleOffset = Quaternion.AngleAxis (angle, transform.up) * obstacleOffset;
				Vector3 obstaclePosition = obstacleOffset + virtualFrames [frameCount].position; 

				Vector3 carOffset = -transform.forward;
				carOffset *= 10;
				carOffset = Quaternion.AngleAxis (angle, transform.up) * carOffset;
				Vector3 carPosOffset = transform.position + carOffset;

				Vector3 offset = -virtualFrames [0].direction;
				Vector3 test = offset;
				offset *= distanceOffset;
				offset = Quaternion.AngleAxis (angle, transform.up) * offset;
				referencePosition = offset + virtualLeader.transform.position;
				virtualFrames.RemoveAt (0);

				test *= defaultDistanceOffset+2;
				test = Quaternion.AngleAxis (angle, transform.up) * test;
				Vector3 testPosition = test + virtualLeader.transform.position;

				if (FreeOfObstacles (obstaclePosition) && FreeOfObstacles (carPosOffset) && FreeOfObstacles(testPosition)) {
					distanceOffset = defaultDistanceOffset;
				} else {
					distanceOffset = 5f;
				}
				//box.transform.position = referencePosition + new Vector3(0,5,0);
			}
		}
		private void LateUpdate(){
			Drive ();
		}

		void Drive(){
			float angleToRefPos = CalculateAngle (referencePosition);
			float angleToLeader = CalculateAngle (leader.transform.position);
			float distanceToRefPos = Vector3.Distance (transform.position, referencePosition);
			float distanceToLeader = Vector3.Distance (transform.position, leader.transform.position);
			Vector3 direction = (referencePosition - transform.position).normalized;
			float steering = angleToRefPos / 45;
			float accelleration = 1f;
			float brake = 0f;
			bool refPosIsInFront = Vector3.Dot(direction, transform.forward) > 0f;

			float possibleDistance = 0.0087f * (m_Car.CurrentSpeed * m_Car.CurrentSpeed) + 0.18f * m_Car.CurrentSpeed - 6.45f;

			if (refPosIsInFront && possibleDistance < distanceToRefPos) {
				accelleration = 1f;
				brake = 0f;
			} else {
				accelleration = 0f;
				brake = -1f;
				if (!refPosIsInFront && possibleDistance < distanceToLeader) {
					steering = angleToLeader / 25;
					brake = 0f;
					accelleration = 1f;
				} else {
					steering = -steering;
				}
			}

			if (distanceToLeader < 10) {
				if (m_Car.CurrentSpeed < 2) {
					steering = -angleToLeader / 45;
				} else {
					steering = angleToLeader / 45;
				}
				brake = -1f;
				accelleration = 0f;
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
