using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissilePath2 : MonoBehaviour
{
    public static List<GameObject> obstacles;
    public static List<Vector3> path;
    public GameObject goal;

    // Start is called before the first frame update
    void Start()
    {
        if (obstacles == null) obstacles = new List<GameObject>();
        if (path == null) path = new List<Vector3>();
        if(goal != null) Pathfind(goal.transform.position);
        else Debug.Log("GOAL IS NULL!!!");
    }

    void Pathfind(Vector3 goal)
    {
        path.Clear();
        path.Add(this.transform.position);

        RaycastHit hit;

        //https://docs.unity3d.com/ScriptReference/Physics.Linecast.html
        Vector3 lastPoint = path[path.Count - 1];
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

        
        path.Add(goal);

    }

    // Update is called once per frame
    void Update()
    {
        Pathfind(goal.transform.position);
        //Pathfind(goal.transform.position);
        for(int i = 0; i < path.Count - 1; i++)
        {
            
        }
    }
}
