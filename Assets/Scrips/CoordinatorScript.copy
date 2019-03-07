using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CoordinatorScript : MonoBehaviour
{
	public GameObject terrain_manager_game_object;
	TerrainManager terrain_manager;
	public GameObject car1;
	public GameObject car2;
	public GameObject car3;
	public Node[,] nodes;
	public Node[] turrets;
	GameObject[] enemies;
	public GameObject lineRenderers;
    // Start is called before the first frame update
    void Start()
    {
		StartCoroutine ("delayedStart");
	}

	IEnumerator delayedStart(){
		//had to delay for terrain_manager to fix the map, strange thing
		yield return new WaitForSeconds (2f);
		terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager> ();
		Init ();
		OrderTurrets();

		ShowPath (firstPath);
		ShowPath (secondPath);
		ShowPath (thirdPath);

		Debug.Log (firstPath.Count);
		Debug.Log (secondPath.Count);
		Debug.Log(thirdPath.Count);
		//printDistanceMatrix ();
	}

    // Update is called once per frame
    void Update()
    {
		if (Input.GetMouseButtonDown (0)) {
			Time.timeScale = 10f;
		}
    }

	void SwapFirsts(){
		if(firstPath.Count > secondPath.Count + distanceMatrix[0,firstPathTurrets[1].turretNumber].Count*2){
			//give first to second
			List<Node> path1 = FindPath(startPosCar1,firstPathTurrets[2]);
			int removeCount = 0;
			foreach (Node target in firstPath) {
				if (target != firstPathTurrets [2]) {
					removeCount++;
				} else {
					removeCount++;
					break;
				}
			}
			firstPath.RemoveRange (0, removeCount);
			removeCount = 0;
			firstPath.InsertRange (0, path1);
			List<Node> path2 = FindPath (startPosCar2, firstPathTurrets [1]);
			//nodes[firstPathTurrets[1].newX,firstPathTurrets[1].newZ].cameFrom 
			List<Node> path21 = FindPath (firstPathTurrets [1], secondPathTurrets [1]);
			path2.AddRange (path21);
			foreach (Node target in secondPath) {
				if (target != secondPathTurrets [1]) {
					removeCount++;
				} else {
					removeCount++;
					break;
				}
			}
			secondPath.RemoveRange (0, removeCount);
			secondPath.InsertRange (0, path2);
		}else if(firstPath.Count > thirdPath.Count + distanceMatrix[0,firstPathTurrets[1].turretNumber].Count*2){
			//give first to third
			List<Node> path1 = FindPath(startPosCar1,firstPathTurrets[2]);
			int removeCount = 0;
			foreach (Node target in firstPath) {
				if (target != firstPathTurrets [2]) {
					removeCount++;
				} else {
					removeCount++;
					break;
				}
			}
			firstPath.RemoveRange (0, removeCount);
			removeCount = 0;
			firstPath.InsertRange (0, path1);
			List<Node> path3 = FindPath (startPosCar3, firstPathTurrets [1]);
			//nodes[firstPathTurrets[1].newX,firstPathTurrets[1].newZ].cameFrom 
			List<Node> path31 = FindPath (firstPathTurrets [1], thirdPathTurrets [1]);
			path3.AddRange (path31);
			foreach (Node target in thirdPath) {
				if (target != thirdPathTurrets [1]) {
					removeCount++;
				} else {
					removeCount++;
					break;
				}
			}
			thirdPath.RemoveRange (0, removeCount);
			thirdPath.InsertRange (0, path3);
		}else if(secondPath.Count > firstPath.Count + distanceMatrix[0,secondPathTurrets[1].turretNumber].Count*2){
			//give second to first
		}else if(secondPath.Count > thirdPath.Count + distanceMatrix[0,secondPathTurrets[1].turretNumber].Count*2){
			//give second to third
		}else if(thirdPath.Count > firstPath.Count + distanceMatrix[0,thirdPathTurrets[1].turretNumber].Count*2){
			//give third to first
		}else if(thirdPath.Count > secondPath.Count + distanceMatrix[0,thirdPathTurrets[1].turretNumber].Count*2){
			//give third to second
		}
	}


	List<Node>[,] distanceMatrix;
	public List<Node> firstPath;
	public List<Node> secondPath;
	public List<Node> thirdPath;

	List<Node> firstPathTurrets;
	List<Node> secondPathTurrets;
	List<Node> thirdPathTurrets;

	public void OrderTurrets(){
		distanceMatrix = new List<Node>[turrets.Length+1, turrets.Length+1];
		Node old1 = startPosCar1;
		Node old2 = startPosCar2;
		Node old3 = startPosCar3;
		firstPath = new List<Node> ();
		secondPath = new List<Node> ();
		thirdPath = new List<Node> ();
		firstPathTurrets = new List<Node>();
		firstPathTurrets.Add (startPosCar1);
		secondPathTurrets = new List<Node>();
		secondPathTurrets.Add (startPosCar2);
		thirdPathTurrets = new List<Node>();
		thirdPathTurrets.Add (startPosCar3);

		for (int i = 0; i < numberOfTurrets; i++) {
			int currentLow = 10000;
			//int currentLow2 = 10000;
			//int currentLow3 = 10000;
			Node best = null;
			List<Node> bestPath = null;
			int bestCar = 0;
			int bestJ = 0;
			for (int j = i; j < numberOfTurrets; j++) {
				List<Node> path = null;
				List<Node> path2 = null;
				List<Node> path3 = null;
				if (distanceMatrix [old1.turretNumber, turrets [j].turretNumber] != null) {
					path = distanceMatrix [old1.turretNumber, turrets [j].turretNumber];
				} else {
					resetNodes ();
					path = FindPath (old1, turrets [j]);
				} 
				if (distanceMatrix [old2.turretNumber, turrets [j].turretNumber] != null) {
					path2 = distanceMatrix [old2.turretNumber, turrets [j].turretNumber];
				} else {
					resetNodes ();
					path2 = FindPath (old2, turrets [j]);
				} 
				if (distanceMatrix [old3.turretNumber, turrets [j].turretNumber] != null) {
					path3 = distanceMatrix [old3.turretNumber, turrets [j].turretNumber];
				} else {
					resetNodes ();
					path3 = FindPath (old3, turrets [j]);
				}
				float birdDistance1 = Vector3.Distance (old1.position, turrets [j].position);
				float birdDistance2 = Vector3.Distance (old2.position, turrets [j].position);
				float birdDistance3 = Vector3.Distance (old3.position, turrets [j].position);
				bool first = false;
				bool second = false;
				bool third = false;
				if (path2.Count + secondPath.Count == path.Count + firstPath.Count && path2.Count + secondPath.Count < path3.Count + thirdPath.Count) {
					if (birdDistance2 < birdDistance1) {
						second = true;
					} else {
						first = true;
					}
				} else if (path2.Count + secondPath.Count == path3.Count + thirdPath.Count && path2.Count + secondPath.Count < path.Count + firstPath.Count) {
					if (birdDistance2 < birdDistance3) {
						second = true;
					} else {
						third = true;
					}
				} else if (path3.Count + thirdPath.Count == path.Count + firstPath.Count && path.Count + firstPath.Count < path2.Count + secondPath.Count) {
					if (birdDistance3 < birdDistance1) {
						third = true;
					} else {
						first = true;
					}
				}

				if ((!first && !third && path2.Count + secondPath.Count <= path.Count + firstPath.Count && path2.Count + secondPath.Count <= path3.Count + thirdPath.Count) || (path2.Count < path.Count / 2 && path2.Count < path3.Count / 2)) {
					if (path2.Count + secondPath.Count < currentLow) {
						best = turrets [j];
						bestJ = j;
						currentLow = path2.Count + secondPath.Count;
						bestPath = path2;
						bestCar = 2;
					}
				}
				if ((!first && !second && path3.Count + thirdPath.Count <= path.Count + firstPath.Count && path3.Count + thirdPath.Count <= path2.Count + secondPath.Count) || (path3.Count < path.Count / 2 && path3.Count < path2.Count / 2)) {
					if (path3.Count + thirdPath.Count < currentLow) {
						best = turrets [j];
						bestJ = j;
						currentLow = path3.Count + thirdPath.Count;
						bestPath = path3;
						bestCar = 3;
					}
				} 
				if ((!second && !third && path.Count + firstPath.Count <= path3.Count + thirdPath.Count && path.Count + firstPath.Count <= path2.Count + secondPath.Count) || (path.Count < path2.Count / 2 && path.Count < path3.Count / 2)) {
					if (path.Count + firstPath.Count < currentLow) {
						best = turrets [j];
						bestJ = j;
						currentLow = path.Count + firstPath.Count;
						bestPath = path;
						bestCar = 1;
					}
				}
				distanceMatrix [old1.turretNumber, turrets [j].turretNumber] = path;
				distanceMatrix [old2.turretNumber, turrets [j].turretNumber] = path2;
				distanceMatrix [old3.turretNumber, turrets [j].turretNumber] = path3;
			}
			turrets [bestJ] = turrets [i];
			turrets [i] = best;

			if (bestCar == 2) {
				secondPath.AddRange (bestPath);
				old2 = turrets [i];
				secondPathTurrets.Add (old2);
			} else if (bestCar == 3) {
				thirdPath.AddRange (bestPath);
				old3 = turrets [i];
				thirdPathTurrets.Add (old3);
			} else if (bestCar == 1) {
				firstPath.AddRange (bestPath);
				old1 = turrets [i];
				firstPathTurrets.Add (old1);
			}
		}
		//SwapFirsts ();
	}

	void resetNodes(){
		for (int i = 0; i < x_N * 2; i++) {
			for (int j = 0; j < z_N * 2; j++) {
				Node temp = nodes [i, j];
				nodes [i, j] = new Node (temp.position, temp.newX, temp.newZ, temp.blockX, temp.blockZ);
			}
		}
	}

	int x_N;
	int z_N;
	int numblocksX;
	int numblocksZ;
	Node startPosCar1;
	Node startPosCar2;
	Node startPosCar3;
	public GameObject tester;
	[SerializeField]
	bool debugMap;
	[SerializeField]
	GameObject badCube;
	[SerializeField]
	GameObject okCube;
	int numberOfTurrets = 0;
	//Lots of stuff initializing our own grid out of world coordinates and precalculated grid.
	void Init(){
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
				foreach (GameObject turret in enemies) {
					if (test.GetComponent<BoxCollider> ().bounds.Contains(turret.transform.position)) {
						hasturret = true;
					}
				}

				nodes [i, j] = new Node (nodePosition,i,j,oldx,oldz);
				nodes [i, j].turret = hasturret;
				if (hasturret) {
					turrets [numberOfTurrets] = nodes [i, j];
					turrets [numberOfTurrets].turretNumber = numberOfTurrets+1;
					numberOfTurrets++;
				}

				if (test.GetComponent<BoxCollider> ().bounds.Contains(car1.transform.position))  {
					startPosCar1 = nodes [i, j];
				}
				if (test.GetComponent<BoxCollider> ().bounds.Contains(car2.transform.position))  {
					startPosCar2 = nodes [i, j];
				}
				if (test.GetComponent<BoxCollider> ().bounds.Contains(car3.transform.position))  {
					startPosCar3 = nodes [i, j];
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
			Instantiate (badCube, new Vector3 (node.position.x, 10f, node.position.z), Quaternion.identity);
		} else {
			Instantiate (okCube, new Vector3 (node.position.x, 10f, node.position.z), Quaternion.identity);
		}
	}

	//simple A* algorithm
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
	//traversing the saved path and reversing it so it doesnt start at end.
	public List<Node> CreatePath(Node start, Node end){
		List<Node> path = new List<Node> ();
		Node current = end;
		while (current != start) {
			path.Add (current);
			current = nodes [current.newX, current.newZ].cameFrom;
		}
		path.Reverse ();
		return path;
	}

	//Heuristic function determining cost of travelling to a certain neighbor.
	// here might be good idea to come up with good values for cost, depending on which node we checking.
	int Heuristic(Node candidateNode, Node currentNode, Node previousNode){
		int directionCurrX = candidateNode.newX - currentNode.newX;
		int directionCurrZ = candidateNode.newZ - currentNode.newZ;

		if (previousNode == null && firstPath.Count > 0 && currentNode == firstPath [firstPath.Count-1]) {
			previousNode = firstPath [firstPath.Count - 2];
		}
		if (previousNode == null && secondPath.Count > 0 && currentNode == secondPath [secondPath.Count-1]) {
			previousNode = secondPath [secondPath.Count - 2];
		}

		if (previousNode == null && thirdPath.Count > 0 && currentNode == thirdPath [thirdPath.Count-1]) {
			previousNode = thirdPath [thirdPath.Count - 2];
		}

		if (previousNode != null) {
			int directionPrevX = currentNode.newX - previousNode.newX;
			int directionPrevZ = currentNode.newZ - previousNode.newZ;

			//if we travel the same direction
			if (directionPrevX == directionCurrX && directionPrevZ == directionCurrZ) {
				//if the same direction is diagonal
				if (Mathf.Abs (directionCurrX) + Mathf.Abs(directionCurrZ) == 2) {
					if (terrain_manager.myInfo.traversability [currentNode.blockX, candidateNode.blockZ] == 1f || terrain_manager.myInfo.traversability [candidateNode.blockX, currentNode.blockZ] == 1f) {
						return 200; //if there's an adjacent obstacle- bad
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

	//returns list of all neighbours
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
	//Slowly showing the path and the order of points in the plane.
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

	/*
		 * every node consist of:
		 * blockX - x-position in predetermined grid
		 * blockZ - z-position in predetermined grid
		 * newX - x-position in new grid
		 * newZ - z-position in new grid
		 * position - world coordinates position of the node
		 * gScore - heuristic score for travelling to this node.
		 * hScore - euclidean distance to goal
		 * cameFrom - the node from which were travelled to get to this node.
		 * */
	public class Node{

		public int blockX;
		public int blockZ;

		public int newX;
		public int newZ;
		public Vector3 position;

		public int gScore;
		public int hScore;

		public Node cameFrom;

		public bool turret;
		public int turretNumber = 0;

		public Node (Vector3 worldPos, int newGridX, int newGridZ, int gridPosX, int gridPosZ){
			position = worldPos;
			newX = newGridX;
			newZ = newGridZ;
			blockX = gridPosX;
			blockZ = gridPosZ;
		}
	}

}
