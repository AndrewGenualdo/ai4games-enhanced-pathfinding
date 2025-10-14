using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{

    public static List<GameObject> obstacles;
    public static List<Vector3> path;

    // Start is called before the first frame update
    void Start()
    {
        if(obstacles == null) obstacles = new List<GameObject>();
        if(path == null) path = new List<Vector3>();
    }

    void Pathfind(Vector3 goal)
    {
        path.Clear();
        path.Add(this.transform.position);

        RaycastHit hit;
        Vector3 lastPoint = path[path.Count - 1];
        //https://docs.unity3d.com/ScriptReference/Physics.Linecast.html
        /*if (Physics.Linecast(lastPoint, goal, null, hit))
        {

        }*/

        path.Add(goal);

    }

    //when adding new segment, if fails but no new obstacles were added, pathfinding has failed/is stuck

    // Update is called once per frame
    void Update()
    {
        
    }
}
