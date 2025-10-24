using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class MissilePath2 : MonoBehaviour
{
    public static List<GameObject> obstacles;
    private static List<PathNode> path;
    public static List<Vector3> finalPath;
    public GameObject goal;

    private struct PathNode
    {
        
        private Vector3 pos;
        private int parentIndex;
        public List<int> nodes;

        public PathNode(Vector3 pos, int parentIndex)
        {
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
    }

    // Start is called before the first frame update
    void Start()
    {
        if (obstacles == null) obstacles = new List<GameObject>();
        if (path == null) path = new List<PathNode>();
        if (finalPath == null) finalPath = new List<Vector3>();
        if(goal != null) Pathfind(goal.transform.position);
        else Debug.Log("GOAL IS NULL!!!");
    }

    void Pathfind(Vector3 goal)
    {
        /*finalPath.Clear();
        finalPath.Add(this.transform.position);

        RaycastHit hit;

        //https://docs.unity3d.com/ScriptReference/Physics.Linecast.html
        Vector3 lastPoint = finalPath[finalPath.Count - 1];
        if (Physics.Linecast(lastPoint, goal, out hit))
        {
            Debug.Log("Hit at: " + hit.transform.position.x + ", " + hit.transform.position.y + ", " + hit.transform.position.z);
            if(hit.transform.position == goal)
            {
                Debug.DrawLine(lastPoint, goal, Color.green);
            }
            else
            {
                Debug.DrawLine(lastPoint, hit.point, Color.yellow);
            }
            //if the raycast from the vertex, hits the object containing said vertex, skip it
            //if the raycast hits the object before the vertex, skip it
            //only compare graph paths at the very end

        }
        else
        {
            Debug.Log("Missed!");
            Debug.DrawLine(lastPoint, goal, Color.red);
        }

        
        finalPath.Add(goal);*/
        path.Clear();
        finalPath.Clear();
        path.Add(new PathNode(this.transform.position, -1)); //-1 = head
        int lastPoint = 0;
        RaycastHit hit;
        //https://docs.unity3d.com/ScriptReference/Physics.Linecast.html
        if (Physics.Linecast(path[lastPoint].GetPos(), goal, out hit))
        {
            Debug.DrawLine(path[lastPoint].GetPos(), hit.transform.position, Color.white);
            if(hit.transform.position == goal)
            {
                //done, no further actions needed
                path.Add(new PathNode(goal, lastPoint));
                path[lastPoint].AddNode(path.Count - 1);
                int parentIndex = path[path.Count - 1].GetParent();
                finalPath.Add(path[parentIndex].GetPos());
                while(parentIndex != -1)
                {
                    parentIndex = path[parentIndex].GetParent();
                    if(parentIndex != -1)
                    {
                        finalPath.Add(path[parentIndex].GetPos());
                    }
                    
                }
            }
            else
            {
                /*Bounds bounds = hit.collider.bounds;
                Vector3 boundPos = bounds.center;
                for(int i = 0; i < 8; i++)
                {
                    Vector3 boundCorner = bounds.extents;
                    if(i % 2 == 0) { boundCorner.x *= -1; } 
                    if(Mathf.Round(i / 2) % 2 == 0) { boundCorner.y *= -1; }
                    if(Mathf.Round(i / 4) % 2 == 0) { boundCorner.z *= -1; }
                }*/
                List<Vector3> vertices = new List<Vector3>();
                hit.collider.gameObject.GetComponent<MeshFilter>().mesh.GetVertices(vertices);
                vertices = vertices.Distinct().ToList();
                for(int i = 0; i < vertices.Count; i++)
                {
                    Vector3 scaledVertex = vertices[i];
                    scaledVertex.x *= hit.collider.transform.localScale.x;
                    scaledVertex.y *= hit.collider.transform.localScale.y;
                    scaledVertex.z *= hit.collider.transform.localScale.z;
                    path.Add(new PathNode(scaledVertex + hit.transform.position, lastPoint));
                    path[lastPoint].AddNode(path.Count - 1);
                }
            }
        }
        //Debug.Log(finalPath.Count);
        finalPath.Reverse();
        DrawGraph(path[0]);
    }

    void DrawGraph(PathNode node)
    {
        for(int i = 0; i < node.nodes.Count; i++)
        {
            Debug.DrawLine(node.GetPos(), path[node.nodes[i]].GetPos(), Color.green);
            DrawGraph(path[node.nodes[i]]);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        Pathfind(goal.transform.position);
        //Pathfind(goal.transform.position);
        for(int i = 0; i < finalPath.Count - 1; i++)
        {
            Debug.DrawLine(finalPath[i], finalPath[i+1], Color.green);
        }
    }
}
