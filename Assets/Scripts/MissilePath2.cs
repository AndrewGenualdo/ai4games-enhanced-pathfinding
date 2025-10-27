using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class MissilePath2 : MonoBehaviour
{
    [SerializeField] public List<PathNode> path;
    public List<Vector3> finalPath;
    private List<PathNode> endNodes;
    public int maxDepth = 3;
    public float distFromObject = 0.5f;
    LineRenderer lineRenderer;

    [System.Serializable]
    public struct PathNode
    {
        
        private Vector3 pos;
        private int parentIndex;
        public List<int> nodes;
        private float distance;

        public PathNode(Vector3 pos, int parentIndex)
        {
            this.distance = -1;
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

        public void SetDistance(float distance)
        {
            this.distance = distance;
        }
    }

    float GetPathDistance(int childNode)
    {
        float totalDist = 0;
        if (childNode == -1) return 0;
        int lastIndex = childNode;
        int parentIndex = path[lastIndex].GetParent();
        while (parentIndex != -1)
        {
            totalDist += (path[parentIndex].GetPos() - path[lastIndex].GetPos()).magnitude;
            lastIndex = parentIndex;
            parentIndex = path[parentIndex].GetParent();
        }
        return totalDist;
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (path == null) path = new List<PathNode>();
        if (finalPath == null) finalPath = new List<Vector3>();
        if (endNodes == null) endNodes = new List<PathNode>();
        drawer = gameObject.AddComponent<LineDrawer>();
    }

    public void GeneratePath(Vector3 goal)
    {
        path.Clear();
        finalPath.Clear();
        endNodes.Clear();
        path.Add(new PathNode(this.transform.position, -1)); //-1 = head
        path[path.Count - 1].SetDistance(GetPathDistance(path.Count - 1));

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
        }

        drawer.BeginFrame();
        DrawGraph(path[0]);
        
    }

    int CountLines(PathNode node)
    {
        int count = 0;
        for(int i = 0; i < node.nodes.Count; i++)
        {
            count += CountLines(path[node.nodes[i]]);
        }
        return count + node.nodes.Count;
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
                float parentDist = node.GetParent() == -1 ? 0 : path[node.GetParent()].GetDistance();
                path.Add(new PathNode(goal, nodeIndex));
                path[path.Count - 1].SetDistance(GetPathDistance(path.Count - 1));
                node.AddNode(path.Count - 1);
                endNodes.Add(node);
            }
            else
            {
                
                
                Mesh mesh = hit.collider.gameObject.GetComponent<MeshFilter>().mesh;

                //START NEW STUFF
                List<Vector3> tempVerts = new List<Vector3>();
                List<Vector3> vertices = new List<Vector3>();
                mesh.GetVertices(tempVerts);

                for(int i = 0; i + 2 < tempVerts.Count; i+=3)
                {
                    vertices.Add((tempVerts[i] + tempVerts[i + 1]) * 0.5f);
                    vertices.Add((tempVerts[i + 1] + tempVerts[i + 2]) * 0.5f);
                    vertices.Add((tempVerts[i + 2] + tempVerts[i]) * 0.5f);
                }

                //END NEW STUFF


                vertices = vertices.Distinct().ToList();
                steps++;
                for (int i = 0; i < vertices.Count; i++)
                {
                    Vector3 worldVertex = hit.collider.transform.TransformPoint(vertices[i]);
                    Vector3 center = hit.collider.bounds.center;
                    Vector3 dir = (worldVertex - center).normalized;
                    float offsetAmount = distFromObject;
                    Vector3 offsetVertex = worldVertex + dir * offsetAmount;

                    RaycastHit hit2;
                    bool valid = false;

                    valid = !Physics.Linecast(node.GetPos(), offsetVertex, out hit2);
                    if (!valid)
                    {
                        valid = hit2.collider.gameObject != hit.collider.gameObject;
                    }
                    /*if(valid)
                    {
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
                    }*/
                    float parentDist = GetPathDistance(node.GetParent());
                    float dist = parentDist + (offsetVertex - node.GetPos()).magnitude;

                    if (valid)
                    {
                        for(int j = 0; j < path.Count; j++)
                        {
                            if (path[j].GetPos() == offsetVertex)
                            {
                                if (path[j].GetDistance() < dist)
                                {
                                    valid = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (valid)
                    {
                        path.Add(new PathNode(offsetVertex, nodeIndex));
                        path[path.Count - 1].SetDistance(GetPathDistance(path.Count - 1));
                        node.AddNode(path.Count - 1);
                        Pathfind(path[path.Count - 1], path.Count - 1, goal, steps, maxSteps);
                    }
                }
            }
        }
    }

    public LineDrawer drawer;

    void DrawGraph(PathNode node)
    {
        for (int i = 0; i < node.nodes.Count; i++)
        {
            Vector3 p1 = node.GetPos();
            Vector3 p2 = path[node.nodes[i]].GetPos();
            bool isPath = false;
            for(int j = 0; j < finalPath.Count - 1; j++)
            {
                if(p1 == finalPath[j] && p2 == finalPath[j+1]) { isPath = true; break; }
            }
            if(isPath)
            {
                drawer.DrawLine(p1, p2, Color.blue, Color.yellow, 0.05f);
            }
            else
            {
                drawer.DrawLine(p1, p2, Color.red, Color.green, 0.005f);
            }
                
            DrawGraph(path[node.nodes[i]]);
        }
    }

    public Vector3 GetPathLocation(float distance)
    {
        float dist = distance;
        for(int i = 0; i < finalPath.Count - 1; i++)
        {
            
            float lineDist = (finalPath[i + 1] - finalPath[i]).magnitude;
            if(dist - lineDist < 0)
            {
                float t = dist / lineDist;
                return Vector3.Lerp(finalPath[i], finalPath[i + 1], t);
            }
            dist -= lineDist;
        }

        return finalPath[finalPath.Count - 1];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
