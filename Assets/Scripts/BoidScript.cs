using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BoidScript : MonoBehaviour
{

    [SerializeField] GameObject BoidTemplate;
    [SerializeField] bool ResetBoids = false;

    [SerializeField] int NumBoids = 5;
    [SerializeField] float neighborhoodRadius = 5;

    [SerializeField] bool cohesionEnabled = true;
    [SerializeField] float cohesionStrenth = 1;

    [SerializeField] bool separationEnabled = true;
    [SerializeField] float separationStrenth = 1;

    [SerializeField] bool alignmentEnabled = true;
    [SerializeField] float alignmentStrenth = 1;

    [SerializeField] float maximumVelocity = 1;

    [SerializeField] GameObject centerObject;

    List<GameObject> BoidList = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (ResetBoids)
        {
            foreach (GameObject boid in BoidList)
            {
                Destroy(boid);
            }
            BoidList.Clear();
            ResetBoids = false;
        }

        while (BoidList.Count < NumBoids)
        { 
            BoidList.Add(Instantiate(BoidTemplate,centerObject.transform.position + Random.onUnitSphere * Random.Range(0.1f,neighborhoodRadius/2), Quaternion.identity));
        }

        while (BoidList.Count > NumBoids)
        {
            GameObject BoidToDelete = BoidList[0];
            BoidList.Remove(BoidToDelete);
            Destroy(BoidToDelete);

        }

        if (cohesionEnabled){ CalculateCohesion(); }
        if (separationEnabled){ CalculateSepartaion(); }
        if (alignmentEnabled){ CalculateAlignment(); }

        LimitVelocity();
    }

    List<GameObject> GetNeighbors (GameObject thisBoid,float distance)
    {
        Vector3 position = thisBoid.transform.position;

        List<GameObject> Neighborhood = new List<GameObject>();

        foreach (var boid in BoidList)
        { 
            if(Mathf.Abs((boid.transform.position - position).magnitude) <= distance) { Neighborhood.Add(boid); }
        }
        Neighborhood.Remove(thisBoid);
        return Neighborhood;
    }

    void CalculateCohesion()
    {
        foreach (var boid in BoidList)
        {
            Vector3 force = Vector3.zero;
            List<GameObject> Neighborhood = GetNeighbors(boid, neighborhoodRadius);
            foreach (var neighbor in Neighborhood)
            {
                force += neighbor.transform.position;
            }
            force /= Neighborhood.Count;

            force = force - boid.transform.position;

            boid.GetComponent<Rigidbody>().AddForce(force * cohesionStrenth);
        }
    }

    void CalculateSepartaion()
    {
        foreach (var boid in BoidList)
        {
            Vector3 force = Vector3.zero;
            List<GameObject> Neighborhood = GetNeighbors(boid, neighborhoodRadius);
            foreach (var neighbor in Neighborhood)
            {
                Vector3 delta = (boid.transform.position - neighbor.transform.position);
               // Debug.Log("delta: " + delta);

                Vector3 hat = delta.normalized;
              //  Debug.Log("hat: " + hat);

                float mag = delta.magnitude;
              //  Debug.Log("mag: " + mag);

                force += (hat / mag);
            }
          //  Debug.Log("force: " + force);

            boid.GetComponent<Rigidbody>().AddForce(force * separationStrenth);
        }
    }

    void CalculateAlignment()
    {
        foreach (var boid in BoidList)
        {
            Vector3 force = Vector3.zero;
            List<GameObject> Neighborhood = GetNeighbors(boid, neighborhoodRadius);
            foreach (var neighbor in Neighborhood)
            {
                force += neighbor.GetComponent<Rigidbody>().velocity;
            }
            if (Neighborhood.Count != 0) { force /= Neighborhood.Count + 1; }

            boid.GetComponent<Rigidbody>().AddForce(force * alignmentStrenth);
        }
    }

    void LimitVelocity()
    {
        foreach(var boid in BoidList)
        {
            Vector3 velocity = boid.GetComponent<Rigidbody>().velocity;
            if (velocity.magnitude > maximumVelocity)
            {
                velocity = velocity.normalized * maximumVelocity;
                Debug.Log("clamping velocity");
            }
        }
    }

}
