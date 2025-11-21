using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MissilePath2 : MonoBehaviour // everything in the file was implemented by Andrew, though there were contributions to the logic of the pathfinding by Anders as well
{   
    [SerializeField] Slider colorSlider;

    [SerializeField] public List<PathNode> path;


    public List<Vector3> finalPath;
    private List<int> endNodes;
    public int maxDepth = 3;
    public LineDrawer drawer;
    [SerializeField] Toggle lineToggle;
    [SerializeField] Slider distFromObjSlider;

    [System.Serializable]
    public class PathNode
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
    void lineChanged(bool val)
    {
        if (val)
        {
            DrawGraph(path[0]);
        }
        else
        {
            drawer.BeginFrame();
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
        lineToggle.onValueChanged.AddListener(lineChanged);
        if (path == null) path = new List<PathNode>();
        if (finalPath == null) finalPath = new List<Vector3>();
        if (endNodes == null) endNodes = new List<int>();
       // drawer = gameObject.AddComponent<LineDrawer>();
    }

    public void ClearPath()
    {
        path.Clear();
        finalPath.Clear();
        endNodes.Clear();
        drawer.BeginFrame();
    }

    public void GeneratePath(Vector3 start, Vector3 goal)
    {
        int hitForward = 0;
        int hitBack = 0;

        Vector3 objVec = goal - start;

        foreach (RaycastHit hit in Physics.RaycastAll(goal, -objVec, objVec.magnitude))
        {
          //  Debug.Log("Backward: " + hit.point);
            hitBack++;
        }
        foreach(RaycastHit hit in Physics.RaycastAll(start, objVec, objVec.magnitude))
        {
          //  Debug.Log("Forward: " + hit.point);
            hitForward++;
        }

        if (hitForward != hitBack){
            Debug.Log("inequal casts"); return; 
        }


        path.Clear();
        finalPath.Clear();
        endNodes.Clear();
        drawer.BeginFrame();
        path.Add(new PathNode(start, -1)); //-1 = head
        path[path.Count - 1].SetDistance(GetPathDistance(path.Count - 1));

        Pathfind(path[0], 0, goal, 0, maxDepth);
        if (endNodes.Count > 0)
        {
            float shortestPath = path[endNodes[0]].GetDistance();
            PathNode shortest = path[endNodes[0]];
            for (int i = 1; i < endNodes.Count; i++)
            {
                if (path[endNodes[i]].GetDistance() < shortestPath)
                {
                    shortestPath = path[endNodes[i]].GetDistance();
                    shortest = path[endNodes[i]];
                }
            }

            finalPath.Add(shortest.GetPos());
            int parentIndex = shortest.GetParent();
            if (parentIndex != -1) finalPath.Add(path[parentIndex].GetPos());
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

    void Pathfind(PathNode node, int nodeIndex, Vector3 goal, int steps, int maxSteps)
    {
        if (steps >= maxSteps) { Debug.Log("max steps"); return; }

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
                endNodes.Add(path.Count - 1);
            }
            else
            {

                MeshFilter mf = hit.collider.GetComponent<MeshFilter>();
                if (!mf || !mf.mesh) return;
                Mesh mesh = mf.mesh;


                List<Vector3> tempVerts = new List<Vector3>();
                mesh.GetVertices(tempVerts);
                int[] tris = mesh.triangles;
                List<Vector3> vertices = new List<Vector3>();
                for (int i = 0; i < tris.Length; i += 3)
                {
                    Vector3 a = tempVerts[tris[i]];
                    Vector3 b = tempVerts[tris[i + 1]];
                    Vector3 c = tempVerts[tris[i + 2]];

                    vertices.Add((a + b) * 0.5f);
                    vertices.Add((b + c) * 0.5f);
                    vertices.Add((c + a) * 0.5f);
                }


                vertices = vertices.Distinct().ToList();
                steps++;
                for (int i = 0; i < vertices.Count; i++)
                {
                    Vector3 worldVertex = hit.collider.transform.TransformPoint(vertices[i]);
                    Vector3 center = hit.collider.bounds.center;
                    Vector3 dir = (worldVertex - center).normalized;
                    float offsetAmount = distFromObjSlider.value;
                    Vector3 offsetVertex = worldVertex + dir * offsetAmount;

                    RaycastHit hit2;
                    bool valid = false;

                    valid = !Physics.Linecast(node.GetPos(), offsetVertex, out hit2);
                    if (!valid)
                    {
                        valid = hit2.collider.gameObject != hit.collider.gameObject;
                    }
                    if (valid)
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
                    }
                    float parentDist = GetPathDistance(node.GetParent());
                    float dist = parentDist + (offsetVertex - node.GetPos()).magnitude;

                    if (valid)
                    {
                        for (int j = 0; j < path.Count; j++)
                        {
                            if ((path[j].GetPos() - offsetVertex).sqrMagnitude < 0.0001f) //for floating point erors
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

    void DrawGraph(PathNode node)
    {
        DrawGraph(node, 0);
    }

    void DrawGraph(PathNode node, int count)
    {
        for (int i = 0; i < node.nodes.Count; i++)
        {
            Vector3 p1 = node.GetPos();
            Vector3 p2 = path[node.nodes[i]].GetPos();
            bool isPath = false;
            for (int j = 0; j < finalPath.Count - 1; j++)
            {
                if ((p1 - finalPath[j]).sqrMagnitude < 0.0001f && (p2 - finalPath[j + 1]).sqrMagnitude < 0.0001f) { isPath = true; break; }
            }
            if (isPath)
            {
                drawer.DrawLine(p1, p2,Color.HSVToRGB(colorSlider.value, 1,1), Color.HSVToRGB(colorSlider.value, 1, 1), 0.05f);
            }
            else
            {
                drawer.DrawLine(p1, p2, Color.red, Color.green, 0.005f);
            }
            count++;
            if (count > 500) return;
            DrawGraph(path[node.nodes[i]]);
        }
    }

    public Vector3 GetPathLocation(float distance)
    {

        if(distance < 0)
        {
            Debug.Log("WHAT THE FUCK");
            return Vector3.zero;
        }

        float dist = distance;
        for (int i = 0; i < finalPath.Count - 1; i++)
        {

            float lineDist = (finalPath[i + 1] - finalPath[i]).magnitude;
            if (dist - lineDist < 0)
            {
                float t = dist / lineDist;
                return Vector3.Lerp(finalPath[i], finalPath[i + 1], t);
            }
            dist -= lineDist;
        }

        return finalPath.Count == 0 ? Vector3.zero : finalPath[finalPath.Count - 1];
    }

    public float GetPathLength()
    {
        float length = 0;
        for (int i = 0; i < finalPath.Count - 1; i++)
        {
            length += (finalPath[i] - finalPath[i + 1]).magnitude;
        }
        return length;
    }

    // Update is called once per frame
    void Update()
    {

    }
}