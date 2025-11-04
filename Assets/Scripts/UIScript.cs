using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{

    [SerializeField] Button generatePathButton;
    [SerializeField] GameObject cam;
    [SerializeField] GameObject goal;
    [SerializeField] GameObject displayText;
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] float moveSpeed = 10.0f;
    Vector3 startPos;
    List<GameObject> obstacles = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        generatePathButton.onClick.AddListener(GeneratePathButton);
    }

    public void GeneratePathButton()
    {
        GetComponent<MissilePath2>().GeneratePath(goal.transform.position);
        GetComponent<BoidScript>().startTime = Time.time;
        GetComponent<BoidScript>().ResetBoids = true;
    }

    int currentObject = -3;
    private string[] objectNames = { "Goal", "Camera" };

    // Update is called once per frame
    void Update()
    {
        bool shouldReset = false;
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            shouldReset = true;
            currentObject++;
        }
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            shouldReset = true;
            currentObject--;
        }
        if (currentObject < -2) currentObject = -2;
        if(currentObject >= obstacles.Count) currentObject = obstacles.Count - 1;

        GameObject currentObj = cam;
        if(currentObject < 0)
        {
            switch(currentObject)
            {
                case -2: currentObj = cam; break;
                case -1: currentObj = goal; break;
            }
        } else
        {
            currentObj = obstacles[currentObject];
        }
        Vector3 posDiff = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) posDiff.z += Time.deltaTime * moveSpeed;
        if (Input.GetKey(KeyCode.S)) posDiff.z -= Time.deltaTime * moveSpeed;
        if (Input.GetKey(KeyCode.D)) posDiff.x += Time.deltaTime * moveSpeed;
        if (Input.GetKey(KeyCode.A)) posDiff.x -= Time.deltaTime * moveSpeed;
        if (Input.GetKey(KeyCode.E)) posDiff.y += Time.deltaTime * moveSpeed;
        if (Input.GetKey(KeyCode.Q)) posDiff.y -= Time.deltaTime * moveSpeed;
        currentObj.transform.position += posDiff;
        if(currentObject != -2 && posDiff !=  Vector3.zero) shouldReset = true;

        Vector3 rotDiff = Vector3.zero;
        if (Input.GetKey(KeyCode.Z)) rotDiff.x += Time.deltaTime * moveSpeed * 5;
        if (Input.GetKey(KeyCode.X)) rotDiff.y += Time.deltaTime * moveSpeed * 5;
        if (Input.GetKey(KeyCode.C)) rotDiff.z += Time.deltaTime * moveSpeed * 5;
        currentObj.transform.rotation = Quaternion.Euler(currentObj.transform.rotation.eulerAngles + rotDiff);
        if (currentObject != -2 && rotDiff != Vector3.zero) shouldReset = true;

        TMP_Text textMeshPro = displayText.GetComponent<TMP_Text>();
        if(currentObject < 0)
        {
            textMeshPro.text = "[" + (currentObject + 2) + "] " + objectNames[-(currentObject + 1)];
        }
        else
        {
             textMeshPro.text = "[" + (currentObject + 2) + "] Obstacle: " + currentObject.ToString();
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            obstacles.Add(Instantiate(obstaclePrefab));
            shouldReset = true;
        }
        if (shouldReset) GeneratePathButton();
    }
}
