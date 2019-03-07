using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI1 : MonoBehaviour
    {
        public BoxCollider carCollider;
        public GameObject terrain_manager_game_object;
        public TerrainManager terrain_manager;
        public TerrainInfo info;
        public Node1[,] map;
        public SubNode1[,] submap;
        public float xGridSize;
        public float zGridSize;
        public float xSubGridSize;
        public float zSubGridSize;
        public int xN;
        public int zN;
        public int xSubN;
        public int zSubN;
        public static List<SubNode1> path0 = new List<SubNode1>();
        public static List<SubNode1> path1 = new List<SubNode1>();
        public static List<SubNode1> path2 = new List<SubNode1>();
        public List<SubNode1> waypoints;

        private CarController m_Car; // the car controller we want to use
        private GameManager gameManager;
        int iteration0;
        int iteration1;
        int iteration2;
        private Rigidbody carRigidbody;
        float angle;
        public float topSpeed = 15f;
        public float cruiseSpeed = 12f;
        float speed;
        float breakForce;
        bool crashed = false;
        bool goalReached = false;
        int crashCounter = 0;


        public GameObject[] friends;
        public GameObject[] enemies;

        float GetXPos(int i, float xGridSize)
        {
            return info.x_low + (xGridSize / 2) + xGridSize * i;
        }

        float GetZPos(int i, float zGridSize)
        {
            return info.z_low + (zGridSize / 2) + zGridSize * i;
        }

        float GetXSubPos(int i, float xSubGridSize)
        {
            return info.x_low + (xSubGridSize / 2) + xSubGridSize * i;
        }

        float GetZSubPos(int i, float zSubGridSize)
        {
            return info.z_low + (zSubGridSize / 2) + zSubGridSize * i;
        }

        private void CreateMap()
        {
            Vector3 pos;
            // float parameter = 1.8f;
            int xStep = 1;
            int zStep = 1;
            xGridSize = (info.x_high - info.x_low) / info.x_N / xStep;
            zGridSize = (info.z_high - info.z_low) / info.z_N / zStep;
            xN = xStep * info.x_N;
            zN = zStep * info.z_N;
            map = new Node1[xN, zN];
            for (int i = 0; i < xN; i++)
            {
                for (int j = 0; j < zN; j++)
                {
                    if (info.traversability[(int)Mathf.Floor(i / xStep), (int)Mathf.Floor(j / zStep)] > 0.5f)
                    {
                        pos = new Vector3(GetXPos(i, xGridSize), 0f, GetZPos(j, zGridSize));
                        map[i, j] = new Node1(false, false, pos, i, j);
                    }
                    else
                    {
                        pos = new Vector3(GetXPos(i, xGridSize), 0f, GetZPos(j, zGridSize));
                        map[i, j] = new Node1(true, false, pos, i, j);
                    }
                }
            }
        }

        private void CreateSubMap()
        {
            Vector3 pos;
            // float parameter = 1.8f;
            int xStep = 2;
            int zStep = 2;
            xSubGridSize = (info.x_high - info.x_low) / info.x_N / xStep;
            zSubGridSize = (info.z_high - info.z_low) / info.z_N / zStep;
            xSubN = xStep * info.x_N;
            zSubN = zStep * info.z_N;
            submap = new SubNode1[xSubN, zSubN];
            for (int i = 0; i < xSubN; i++)
            {
                for (int j = 0; j < zSubN; j++)
                {
                    if (info.traversability[(int)Mathf.Floor(i / xStep), (int)Mathf.Floor(j / zStep)] > 0.5f)
                    {
                        pos = new Vector3(GetXSubPos(i, xSubGridSize), 0f, GetZSubPos(j, zSubGridSize));
                        submap[i, j] = new SubNode1(false, false, pos, i, j);
                        float temp = 100;
                        foreach (Node1 node in map)
                        {
                            if (Mathf.Abs(node.position.x - pos.x) + Mathf.Abs(node.position.z - pos.z) < temp)
                            {
                                temp = Mathf.Abs(node.position.x - pos.x) + Mathf.Abs(node.position.z - pos.z);
                                submap[i, j].MegaNode1 = node;
                            }
                        }

                        //Vector3 neareastNode1Pos;
                        //for (int row = -1; row < 2; row += 2)
                        //{
                        //    for (int col = -1; col < 2; col += 2)
                        //    {
                        //        neareastNode1Pos.x = pos.x + row * 5;
                        //        neareastNode1Pos.y = pos.y;
                        //        neareastNode1Pos.z = pos.z + col * 5;
                        //        submap[i, j].MegaNode1 = GetNode1InMap(neareastNode1Pos, xGridSize, zGridSize);
                        //        if (submap[i, j].MegaNode1 != null)
                        //            break;
                        //    }
                        //}
                    }
                    else
                    {
                        pos = new Vector3(GetXSubPos(i, xSubGridSize), 0f, GetZSubPos(j, zSubGridSize));
                        submap[i, j] = new SubNode1(true, false, pos, i, j);
                        float temp = 100;
                        foreach (Node1 node in map)
                        {
                            if (Mathf.Abs(node.position.x - pos.x) + Mathf.Abs(node.position.z - pos.z) < temp)
                            {
                                temp = Mathf.Abs(node.position.x - pos.x) + Mathf.Abs(node.position.z - pos.z);
                                submap[i, j].MegaNode1 = node;
                            }
                        }
                        //Vector3 neareastNode1Pos;
                        //for (int row = -1; row < 2; row += 2)
                        //{
                        //    for (int col = -1; col < 2; col += 2)
                        //    {
                        //        neareastNode1Pos.x = pos.x + row * 5;
                        //        neareastNode1Pos.y = pos.y;
                        //        neareastNode1Pos.z = pos.z + col * 5;
                        //        submap[i, j].MegaNode1 = GetNode1InMap(neareastNode1Pos, xGridSize, zGridSize);
                        //        if (submap[i, j].MegaNode1 != null)
                        //            break;
                        //    }
                        //}
                    }
                }
            }
        }

        private List<Node1> GetNeighbours(Node1 currentNode1)
        {
            List<Node1> neighbours = new List<Node1>();

            for (int i = -1; i < 2; i++)
            {
                int j = 0;
                if (i == 0 && j == 0)
                {
                    continue;
                }
                int xPos = currentNode1.xGrid + i;
                int zPos = currentNode1.zGrid + j;
                if (xPos >= 0 && xPos < xN && zPos >= 0 && zPos < zN)
                {
                    neighbours.Add(map[xPos, zPos]);
                }
            }
            for (int j = -1; j < 2; j++)
            {
                int i = 0;
                if (i == 0 && j == 0)
                {
                    continue;
                }
                int xPos = currentNode1.xGrid + i;
                int zPos = currentNode1.zGrid + j;
                if (xPos >= 0 && xPos < xN && zPos >= 0 && zPos < zN)
                {
                    neighbours.Add(map[xPos, zPos]);
                }
            }
            return neighbours;
        }

        private List<SubNode1> GetSubNeighbours(SubNode1 currentNode1)
        {
            List<SubNode1> neighbours = new List<SubNode1>();

            for (int i = -1; i < 2; i++)
            {
                int j = 0;
                if (i == 0 && j == 0)
                {
                    continue;
                }
                int xPos = currentNode1.xSubGrid + i;
                int zPos = currentNode1.zSubGrid + j;
                if (xPos >= 0 && xPos < xN && zPos >= 0 && zPos < zN)
                {
                    neighbours.Add(submap[xPos, zPos]);
                }
            }
            for (int j = -1; j < 2; j++)
            {
                int i = 0;
                if (i == 0 && j == 0)
                {
                    continue;
                }
                int xPos = currentNode1.xSubGrid + i;
                int zPos = currentNode1.zSubGrid + j;
                if (xPos >= 0 && xPos < xN && zPos >= 0 && zPos < zN)
                {
                    neighbours.Add(submap[xPos, zPos]);
                }
            }
            return neighbours;
        }

        private Node1 GetNode1InMap(Vector3 worldPosition, float xIntervel, float zIntervel)
        {
            int x = 0;
            int z = 0;
            float count = info.x_low + xIntervel;
            Node1 currentNode1;
            while (count <= worldPosition.x)
            {
                x++;
                count += xIntervel;
            }
            count = info.z_low + zIntervel;
            while (count <= worldPosition.z)
            {
                z++;
                count += zIntervel;
            }
            if (x < map.GetLength(0) && z < map.GetLength(1))
            {
                currentNode1 = map[x, z];
                return currentNode1;
            }
            else return null;
        }

        private SubNode1 GetSubNode1InMap(Vector3 worldPosition, float xIntervel, float zIntervel)
        {
            int x = 0;
            int z = 0;
            float count = info.x_low + xIntervel;
            SubNode1 currentNode1;
            while (count <= worldPosition.x)
            {
                x++;
                count += xIntervel;
            }
            count = info.z_low + zIntervel;
            while (count <= worldPosition.z)
            {
                z++;
                count += zIntervel;
            }
            currentNode1 = submap[x, z];
            return currentNode1;
        }

        private Node1 ExpandTree(Vector3 startPos, Node1 thisNode1, Node1 node1, Node1 node2)
        {
            Node1 parentNode1 = thisNode1.parent;
            List<Node1> parentNeighbours = new List<Node1>();
            bool hasNeighbour = false;
            Node1 bestNeighbour = null;

            while (!hasNeighbour)
            {
                parentNeighbours = GetNeighbours(parentNode1);
                if (parentNeighbours.Count == 0)
                {
                    hasNeighbour = false;
                }
                else
                {
                    float maxDistance = 0;
                    foreach (Node1 parentNeighbour in parentNeighbours)
                    {
                        if (parentNeighbour.occupied == false && parentNeighbour.traversable == true)
                        {
                            hasNeighbour = true;
                            float distance = Vector3.Distance(parentNeighbour.position, node1.position) + Vector3.Distance(parentNeighbour.position, node2.position);
                            if (distance >= maxDistance)
                            {
                                bestNeighbour = parentNeighbour;
                                maxDistance = distance;
                            }
                        }
                    }
                    if (hasNeighbour)
                    {
                        bestNeighbour.occupied = true;
                        bestNeighbour.parent = parentNode1;
                        parentNode1.children.Add(bestNeighbour);
                        return bestNeighbour;
                    }
                    else
                    {
                        parentNode1 = parentNode1.parent;
                        if (parentNode1.position == startPos)
                            return null;
                    }
                }
            }
            return null;
        }

        private void GenerateTree(Node1 node0, Node1 node1, Node1 node2)
        {
            List<Node1> neighbours0 = new List<Node1>();
            List<Node1> neighbours1 = new List<Node1>();
            List<Node1> neighbours2 = new List<Node1>();

            Vector3 start0Pos = node0.position;
            Vector3 start1Pos = node1.position;
            Vector3 start2Pos = node2.position;
            print(start0Pos.x);

            Node1 new_node0 = node0;
            Node1 new_node1 = node1;
            Node1 new_node2 = node2;

            bool havepath0, havepath1, havepath2;
            havepath0 = havepath1 = havepath2 = true;

            while (havepath0 || havepath1 || havepath2)
            {
                neighbours0 = GetNeighbours(new_node0);
                neighbours1 = GetNeighbours(new_node1);
                neighbours2 = GetNeighbours(new_node2);

                // tree for node0
                if (neighbours0.Count == 0)
                    havepath0 = false;
                float max_distance = 0;
                bool flag = false;
                Node1 bestNeighbour = new_node0;
                foreach (Node1 neighbour in neighbours0)
                {
                    if (neighbour.occupied == false && neighbour.traversable == true)
                    {
                        flag = true;
                        float distance = Vector3.Distance(neighbour.position, node1.position) + Vector3.Distance(neighbour.position, node2.position);
                        if (max_distance <= distance)
                        {
                            max_distance = distance;
                            bestNeighbour = neighbour;
                        }
                    }
                }
                if (flag == true)
                {
                    new_node0.children.Add(bestNeighbour);
                    bestNeighbour.parent = new_node0;
                    new_node0 = bestNeighbour;
                    bestNeighbour.occupied = true;
                }
                else
                {
                    bestNeighbour = ExpandTree(start0Pos, new_node0, new_node1, new_node2);
                    if (bestNeighbour == null)
                        havepath0 = false;
                    else
                        new_node0 = bestNeighbour;
                }




                // tree for node1
                if (neighbours1.Count == 0)
                    havepath1 = false;
                max_distance = 0;
                flag = false;
                bestNeighbour = new_node1;
                foreach (Node1 neighbour in neighbours1)
                {
                    if (neighbour.occupied == false && neighbour.traversable == true)
                    {
                        flag = true;
                        float distance = Vector3.Distance(neighbour.position, new_node0.position) + Vector3.Distance(neighbour.position, new_node2.position);
                        if (max_distance <= distance)
                        {
                            max_distance = distance;
                            bestNeighbour = neighbour;
                        }
                    }
                }
                if (flag == true)
                {
                    new_node1.children.Add(bestNeighbour);
                    bestNeighbour.parent = new_node1;
                    new_node1 = bestNeighbour;
                    bestNeighbour.occupied = true;
                }
                else
                {
                    bestNeighbour = ExpandTree(start1Pos, new_node1, new_node0, new_node2);
                    if (bestNeighbour == null)
                        havepath1 = false;
                    else
                        new_node1 = bestNeighbour;
                }


                // tree for node2
                if (neighbours2.Count == 0)
                    havepath2 = false;
                max_distance = 0;
                flag = false;
                bestNeighbour = new_node2;
                foreach (Node1 neighbour in neighbours2)
                {
                    if (neighbour.occupied == false && neighbour.traversable == true)
                    {
                        flag = true;
                        float distance = Vector3.Distance(neighbour.position, new_node0.position) + Vector3.Distance(neighbour.position, new_node1.position);
                        if (max_distance <= distance)
                        {
                            max_distance = distance;
                            bestNeighbour = neighbour;
                        }
                    }
                }
                if (flag == true)
                {
                    new_node2.children.Add(bestNeighbour);
                    bestNeighbour.parent = new_node2;
                    new_node2 = bestNeighbour;
                    bestNeighbour.occupied = true;
                }
                else
                {
                    bestNeighbour = ExpandTree(start2Pos, new_node2, new_node0, new_node1);
                    if (bestNeighbour == null)
                        havepath2 = false;
                    else
                        new_node2 = bestNeighbour;
                }
            }
        }

        private int PlaceInNode1(SubNode1 currentSubNode1)
        {
            if (currentSubNode1.position.x < currentSubNode1.MegaNode1.position.x)
            {
                if (currentSubNode1.position.z < currentSubNode1.MegaNode1.position.z)
                    return 4;
                else
                    return 1;
            }
            else
            {
                if (currentSubNode1.position.z < currentSubNode1.MegaNode1.position.z)
                    return 3;
                else
                    return 2;
            }

        }

        private bool RightAvailable(SubNode1 currentSubNode1)
        {
            //int placeInNode1 = PlaceInNode1(currentSubNode1);
            Vector3 rightDirection = Quaternion.Euler(0, 90, 0) * currentSubNode1.direction;
            rightDirection = rightDirection.normalized;
            Vector3 checkPos = currentSubNode1.position + 5 * rightDirection;
            List<Node1> allNeighbourNode1s = new List<Node1>();
            if (currentSubNode1.MegaNode1.parent != null)
                allNeighbourNode1s.Add(currentSubNode1.MegaNode1.parent);
            foreach (Node1 child in currentSubNode1.MegaNode1.children)
            {
                allNeighbourNode1s.Add(child);
            }
            List<Vector3> checkPoints = new List<Vector3>();
            foreach (Node1 neighbourNode1 in allNeighbourNode1s)
            {
                checkPoints.Add(currentSubNode1.MegaNode1.position / 4 + neighbourNode1.position * 3 / 4);
                checkPoints.Add(currentSubNode1.MegaNode1.position * 3 / 4 + neighbourNode1.position / 4);
            }
            if (checkPoints.Contains(checkPos))
                return false;
            return true;
        }

        private bool LeftAvailable(SubNode1 currentSubNode1)
        {
            //int placeInNode1 = PlaceInNode1(currentSubNode1);
            Vector3 leftDirection = Quaternion.Euler(0, -90, 0) * currentSubNode1.direction;
            leftDirection = leftDirection.normalized;
            Vector3 checkPos = currentSubNode1.position + 5 * leftDirection;
            List<Node1> allNeighbourNode1s = new List<Node1>();
            if (currentSubNode1.MegaNode1.parent != null)
                allNeighbourNode1s.Add(currentSubNode1.MegaNode1.parent);
      
            foreach (Node1 child in currentSubNode1.MegaNode1.children)
            {
                allNeighbourNode1s.Add(child);
            }
            List<Vector3> checkPoints = new List<Vector3>();
            Vector3 checkPoint1, checkPoint2;
            foreach (Node1 neighbourNode1 in allNeighbourNode1s)
            {
                checkPoint1.x = currentSubNode1.MegaNode1.position.x / 4 + neighbourNode1.position.x * 3 / 4;
                checkPoint1.y = currentSubNode1.MegaNode1.position.y / 4 + neighbourNode1.position.y * 3 / 4;
                checkPoint1.z = currentSubNode1.MegaNode1.position.z / 4 + neighbourNode1.position.z * 3 / 4;
                checkPoints.Add(checkPoint1);
                checkPoint2 = currentSubNode1.MegaNode1.position * 3 / 4 + neighbourNode1.position / 4;
                checkPoints.Add(checkPoint2);
            }
            if (checkPoints.Contains(checkPos))
                return false;
            return true;
        }

        private bool StraightAvailable(SubNode1 currentSubNode1)
        {
            //int placeInNode1 = PlaceInNode1(currentSubNode1);
            Vector3 direction = currentSubNode1.direction;
            direction = direction.normalized;
            Vector3 checkPos = currentSubNode1.position + 5 * direction;
            List<Node1> allNeighbourNode1s = new List<Node1>();
            if (currentSubNode1.MegaNode1.parent != null)
                allNeighbourNode1s.Add(currentSubNode1.MegaNode1.parent);
            foreach (Node1 child in currentSubNode1.MegaNode1.children)
            {
                allNeighbourNode1s.Add(child);
            }
            List<Vector3> checkPoints = new List<Vector3>();
            foreach (Node1 neighbourNode1 in allNeighbourNode1s)
            {
                checkPoints.Add(currentSubNode1.MegaNode1.position / 4 + neighbourNode1.position * 3 / 4);
                checkPoints.Add(currentSubNode1.MegaNode1.position * 3 / 4 + neighbourNode1.position / 4);
            }
            if (checkPoints.Contains(checkPos))
                return false;
            return true;
        }

        private List<SubNode1> FindPath(SubNode1 startSub, string direction)
        {
            SubNode1 currentSubNode1 = startSub;
            Vector3 originalPos = startSub.position;
            while (currentSubNode1.occupied != true)
            {
                if (direction == "LEFT")
                {
                    //if (currentSubNode1.parent == null)
                    //{
                    //    currentSubNode1.child = GetSubNode1InMap(currentSubNode1.position + currentSubNode1.direction.normalized * 10, xSubGridSize, zSubGridSize);
                    //    currentSubNode1.child.direction = currentSubNode1.direction;
                    //    currentSubNode1.child.parent = currentSubNode1;
                    //}
                    if (LeftAvailable(currentSubNode1))
                    {
                        currentSubNode1.child = GetSubNode1InMap(currentSubNode1.position + Quaternion.Euler(0, -90, 0) * currentSubNode1.direction.normalized * 10, xSubGridSize, zSubGridSize);
                        currentSubNode1.child.direction = Quaternion.Euler(0, -90, 0) * currentSubNode1.direction;
                        currentSubNode1.child.parent = currentSubNode1;
                    }
                    else if (StraightAvailable(currentSubNode1))
                    {
                        currentSubNode1.child = GetSubNode1InMap(currentSubNode1.position + currentSubNode1.direction.normalized * 10, xSubGridSize, zSubGridSize);
                        currentSubNode1.child.direction = currentSubNode1.direction;
                        currentSubNode1.child.parent = currentSubNode1;
                    }
                    else
                    {
                        currentSubNode1.child = GetSubNode1InMap(currentSubNode1.position + Quaternion.Euler(0, 90, 0) * currentSubNode1.direction.normalized * 10, xSubGridSize, zSubGridSize);
                        currentSubNode1.child.direction = Quaternion.Euler(0, 90, 0) * currentSubNode1.direction;
                        currentSubNode1.child.parent = currentSubNode1;
                    }

                    currentSubNode1.occupied = true;
                    currentSubNode1 = currentSubNode1.child;
                }
                else if (direction == "RIGHT")
                {
                    //if (currentSubNode1.parent == null)
                    //{
                    //    currentSubNode1.child = GetSubNode1InMap(currentSubNode1.position + currentSubNode1.direction.normalized * 10, xSubGridSize, zSubGridSize);
                    //    currentSubNode1.child.direction = currentSubNode1.direction;
                    //    currentSubNode1.child.parent = currentSubNode1;
                    //}
                    if (RightAvailable(currentSubNode1))
                    {
                        currentSubNode1.child = GetSubNode1InMap(currentSubNode1.position + Quaternion.Euler(0, 90, 0) * currentSubNode1.direction.normalized * 10, xSubGridSize, zSubGridSize);
                        currentSubNode1.child.direction = Quaternion.Euler(0, 90, 0) * currentSubNode1.direction;
                        currentSubNode1.child.parent = currentSubNode1;
                    }
                    else if (StraightAvailable(currentSubNode1))
                    {
                        currentSubNode1.child = GetSubNode1InMap(currentSubNode1.position + currentSubNode1.direction.normalized * 10, xSubGridSize, zSubGridSize);
                        currentSubNode1.child.direction = currentSubNode1.direction;
                        currentSubNode1.child.parent = currentSubNode1;
                    }
                    else
                    {
                        currentSubNode1.child = GetSubNode1InMap(currentSubNode1.position + Quaternion.Euler(0, -90, 0) * currentSubNode1.direction.normalized * 10, xSubGridSize, zSubGridSize);
                        currentSubNode1.child.direction = Quaternion.Euler(0, -90, 0) * currentSubNode1.direction;
                        currentSubNode1.child.parent = currentSubNode1;
                    }

                    currentSubNode1.occupied = true;
                    currentSubNode1 = currentSubNode1.child;
                }
            }
            SubNode1 originalSubNode1 = GetSubNode1InMap(originalPos, xSubGridSize, zSubGridSize);
            List<SubNode1> path = new List<SubNode1>();
            while (originalSubNode1.child.position != originalPos)
            {
                path.Add(originalSubNode1);
                originalSubNode1 = originalSubNode1.child;
            }
            return path;
        }

        public void Waypoints(List<SubNode1> path)
        {
            // path = connectedPath;
            // ConnectPath();
            // path = connectedPath;

            waypoints = new List<SubNode1>();
            waypoints.Clear();

            float angle = Vector3.Angle(path[0].position, path[1].position);
            waypoints.Add(path[0]);
            path[0].waypoint = true;

            for (int i = 1; i < path.Count - 1; i++)
            {
                float newAngle = Vector3.Angle(path[i].position, path[i + 1].position);
                if (Mathf.Abs(newAngle - angle) >= 0)
                {
                    path[i].waypoint = true;
                    waypoints.Add(path[i]);
                }
                angle = newAngle;
            }
            path[path.Count - 1].waypoint = true;
            path[path.Count - 1].lengthToNextWaypoint = 0f;
            waypoints.Add(path[path.Count - 1]);
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                if (i > 0)
                    waypoints[i - 1].parentWaypoint = waypoints[i];

                waypoints[i].lengthToNextWaypoint = (waypoints[i + 1].position - waypoints[i].position).magnitude;
            }
            waypoints[waypoints.Count - 1].lengthToNextWaypoint = 0f;
        }

        private void Start()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();

            // note that both arrays will have holes when objects are destroyed
            // but for initial planning they should work
            friends = GameObject.FindGameObjectsWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");



            // Plan your path here
            // ...
            crashed = false;
            if (transform.name == "ArmedCar (1)")
            {

                info = terrain_manager.myInfo;
                CreateMap();
                CreateSubMap();
                Vector3 start0Pos = friends[0].transform.position;
                Vector3 start1Pos = friends[1].transform.position;
                Vector3 start2Pos = friends[2].transform.position;

                Node1 start0 = GetNode1InMap(start0Pos, xGridSize, zGridSize);
                Node1 start1 = GetNode1InMap(start1Pos, xGridSize, zGridSize);
                Node1 start2 = GetNode1InMap(start2Pos, xGridSize, zGridSize);

                while(start0 == start2)
                {
                    start2Pos.x--;
                    start2 = GetNode1InMap(start2Pos, xGridSize, zGridSize);
                }
                while (start0 == start1)
                {
                    start1Pos.z++;
                    start1 = GetNode1InMap(start1Pos, xGridSize, zGridSize);
                }
                while (start1 == start2)
                {
                    start2Pos.z--;
                    start2 = GetNode1InMap(start2Pos, xGridSize, zGridSize);
                }
                start0Pos.x = start0.position.x + 5;
                start0Pos.z = start0.position.z;
                start1Pos.x = start1.position.x + 5;
                start1Pos.z = start1.position.z;
                start2Pos.x = start2.position.x + 5;
                start2Pos.z = start2.position.z;
                SubNode1 startSub0 = GetSubNode1InMap(start0Pos, xSubGridSize, zSubGridSize);
                SubNode1 startSub1 = GetSubNode1InMap(start1Pos, xSubGridSize, zSubGridSize);
                SubNode1 startSub2 = GetSubNode1InMap(start2Pos, xSubGridSize, zSubGridSize);
                start0.occupied = true;
                start1.occupied = true;
                start2.occupied = true;
                GenerateTree(start0, start1, start2);
                startSub0.direction = Vector3.right;
                startSub1.direction = Vector3.right;
                startSub2.direction = Vector3.right;
                path0 = FindPath(startSub0, "RIGHT");
                path1 = FindPath(startSub1, "RIGHT");
                path2 = FindPath(startSub2, "RIGHT");
                Waypoints(path0);
            }
            else if (transform.name == "ArmedCar")
            {
                Waypoints(path1);
            }
            else
            {
                Waypoints(path2);
            }
            StartCoroutine(DidWeCrash());
        }

        float CalculateTurnAngle(Vector3 from, Vector3 to, float maximumSteerAngle, out float angle)
        {
            Vector3 direction = to - from;
            angle = Vector3.Angle(direction, transform.forward);
            if (Vector3.Cross(direction, transform.forward).y > 0)
            {
                angle = -angle;
            }
            return Mathf.Clamp(angle, (-1) * maximumSteerAngle, maximumSteerAngle) / maximumSteerAngle;
        }

        void AdjustSpeed(Vector3 from, SubNode1 waypoint)
        {
            // float nextDistanceLength = waypoint.lengthToNextWaypoint;
            float distance = (waypoint.position - from).magnitude;
            float velocity = m_Car.CurrentSpeed;
            float turnCounter = TurnAhead(waypoint);
            //print(turnCounter);
            if ((distance > 15f && velocity < topSpeed) || velocity < 8f)
            {
                speed = 1f;
                breakForce = 0;
            }
            else
            {
                speed = 0f;
                breakForce = 0f;
            }
        }

        float TurnAhead(SubNode1 waypoint)
        {
            float turns = 0;
            SubNode1 currentNode1 = waypoint;
            for (int i = 0; i < 8; i++)
            {
                if (currentNode1.child == null)
                    break;
                turns += Vector3.Angle(currentNode1.position, currentNode1.child.position);
                currentNode1 = currentNode1.child;
            }
            return turns;
        }

        IEnumerator DidWeCrash()
        {
            yield return new WaitForSeconds(2f);
            Vector3 myPosition = transform.position;
            //print("crash");
            while (!goalReached)
            {
                yield return new WaitForSeconds(1f);
                if ((myPosition - transform.position).magnitude < 0.5f && !crashed)
                {
                    //print("crash!");
                    crashed = true;
                    crashCounter++;
                    yield return new WaitForSeconds(1f);
                    crashed = false;
                    yield return new WaitForSeconds(2f);
                }
                else
                {
                    crashCounter = 0;
                }
                myPosition = transform.position;
            }
        }

        private void FixedUpdate()
        {


            // Execute your path here
            // ...

            Vector3 avg_pos = Vector3.zero;

            foreach (GameObject friend in friends)
            {
                avg_pos += friend.transform.position;
            }
            avg_pos = avg_pos / friends.Length;
            Vector3 direction = (avg_pos - transform.position).normalized;

            bool is_to_the_right = Vector3.Dot(direction, transform.right) > 0f;
            bool is_to_the_front = Vector3.Dot(direction, transform.forward) > 0f;

            float steering = 0f;
            float acceleration = 0;

            if (is_to_the_right && is_to_the_front)
            {
                steering = 1f;
                acceleration = 1f;
            }
            else if (is_to_the_right && !is_to_the_front)
            {
                steering = -1f;
                acceleration = -1f;
            }
            else if (!is_to_the_right && is_to_the_front)
            {
                steering = -1f;
                acceleration = 1f;
            }
            else if (!is_to_the_right && !is_to_the_front)
            {
                steering = 1f;
                acceleration = -1f;
            }

            // this is how you access information about the terrain
            int i = terrain_manager.myInfo.get_i_index(transform.position.x);
            int j = terrain_manager.myInfo.get_j_index(transform.position.z);
            float grid_center_x = terrain_manager.myInfo.get_x_pos(i);
            float grid_center_z = terrain_manager.myInfo.get_z_pos(j);

            Debug.DrawLine(transform.position, new Vector3(grid_center_x, 0f, grid_center_z));


            // this is how you control the car
            if (transform.name == "ArmedCar (1)")
            {
                if (!goalReached)
                {
                    if (iteration0 < path0.Count)
                    {
                        Debug.DrawLine(transform.position, path0[iteration0].position);
                        if (!crashed)
                        {
                            AdjustSpeed(transform.position, path0[iteration0]);
                            float steerAngle = CalculateTurnAngle(transform.position, path0[iteration0].position, m_Car.m_MaximumSteerAngle, out angle);
                            if (Mathf.Abs(angle) > 90)
                                m_Car.Move(steerAngle * -1, 0, -0.5f, 0);
                            else if (angle > 0.05f || angle < -0.05f)
                                m_Car.Move(steerAngle, speed, breakForce, 0);
                            else
                                m_Car.Move(0f, speed, breakForce, 0f);


                            if ((path0[iteration0].position - transform.position).magnitude < 5f)
                            {
                                iteration0++;
                                // print("changed waypoint");
                            }
                        }
                        else
                        {
                            float steerAngle = CalculateTurnAngle(transform.position, path0[iteration0].position, m_Car.m_MaximumSteerAngle, out angle);
                            m_Car.Move(-steerAngle, 0, -1f, 0);
                        }
                    }
                    else
                    {
                        if (m_Car.CurrentSpeed > 0.2f)
                            m_Car.Move(0f, 0f, 0f, 1f);
                        else
                        {
                            StopCoroutine(DidWeCrash());
                            goalReached = true;
                        }
                    }
                }
            }
            else if (transform.name == "ArmedCar")
            {
                if (!goalReached)
                {
                    if (iteration1 < path1.Count)
                    {
                        Debug.DrawLine(transform.position, path1[iteration1].position);
                        if (!crashed)
                        {
                            AdjustSpeed(transform.position, path1[iteration1]);
                            float steerAngle = CalculateTurnAngle(transform.position, path1[iteration1].position, m_Car.m_MaximumSteerAngle, out angle);
                            if (Mathf.Abs(angle) > 90)
                                m_Car.Move(steerAngle * -1, 0, -0.5f, 0);
                            else if (angle > 0.05f || angle < -0.05f)
                                m_Car.Move(steerAngle, speed, breakForce, 0);
                            else
                                m_Car.Move(0f, speed, breakForce, 0f);


                            if ((path1[iteration1].position - transform.position).magnitude < 5f)
                            {
                                iteration1++;
                                // print("changed waypoint");
                            }
                        }
                        else
                        {
                            //print("crash!");
                            float steerAngle = CalculateTurnAngle(transform.position, path1[iteration1].position, m_Car.m_MaximumSteerAngle, out angle);
                            m_Car.Move(-steerAngle, 0, -1f, 0);
                        }
                    }
                    else
                    {
                        if (m_Car.CurrentSpeed > 0.2f)
                            m_Car.Move(0f, 0f, 0f, 1f);
                        else
                        {
                            StopCoroutine(DidWeCrash());
                            goalReached = true;
                        }
                    }
                }
            }
            else
            {
                if (!goalReached)
                {
                    if (iteration2 < path2.Count)
                    {
                        Debug.DrawLine(transform.position, path2[iteration2].position);
                        if (!crashed)
                        {
                            AdjustSpeed(transform.position, path2[iteration2]);
                            float steerAngle = CalculateTurnAngle(transform.position, path2[iteration2].position, m_Car.m_MaximumSteerAngle, out angle);
                            if (Mathf.Abs(angle) > 90)
                                m_Car.Move(steerAngle * -1, 0, -0.5f, 0);
                            else if (angle > 0.05f || angle < -0.05f)
                                m_Car.Move(steerAngle, speed, breakForce, 0);
                            else
                                m_Car.Move(0f, speed, breakForce, 0f);


                            if ((path2[iteration2].position - transform.position).magnitude < 5f)
                            {
                                iteration2++;
                                // print("changed waypoint");
                            }
                        }
                        else
                        {
                            float steerAngle = CalculateTurnAngle(transform.position, path2[iteration2].position, m_Car.m_MaximumSteerAngle, out angle);
                            m_Car.Move(-steerAngle, 0, -1f, 0);
                        }
                    }
                    else
                    {
                        if (m_Car.CurrentSpeed > 0.2f)
                            m_Car.Move(0f, 0f, 0f, 1f);
                        else
                        {
                            StopCoroutine(DidWeCrash());
                            goalReached = true;
                        }
                    }
                }
            }
            // Debug.Log("Steering:" + steering + " Acceleration:" + acceleration);
            // m_Car.Move(steering, acceleration, acceleration, 0f);
            //m_Car.Move(0f, -1f, 1f, 0f);


        }

        void OnDrawGizmos()
        {
            // print("draw");
            if (map != null)    
            {
                foreach (Node1 node in map)
                {
                    if (node.parent != null)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(node.position, node.parent.position);
                    }
                    else
                    {
                        //Gizmos.color = Color.green;
                        //Gizmos.DrawCube(node.position, new Vector3((info.x_high - info.x_low) / xN, 0.1f, (info.z_high - info.z_low) / zN));
                    }

                }
            }

            if (submap != null)
            {
                //SubNode1 subNode1;

                foreach (SubNode1 subNode1 in submap)
                {
                    //if (waypoints.Contains(subNode1))
                    //{
                    //    Gizmos.color = Color.yellow;
                    //    Gizmos.DrawCube(subNode1.position, new Vector3((info.x_high - info.x_low) / xSubN, 0.1f, (info.z_high - info.z_low) / zSubN));
                    //}
                    //Gizmos.color = Color.grey;
                    //Gizmos.DrawCube(subNode1.position, new Vector3((info.x_high - info.x_low) / xSubN, 0.1f, (info.z_high - info.z_low) / zSubN));
                    if (subNode1.parent != null)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(subNode1.position, subNode1.parent.position);
                    }
                }
            }
        }
    }

    public class Node1
    {
        public bool traversable;
        public Vector3 position;
        public bool occupied;
        public int xGrid;
        public int zGrid;
        public Node1 parent;
        public List<Node1> children;
        public Node1(bool _traversable, bool _occupied, Vector3 _position, int x, int z)
        {
            children = new List<Node1>();
            traversable = _traversable;
            occupied = _occupied;
            position = _position;
            xGrid = x;
            zGrid = z;
        }
    }

    public class SubNode1
    {
        public bool traversable;
        public Vector3 position;
        public bool occupied;
        public int xSubGrid;
        public int zSubGrid;
        public Node1 MegaNode1;
        public Vector3 direction;
        public SubNode1 child;
        public SubNode1 parent;
        public bool waypoint = false;
        public SubNode1 parentWaypoint;
        public float lengthToNextWaypoint;
        public SubNode1(bool _traversable, bool _occupied, Vector3 _position, int x, int z)
        {
            traversable = _traversable;
            occupied = _occupied;
            position = _position;
            xSubGrid = x;
            zSubGrid = z;
        }
    }
}
