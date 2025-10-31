using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BoidScript : MonoBehaviour
{

    [SerializeField] GameObject BoidTemplate;
    [SerializeField] public bool ResetBoids = false;

    [SerializeField] int NumBoids = 5;
    [SerializeField] float neighborhoodRadius = 5;

    [SerializeField] bool cohesionEnabled = true;
    [SerializeField] float cohesionStrenth = 1;

    [SerializeField] bool separationEnabled = true;
    [SerializeField] float separationStrenth = 1;

    [SerializeField] bool alignmentEnabled = true;
    [SerializeField] float alignmentStrenth = 1;

    [SerializeField] bool centerAttractionEnabled = true;
    [SerializeField] float centerAttractionStrenth = 1;

    [SerializeField] float maximumVelocity = 1;

    [SerializeField] bool randomForceEnabled = true;
    [SerializeField] float randomForceStrenth = 1;

    [SerializeField] public GameObject centerObject;

    [SerializeField] public GameObject goal;

    [SerializeField] public GameObject markerObject;

    [SerializeField] float speed = 1;

    [SerializeField] float smoothing = 1;


    List<GameObject> BoidList = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
       centerObject.GetComponent<MissilePath2>().GeneratePath(goal.transform.position);
       
    }

    public float startTime = 0;

    // Update is called once per frame
    void Update()
    {
        float leng = centerObject.GetComponent<MissilePath2>().GetPathLength();
        float dist = (Time.time - startTime) * speed;

        Vector3 closeLoc =  centerObject.GetComponent<MissilePath2>().GetPathLocation(dist);
        Vector3 farLoc = centerObject.GetComponent<MissilePath2>().GetPathLocation(leng/smoothing + dist);

        centerObject.transform.position = closeLoc;

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
            BoidList.Add(Instantiate(BoidTemplate, centerObject.transform.position + Random.onUnitSphere * Random.Range(0, neighborhoodRadius / 2), Quaternion.identity));
            BoidList[BoidList.Count - 1].GetComponent<Rigidbody>().velocity = Random.onUnitSphere * Random.Range(0, maximumVelocity);
        }

        while (BoidList.Count > NumBoids)
        {
            GameObject BoidToDelete = BoidList[0];
            BoidList.Remove(BoidToDelete);
            Destroy(BoidToDelete);
        }
        foreach (var boid in BoidList)
        { 
            if (cohesionEnabled) { CalculateCohesion(boid); }
            if (separationEnabled) { CalculateSepartaion(boid); }
            if (alignmentEnabled) { CalculateAlignment(boid); }
            if (centerAttractionEnabled) { CalculateCenterAttraction(boid); }
            if (randomForceEnabled) { AddRandomForce(boid); }
            LimitVelocity(boid);
            PointTowardsVelocity(boid);
        }
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

    void CalculateCohesion(GameObject boid)
    {
 
            Vector3 force = boid.transform.position;
            List<GameObject> Neighborhood = GetNeighbors(boid, neighborhoodRadius);
            foreach (var neighbor in Neighborhood)
            {
                force += neighbor.transform.position;
            }
            force /= Neighborhood.Count +1;

       // if ((int)Time.time % 2 == 0) 
      //  { Instantiate(markerObject, force, Quaternion.identity); }

            force = force - boid.transform.position;

            boid.GetComponent<Rigidbody>().AddForce(force * cohesionStrenth);
        
    }

    void CalculateSepartaion(GameObject boid)
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

    void CalculateAlignment(GameObject boid)
    {


            Vector3 force = Vector3.zero;
            List<GameObject> Neighborhood = GetNeighbors(boid, neighborhoodRadius);
            foreach (var neighbor in Neighborhood)
            {
                force += neighbor.GetComponent<Rigidbody>().velocity;
            }
            if (Neighborhood.Count != 0) { force /= (Neighborhood.Count + 1); }

            boid.GetComponent<Rigidbody>().AddForce(-force * alignmentStrenth);
        
    }

    void CalculateCenterAttraction(GameObject boid)
    {
     //   Debug.Log("caclulating center attraction");


            Vector3 force = Vector3.zero;

            force = centerObject.transform.position - boid.transform.position;

            boid.GetComponent<Rigidbody>().AddForce(force * centerAttractionStrenth);
        
    }

    void AddRandomForce(GameObject boid)
    {

            Vector3 force = Vector3.zero;

            force = Random.onUnitSphere;

            boid.GetComponent<Rigidbody>().AddForce(force * randomForceStrenth);
        
    }

    void LimitVelocity(GameObject boid)
    {

            Vector3 velocity = boid.GetComponent<Rigidbody>().velocity;
            if (velocity.magnitude > maximumVelocity)
            {
                boid.GetComponent<Rigidbody>().velocity = velocity.normalized * maximumVelocity;
                //Debug.Log("clamping velocity");
            }
        
    }

    void PointTowardsVelocity(GameObject boid)
    {
            boid.GetComponent<Transform>().transform.LookAt(boid.GetComponent<Rigidbody>().velocity + boid.transform.position);
    }

}
