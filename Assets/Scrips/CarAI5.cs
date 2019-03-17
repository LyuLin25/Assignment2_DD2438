using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Node = CoordinatorScript.Node;


namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI5 : MonoBehaviour
    {
		private CarController m_Car; // the car controller we want to use

		public GameObject terrain_manager_game_object;
		TerrainManager terrain_manager;

		public GameObject[] friends;
		public GameObject[] enemies;

		public GameObject leader;
		public GameObject virtualLeaderObject;
		public string side;
		public bool isLeader;
		public GameObject debugBox;
		public Transform debugHolder;
		Vector3 referenceForward;
		public Vector3 forwardDirection;

		GameObject virtualLeader;
		GameObject box;
		List<VirtualLeader> virtualFrames;


		//how far behind leader should we follow?
		int frameCount = 3;

		//how far to the sides of the leader should we follow?
		float distanceOffset;
		float defaultDistanceOffset = 4f;

		//this is useless.
		float angle = 90;
		float forwardAngle = 70;

		List<Node> globalPath;

		private void Start()
		{
			// get the car controller
			m_Car = GetComponent<CarController>();
			terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
			distanceOffset = defaultDistanceOffset;

			if (isLeader) {
				enemies = GameObject.FindGameObjectsWithTag("Enemy");
				StartCoroutine ("WaitAndStart");
			} else {
				virtualFrames = new List<VirtualLeader> ();
				virtualLeader = Instantiate (virtualLeaderObject, leader.transform.position, Quaternion.identity);
				if (side == "Left") {
					angle = -angle;
					forwardAngle = -forwardAngle;
				}
				referencePosition = virtualLeader.transform.position;
				forwardDirection = transform.forward;
				box = Instantiate (debugBox, referencePosition+Vector3.up*5, Quaternion.identity);
			}
		}

		IEnumerator WaitAndStart(){
			yield return new WaitForSeconds (2f);
			if (isLeader) {
				terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager> ();
				Init ();
				lockedNode = startPos;
				GetOffsetPoint (startPos);
				GetBestPath ();
				StartCoroutine ("UpdateTarget");
			}
		}

		Vector3 targetTurret;

		void GetBestPath(){
			List<Node> attackPoints = GetRandomSingles ();
			float shortestDist = 100000;
			List<Node> bestPath = new List<Node>();
			Node bestAttackPoint = attackPoints[0];
			foreach (Node point in attackPoints) {
				List<Node> pathToPoint = FindPath (lockedNode, point);
				if (pathToPoint[pathToPoint.Count-1].gScore < shortestDist) {
					bestAttackPoint = point;
					bestPath = pathToPoint;
					shortestDist = pathToPoint[pathToPoint.Count-1].gScore;
				}
			}
			targetTurret = FindTurretToAttack (bestAttackPoint);
			globalPath = bestPath;
			if (globalPath.Count < 3) {
				stepTarget = 0;
			}
			ShowPath (globalPath);
		}

		Vector3 FindTurretToAttack(Node attackPoint){
			enemies = GameObject.FindGameObjectsWithTag("Enemy");
			foreach (GameObject enemy in enemies) {
				if(!Physics.Linecast(attackPoint.position, enemy.transform.position)){
					return enemy.transform.position;
				}
			}
			return Vector3.one;
		}

		Vector3 referencePosition;



		private void Update()
		{
			if (Input.GetMouseButtonDown (0)) {
				Debug.Log ("Fixing bug");
				m_Car.Move (0f, 1f, -1f, 0f);
			}
			if (!isLeader) { //used by followers
				virtualFrames.Add (new VirtualLeader (leader.transform.position, leader.transform.forward));

				if (virtualFrames.Count > frameCount) {
					float angleToLeader = CalculateAngleFromVirtual (virtualFrames [0], leader.transform.position);

					virtualLeader.transform.position = virtualFrames [0].position;
					forwardDirection = virtualFrames [0].direction;

					Vector3 obstacleOffset = virtualFrames [frameCount].direction;
					obstacleOffset *= defaultDistanceOffset + 3;
					obstacleOffset = Quaternion.AngleAxis (-forwardAngle, transform.up) * obstacleOffset;
					Vector3 obstaclePosition = obstacleOffset + virtualFrames [frameCount].position; 

					obstacleOffset = -virtualFrames [frameCount].direction;
					obstacleOffset *= defaultDistanceOffset;
					obstacleOffset = Quaternion.AngleAxis (angle, transform.up) * obstacleOffset;
					Vector3 testPos = obstacleOffset + virtualFrames [frameCount].position;

					Vector3 offset = -virtualFrames [0].direction;
					Vector3 test = -virtualFrames [0].direction;
					offset *= distanceOffset;
					offset = Quaternion.AngleAxis (angle, transform.up) * offset;
					referencePosition = offset + virtualLeader.transform.position;
					virtualFrames.RemoveAt (0);


					test *= defaultDistanceOffset + 5;
					test = Quaternion.AngleAxis (angle, transform.up) * test;
					Vector3 testPosition = test + virtualLeader.transform.position;

					if (angleToLeader > 10 && side == "Right") {
						distanceOffset = 3.5f;
					} else if (angleToLeader < -10 && side == "Left") {
						distanceOffset = 3.5f;
					} else if (FreeOfObstacles (obstaclePosition) && FreeOfObstacles (testPosition) && FreeOfObstacles (testPos)) {
						distanceOffset = defaultDistanceOffset;
					} else {
						distanceOffset = 2f;
					}
					//debugging reference position
					box.transform.position = referencePosition + new Vector3 (0, 5, 0);
				}
			} else if (hasStarted) { //used by leader

				Vector3 obstacleOffset = transform.forward;
				obstacleOffset *= 10;
				obstacleOffset = Quaternion.AngleAxis (70, transform.up) * obstacleOffset;
				Vector3 carPosLeft = obstacleOffset + transform.position;

				obstacleOffset = transform.forward;
				obstacleOffset *= 10;
				obstacleOffset = Quaternion.AngleAxis (-70, transform.up) * obstacleOffset;
				Vector3 carPosRight = obstacleOffset + transform.position;


				//obstacle avoidance for leader car.
				if (!FreeOfObstacles (carPosLeft)) {
					referencePosition = carPosRight;
				} else if (!FreeOfObstacles (carPosRight)) {
					referencePosition = carPosLeft;
				} else {
					referencePosition = GetOffsetPoint(lockedNode);
				}
			}
			if (!start) {
				Drive ();
			} else {
				BackOff ();
			}
		}


		Vector3 GetOffsetPoint(Node target){
			Vector3 obstaclePositionRight = Vector3.right * 6 + target.position;
			Vector3 obstaclePositionLeft = Vector3.left * 6 + target.position;
			Vector3 obstaclePositionForward = Vector3.forward * 6 + target.position;
			Vector3 obstaclePositionBack = Vector3.back * 6 + target.position;

			if (!FreeOfObstacles (obstaclePositionLeft)) {
				return obstaclePositionRight;
			} else if (!FreeOfObstacles (obstaclePositionRight)) {
				return obstaclePositionLeft;
			} else if (!FreeOfObstacles (obstaclePositionBack)) {
				return obstaclePositionForward;
			} else if (!FreeOfObstacles (obstaclePositionForward)) {
				return obstaclePositionBack;
			} else {
				return target.position;
			}
		}

		//after reference position is set we drive towards it, followers must wait until leader has started (might be unnecessary)
		bool hasStarted = false;

		float steering = 0f;
		float accelleration = 1f;
		float brake = 0f;

		bool start = true;

		void BackOff(){
			if (Physics.Raycast (transform.position, -transform.forward, 3)) {
				start = false;
			}
			m_Car.Move (0f, 0f, -1f, 0f);
		}
		void Drive(){
			float angleToRefPos = CalculateAngle (referencePosition);
			float distanceToRefPos = Vector3.Distance (transform.position, referencePosition);
			Vector3 direction = (referencePosition - transform.position).normalized;
			Vector3 leaderDirection = (leader.transform.position - transform.position).normalized;

			steering = angleToRefPos / 45f;
			accelleration = 1f;
			brake = 0f;

			bool refPosIsInFront = Vector3.Dot(direction, transform.forward) > 0f;

			bool leaderIsInFront = Vector3.Dot (leaderDirection, transform.forward) > 0f;

			//just some calculation for distance possible to brake for.
			float possibleDistance = 0.0087f * (m_Car.CurrentSpeed * m_Car.CurrentSpeed) + 0.18f * m_Car.CurrentSpeed - 4.45f;

			if (refPosIsInFront && possibleDistance < distanceToRefPos) {
				accelleration = 1f;
				brake = 0f;
			} else if(!isLeader && m_Car.CurrentSpeed > 2){
				
				accelleration = 0f;
				brake = -1f;
			}

			//leader brake before entering dangerzone :P
			if (isLeader && m_Car.CurrentSpeed > 15 && lockedNode.threatScore > 0) {
				accelleration = 0f;
				brake = -1f;

			} else if (isLeader && m_Car.CurrentSpeed < 5) {
				accelleration = 0.5f;
				brake = 0f;
			}
			//leader linecast to target turret and after a 3 seconds recalculated the grid as well as gets a new path.
			if (isLeader && m_Car.CurrentSpeed > 1 && !Physics.Linecast(transform.position+Vector3.up*2, targetTurret) && Mathf.Abs(transform.position.x-globalPath[globalPath.Count-1].position.x) < 10 && Mathf.Abs(transform.position.z-globalPath[globalPath.Count-1].position.z) < 10 ){
				accelleration = 0;
				brake = -1f;
				if (recalculate) {
					Debug.Log ("now");
					recalculate = false;
					StartCoroutine ("GetNewPathAfterDelay");
				}
			}

			//we dont want to drive in front of the leader
			if (!leaderIsInFront && !isLeader && m_Car.CurrentSpeed > 5f) {
				accelleration = 0f;
				brake = -1f;
			} else if (isLeader && !refPosIsInFront) {
				accelleration = 0f;
				brake = 0f;
				steering = 0f;
			}
			m_Car.Move (steering, accelleration, brake, 0f);
		}

		bool TurretIsKilled(){
			enemies = GameObject.FindGameObjectsWithTag("Enemy");
			foreach (GameObject enemy in enemies) {
				if (enemy.transform.position == targetTurret) {
					return false;
				}
			}
			return true;
		}

		//finds a new path to "closestTurret" could maybe be some other node or try to drive to all of them and take the lowest score?
		IEnumerator GetNewPathAfterDelay(){
			while (!TurretIsKilled ()) {
				yield return null;
			}
			RecalculateThreatScore ();
			GetBestPath ();
			ShowPath (globalPath);
			stepLocked = 0;
			stepTarget = 0;
			lockedNode = globalPath [0];
			targetNode = globalPath [0];

			//restarts the coroutine for updating leader target if it has terminated.
			if (!updatingTarget) {
				updatingTarget = true;
				StartCoroutine ("UpdateTarget");
			}
			recalculate = true;
		}

		bool recalculate = true;

		int stepLocked = 0;
		int stepTarget = 0;
		Node lockedNode;
		Node targetNode;
		bool updatingTarget = false;

		//tries to update target every 0.1 sec
		IEnumerator UpdateTarget(){
			float distanceToTarget = 100f;
			float distanceToNext = 100f;
			updatingTarget = true;
			while (globalPath == null) {
				yield return null;
			}
			lockedNode = globalPath [stepLocked];
			targetNode = globalPath [stepTarget];

			hasStarted = true;

			while (stepTarget < globalPath.Count) {

				distanceToTarget = Vector3.Distance (targetNode.position, transform.position);
				distanceToNext = Vector3.Distance (globalPath [stepLocked].position, transform.position);

				Vector3 targetDirection = targetNode.position - transform.position;

				//sensor from car position to possible target
				//is true if the sensor is interrupted by anything inbetween
				bool sensor = Physics.Raycast (transform.position, targetDirection, distanceToTarget);

				//when sensor is false we say we "see" the target and tries to "see" next target in path.
				if(distanceToTarget < 40f && !sensor){
					stepTarget++;
					if (stepTarget == globalPath.Count) {
						lockedNode = targetNode;
						break;
					}
					targetNode = globalPath [stepTarget];
				} 
				//when either we "see" 4 blocks ahead or distance to the next locked target is lower then 13 we update lockedNode
				if ((distanceToNext < 16f && stepTarget - stepLocked > 0) || (stepTarget - stepLocked > 6 && distanceToNext < 40f)) {
					lockedNode = globalPath [stepLocked];
					stepLocked++;
				}
				yield return null;
			}
			updatingTarget = false;
		}

		//check if a certain Vec3 position has an obstacle returns true if it is free.
		bool FreeOfObstacles(Vector3 position){
			if (terrain_manager.myInfo.traversability [terrain_manager.myInfo.get_i_index (position.x), terrain_manager.myInfo.get_j_index (position.z)] == 1f) {
				return false;
			} else {
				return true;
			}
		}

		//some random angle used in p4
		float CalculateAngleFromVirtual(VirtualLeader virtualCar, Vector3 target){
			Vector3 targetDirection = target - virtualCar.position;
			float angle = Vector3.Angle (targetDirection, virtualCar.direction);
			Vector3 test = Vector3.Cross (targetDirection, virtualCar.direction);
			if(test.y > 0){
				angle = -angle;
			}
			return angle;
		}

		//calculates angle from transform to target.
		float CalculateAngle(Vector3 target){
			Vector3 targetDirection = target - transform.position;
			float angle = Vector3.Angle (targetDirection, transform.forward);
			Vector3 test = Vector3.Cross (targetDirection, transform.forward);
			if(test.y > 0){
				angle = -angle;
			}
			return angle;
		}

		List<Node> GetRandomSingles(){
			List<Node> singles = new List<Node> ();
			for (int i = 0; i < 40; i+=10) {
				for (int j = 0; j < 40; j+=10) {
					Node foundOne = GetFromSmallQuadrant (i, j, 100);
					if (foundOne != null) {
						singles.Add (foundOne);
					}
				}
			}
			if (singles == null) {
				for (int i = 0; i < 40; i+=10) {
					for (int j = 0; j < 40; j+=10) {
						Node foundOne = GetFromSmallQuadrant (i, j, 200);
						if (foundOne != null) {
							singles.Add (foundOne);
						}
					}
				}
			}
			return singles;
		}

		Node GetFromSmallQuadrant(int index_x, int index_z, int maxThreat){
			Node bestNode = null;
			float olddistance = 10000;
			for (int i = 0; i < 10; i++) {
				for (int j = 0; j < 10; j++) {
					if (FreeOfObstacles (nodes [index_x + i, index_z + j].position) && nodes[index_x + i, index_z + j].threatScore == maxThreat) {
						float dist = Vector3.Distance (transform.position, nodes [index_x + i, index_z + j].position);
						if (dist < olddistance) {
							bestNode = nodes [index_x + i, index_z + j];
							olddistance = dist;
						}
					}
				}
			}
			return bestNode;
		}

		//recalculates the whole map and the score from which the "rest" turrets can see.
		void RecalculateThreatScore(){
			enemies = GameObject.FindGameObjectsWithTag("Enemy");
			RaycastHit hit;
			foreach (Transform child in debugHolder) {
				GameObject.Destroy (child.gameObject);
			}
			for (int i = 0; i < 40; i++) {
				for (int j = 0; j < 40; j++) {
					if(FreeOfObstacles(nodes[i,j].position)){
						int score = 0;
						nodes [i, j].gScore = 0;
						nodes [i, j].hScore = 0;
						foreach (GameObject enemy in enemies) {
							if (!Physics.Linecast(nodes[i,j].position+Vector3.up*2, enemy.transform.position, out hit, ~(1<<2))) {
								score += 100;
							}
						}
						//Debugging used to see all nodes with a score of 100 (1 turret can be seen)
						if (debugMap && score < 200 && score > 0) {
							Instantiate (okCube, new Vector3 (nodes[i,j].position.x, 10f, nodes[i,j].position.z), Quaternion.identity, debugHolder);
						}
						nodes [i, j].threatScore = score;
					}
				}
			}
		}

		//the A*
		public List<Node> FindPath(Node start, Node goal){
			List<Node> open = new List<Node> ();
			List<Node> closed = new List<Node> ();
			open.Add (start);

			while (open.Count > 0) {
				Node current = open [0];
				for (int i = 1; i < open.Count; i++) {
					if ((open [i].gScore + open [i].hScore) < (current.gScore + current.hScore) || (open [i].gScore + open [i].hScore) == (current.gScore + current.hScore) && open [i].hScore < current.hScore) {
						current = open [i];
					}
				}
				open.Remove (current);
				closed.Add (current);
				if(current.position == goal.position){
					return CreatePath (start, goal);

				}
				foreach (Node neighbour in Neighbours(current)) {
					if (!(terrain_manager.myInfo.traversability [neighbour.blockX,neighbour.blockZ] == 0f) || closed.Contains(neighbour)) {
						continue;
					}
					int newCost = current.gScore + Heuristic(neighbour, current, current.cameFrom);
					if (newCost < neighbour.gScore || !open.Contains(neighbour)) {
						neighbour.gScore = newCost;
						neighbour.hScore = (int)Vector3.Distance (goal.position, neighbour.position);// Distance (goal, neighbour);
						neighbour.cameFrom = current;

						if (!open.Contains(neighbour)) {
							open.Add (neighbour);
						}
					}
				}
			}
			return null;
		}

		//Creating the path by using the "cameFrom" component of a node, traversing the path from goal to start and reversing it.
		public List<Node> CreatePath(Node start, Node end){
			List<Node> path = new List<Node> ();
			Node current = end;
			while (current != start) {
				path.Add (current);
				current = nodes [current.newX, current.newZ].cameFrom;
			}
			path.Reverse ();
			//ShowPath (path);
			return path;
		}

		//returns all adjacent neighbours.
		public List<Node> Neighbours(Node node){
			List<Node> neighbours = new List<Node> ();
			for (int i = -1; i <= 1; i++) {
				for (int j = -1; j <= 1; j++) {
					if (i == 0 && j == 0) {
						continue;
					}
					int adjacentX = (int)node.newX + i;
					int adjacentZ = (int)node.newZ + j;
					if (adjacentX >= numblocksX && adjacentX < numblocksX*x_N && adjacentZ >= numblocksX && adjacentZ < numblocksZ*z_N) {
						neighbours.Add (nodes[adjacentX,adjacentZ]);
					}
				}
			}
			return neighbours;
		}

		//instantiates a linerenderer for a path that is wished to be displayed on the map.
		public GameObject lineRenderers;
		void ShowPath (List<Node> path){
			int i = 0;
			GameObject test = Instantiate (lineRenderers);
			LineRenderer show = test.GetComponent<LineRenderer> ();

			show.positionCount = path.Count;
			while (i < path.Count) {
				Vector3 position = new Vector3 (path [i].position.x, 1f, path [i].position.z);
				show.SetPosition (i, position);
				i++;
			}
		}

		//A* heuristic, added so that threatscore above 100 is BAD
		int Heuristic(Node candidateNode, Node currentNode, Node previousNode){
			int directionCurrX = candidateNode.newX - currentNode.newX;
			int directionCurrZ = candidateNode.newZ - currentNode.newZ;

			if (previousNode != null) {
				int directionPrevX = currentNode.newX - previousNode.newX;
				int directionPrevZ = currentNode.newZ - previousNode.newZ;
				if (candidateNode.threatScore > 100) {
					return 5000;
				}
				//if we travel the same direction
				if (directionPrevX == directionCurrX && directionPrevZ == directionCurrZ) {
					//if the same direction is diagonal
					if (Mathf.Abs (directionCurrX) + Mathf.Abs(directionCurrZ) == 2) {
						if (terrain_manager.myInfo.traversability [currentNode.blockX, candidateNode.blockZ] == 1f || terrain_manager.myInfo.traversability [candidateNode.blockX, currentNode.blockZ] == 1f) {
							return 20000; //if there's an adjacent obstacle- bad
						} else {
							return 30; //otherwise can keep going diagonally.
						}
					}
					//if going straight but there's an obstacle to the right or left
					if (terrain_manager.myInfo.traversability [nodes [currentNode.newX - 1, currentNode.newZ - 1].blockX, nodes [currentNode.newX - 1, currentNode.newZ - 1].blockZ] == 1f || terrain_manager.myInfo.traversability [nodes [currentNode.newX + 1, currentNode.newZ + 1].blockX, nodes [currentNode.newX + 1, currentNode.newZ + 1].blockZ] == 1f) {
						return 10;
					} else {
						return 10;
					}
				}
				if (directionCurrX == -1*directionPrevX || directionCurrZ == -1*directionPrevZ) {
					return 5000;
				}
				if (directionCurrX != directionPrevX && directionCurrZ != directionPrevZ) {
					return 200;
				}
				//if not travelling same direction & going diagonally (steering required)
				if (Mathf.Abs (directionCurrX) + Mathf.Abs(directionCurrZ) == 2) {
					//if there's an adjacent obstacle - bad
					if (terrain_manager.myInfo.traversability [currentNode.blockX, candidateNode.blockZ] == 1f || terrain_manager.myInfo.traversability [candidateNode.blockX, currentNode.blockZ] == 1f) {
						return 400;
					} else {
						return 20;
					}
				}
				return 30;
			}
			//if no previous node is known
			return 500;
		}

		Node[,] nodes;
		Node[] turrets;
		int x_N;
		int z_N;
		int numblocksX;
		int numblocksZ;
		Node startPos;
		public GameObject tester;
		[SerializeField]
		bool debugMap;
		[SerializeField]
		GameObject badCube;
		[SerializeField]
		GameObject okCube;
		int numberOfTurrets = 0;
		//Lots of stuff initializing our own grid out of world coordinates and precalculated grid.
		public void Init(){
			float worldposXmin = terrain_manager.myInfo.x_low;
			float worldposXmax = terrain_manager.myInfo.x_high;

			float worldposZmin = terrain_manager.myInfo.z_low;
			float worldposZmax = terrain_manager.myInfo.z_high;

			x_N = terrain_manager.myInfo.x_N;
			z_N = terrain_manager.myInfo.z_N;

			float blocksizeX = (worldposXmax - worldposXmin) / x_N;
			float blocksizeZ = (worldposZmax - worldposZmin) / z_N;

			numblocksX = 2;
			numblocksZ = 2;
			float newGridSizeX = blocksizeX/2;
			float newGridSizeZ = blocksizeZ/2;

			int newX = (int)((worldposXmax - worldposXmin) / newGridSizeX);
			int newZ = (int)((worldposZmax - worldposZmin) / newGridSizeZ);

			//Calculate how many times blocksizeX (the blocksizes given to us) must be devided to be smaller then 5 pixel wide/long
			nodes = new Node[newX, newZ];
			int oldx = -1;
			int oldz = -1;
			float posX = worldposXmin;
			float posZ = worldposZmin;

			enemies = GameObject.FindGameObjectsWithTag("Enemy");
			turrets = new Node[enemies.Length];
			for (int i = 0; i < newX; i++) {
				if (i % (numblocksX) == 0) {
					oldx++;
				}
				if (i == 0) {
					oldx = 0;
					posX = worldposXmin + newGridSizeX / 2;
				}else {
					posX += newGridSizeX;
				}
				for (int j = 0; j < newZ; j++) {
					if (j % (numblocksZ) == 0) {
						oldz++;
					} 
					if (j == 0) {
						oldz = 0;
						posZ = worldposZmin + newGridSizeZ / 2;
					}else {
						posZ += newGridSizeZ;
					}

					Vector3 nodePosition = new Vector3 (posX, 0f, posZ);
					bool hasturret = false;
					GameObject test = Instantiate (tester, nodePosition, Quaternion.identity);
					int score = 0;

					//initiates the threatscore
					foreach (GameObject turret in enemies) {
						
						if (test.GetComponent<BoxCollider> ().bounds.Contains(turret.transform.position)) {
							hasturret = true;
						
						}

						if (FreeOfObstacles(nodePosition)) {
							RaycastHit hit;
							if (!Physics.Linecast(nodePosition+Vector3.up*2, turret.transform.position, out hit, ~(1<<2))) {
								score += 100;
							}
						}
					}

					nodes [i, j] = new Node (nodePosition,i,j,oldx,oldz);
					nodes [i, j].threatScore = score;
					nodes [i, j].turret = hasturret;

					if (hasturret) {
						turrets [numberOfTurrets] = nodes [i, j];
						turrets [numberOfTurrets].turretNumber = numberOfTurrets+1;
						numberOfTurrets++;
					}

					if (test.GetComponent<BoxCollider> ().bounds.Contains(transform.position))  {
						nodes [i, j].cameFrom = nodes [i, j - 1];
						startPos = nodes [i, j];
					}

					Destroy (test);
					if (debugMap) {
						//Debug map traversability.
						ShowMapTraversability(nodes[i,j]);
					}
				}
			}
		}

		void ShowMapTraversability(Node node){
			if (terrain_manager.myInfo.traversability [node.blockX, node.blockZ] == 1f) {
				//Instantiate (badCube, new Vector3 (node.position.x, 10f, node.position.z), Quaternion.identity, debugHolder);
			} else if(node.threatScore < 200 && node.threatScore > 0){
				Instantiate (okCube, new Vector3 (node.position.x, 10f, node.position.z), Quaternion.identity, debugHolder);
			}
		}
	}
}
