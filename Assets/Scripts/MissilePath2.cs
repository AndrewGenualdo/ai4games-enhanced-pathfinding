using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class MissilePath2 : MonoBehaviour
{
    private static List<PathNode> path;
    public static List<Vector3> finalPath;
    private static List<PathNode> endNodes;
    public GameObject goal;
    public int maxDepth = 3;

    private struct PathNode
    {
        
        private Vector3 pos;
        private int parentIndex;
        public List<int> nodes;
        private float distance;

        public PathNode(Vector3 pos, int parentIndex, float distance)
        {
            if(parentIndex != -1)
            {
                this.distance = path[parentIndex].distance + distance;
            }
            else
            {
                this.distance = 0;
            }
            this.pos = pos;
            this.parentIndex = parentIndex;
            this.nodes = new List<int>();
        }

        public void AddNode(int nodeIndex)
        {
            nodes.Add(nodeIndex);
        }

        public void RemoveNode(int nodeIndex)
        {
            nodes.Remove(nodeIndex);
        }

        public int GetParent()
        {
            return parentIndex;
        }

        public Vector3 GetPos()
        {
            return pos;
        }

        public float GetDistance()
        {
            return distance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (path == null) path = new List<PathNode>();
        if (finalPath == null) finalPath = new List<Vector3>();
        if (endNodes == null) endNodes = new List<PathNode>();
        if(goal != null) Pathfind(goal.transform.position);
        else Debug.Log("GOAL IS NULL!!!");
    }

    void Pathfind(Vector3 goal)
    {
        path.Clear();
        finalPath.Clear();
        endNodes.Clear();
        path.Add(new PathNode(this.transform.position, -1, 0)); //-1 = head
        

        Pathfind(path[0], 0, goal, 0, maxDepth);
        if(endNodes.Count > 0)
        {
            float shortestPath = endNodes[0].GetDistance();
            PathNode shortest = endNodes[0];
            for (int i = 1; i < endNodes.Count; i++)
            {
                if (endNodes[i].GetDistance() < shortestPath)
                {
                    shortestPath = endNodes[i].GetDistance();
                    shortest = endNodes[i];
                }
            }

            finalPath.Add(shortest.GetPos());
            int parentIndex = shortest.GetParent();
            if(parentIndex != -1) finalPath.Add(path[parentIndex].GetPos());
            while (parentIndex != -1)
            {
                parentIndex = path[parentIndex].GetParent();
                if (parentIndex != -1)
                {
                    finalPath.Add(path[parentIndex].GetPos());
                }
            }

            finalPath.Reverse();
            finalPath.Add(goal);
            DrawGraph(path[0]);
        }
        

        
    }

    void Pathfind(PathNode node, int nodeIndex, Vector3 goal, int steps, int maxSteps)
    {
        if (steps >= maxSteps) return;
        
        RaycastHit hit;
        //https://docs.unity3d.com/ScriptReference/Physics.Linecast.html
        if (Physics.Linecast(node.GetPos(), goal, out hit))
        {
            

            if (hit.transform.position == goal)
            {
                //done, no further actions needed
                path.Add(new PathNode(goal, nodeIndex, (goal - node.GetPos()).magnitude));
                node.AddNode(path.Count - 1);
                endNodes.Add(node);
            }
            else
            {
                List<Vector3> vertices = new List<Vector3>();
                hit.collider.gameObject.GetComponent<MeshFilter>().mesh.GetVertices(vertices);
                vertices = vertices.Distinct().ToList();
                steps++;
                for (int i = 0; i < vertices.Count; i++)
                {
                    Vector3 worldVertex = hit.collider.transform.TransformPoint(vertices[i]);
                    Vector3 center = hit.collider.bounds.center;
                    Vector3 dir = (worldVertex - center).normalized;
                    float offsetAmount = (worldVertex - center).magnitude * 0.2f;
                    Vector3 offsetVertex = worldVertex + dir * offsetAmount;

                    RaycastHit hit2;
                    bool valid = false;

                    valid = !Physics.Linecast(node.GetPos(), offsetVertex, out hit2);
                    if (!valid)
                    {
                        valid = hit2.collider.gameObject != hit.collider.gameObject;
                    }
                    int parentIndex = node.GetParent();
                    while (parentIndex != -1)
                    {
                        if (path[parentIndex].GetPos() == offsetVertex)
                        {
                            valid = false;
                            break;
                        }
                        parentIndex = path[parentIndex].GetParent();
                    }
                    if (valid)
                    {
                        path.Add(new PathNode(offsetVertex, nodeIndex, (offsetVertex - node.GetPos()).magnitude));
                        node.AddNode(path.Count - 1);
                        Pathfind(path[path.Count - 1], path.Count - 1, goal, steps, maxSteps);
                    }
                }
            }
        }
    }

    void DrawGraph(PathNode node)
    {
        for(int i = 0; i < node.nodes.Count; i++)
        {
            DrawArrow(node.GetPos(), path[node.nodes[i]].GetPos(), Color.green);
            DrawGraph(path[node.nodes[i]]);
        }
        
    }

    void DrawArrow(Vector3 start, Vector3 end, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 10f)
    {
        Debug.DrawLine(start, end, color);
        Vector3 direction = end - start;
        if (direction == Vector3.zero) return;

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;

        Debug.DrawRay(end, right * arrowHeadLength, color);
        Debug.DrawRay(end, left * arrowHeadLength, color);
    }

    // Update is called once per frame
    void Update()
    {
        Pathfind(goal.transform.position);
        //Pathfind(goal.transform.position);
        for(int i = 0; i < finalPath.Count - 1; i++)
        {
            DrawArrow(finalPath[i], finalPath[i+1], Color.yellow);
        }
    }
}
