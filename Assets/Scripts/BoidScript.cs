using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BoidScript : MonoBehaviour
{

    [SerializeField] GameObject BoidTemplate;
    [SerializeField] int NumBoids = 5;
    [SerializeField] float neighborhoodRadius = 5;

    [SerializeField] float cohesionStrenth = 1;

    [SerializeField] GameObject centerObject;

    List<GameObject> BoidList = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        while (BoidList.Count < NumBoids)
        { 
            BoidList.Add(Instantiate(BoidTemplate,centerObject.transform.position + Random.onUnitSphere * Random.Range(0.1f,neighborhoodRadius), Quaternion.identity));
        }

        while (BoidList.Count > NumBoids)
        {
            GameObject BoidToDelete = BoidList[0];
            BoidList.Remove(BoidToDelete);
            Destroy(BoidToDelete);

        }

        CalculateCohesion();

    }

    List<GameObject> GetNeighbors (Vector3 position,float distance)
    {
        List<GameObject> Neighborhood = new List<GameObject>();

        foreach (var boid in BoidList)
        { 
            if(Mathf.Abs((boid.transform.position - position).magnitude) <= distance) { Neighborhood.Add(boid); }
        }

        return Neighborhood;
    }

    void CalculateCohesion()
    {
        foreach (var boid in BoidList)
        {
            Vector3 force = new Vector3();
            List<GameObject> Neighborhood = GetNeighbors(boid.transform.position, neighborhoodRadius);
            foreach (var neighbor in Neighborhood)
            {
                force += neighbor.transform.position - boid.transform.position;
            }


            boid.GetComponent<Rigidbody>().AddForce(force * cohesionStrenth);
        }

    }



}
