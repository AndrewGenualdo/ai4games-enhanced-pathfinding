using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    [SerializeField] Button resetButton;

    [SerializeField] Button restartButton;
    [SerializeField] Button addWallButton;
    [SerializeField] Slider colorSlider;
    public LineDrawer drawer;

    [SerializeField] Button generatePathButton;
    [SerializeField] GameObject cam;
    [SerializeField] GameObject goal;
    [SerializeField] GameObject start;
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
        resetButton.onClick.AddListener(Reset);

        restartButton.onClick.AddListener(restart);
        colorSlider.onValueChanged.AddListener(updateColors);
    }

    private void AddWall()
    {
    }
    private void restart()
    {
        this.gameObject.transform.position = start.transform.position;
        GeneratePathButton();
    }
    private void Reset()
    {
        SceneManager.LoadScene(0);
    }
    public void GeneratePathButton()
    {
        GetComponent<MissilePath2>().GeneratePath(goal.transform.position);
        GetComponent<BoidScript>().startTime = Time.time;
        GetComponent<BoidScript>().ResetBoids = true;
    }

    private void updateColors(float value)
    {
        colorSlider.GetComponentInChildren<Image>().color = Color.HSVToRGB(value, 1, 1);
        drawer.setColor(Color.HSVToRGB(value, 1, 1));
    }


    int currentObject = -3;
    private string[] objectNames = { "Goal", "Camera" , "Start"};

    // Update is called once per frame
    void Update()
    {
        bool shouldReset = false;
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
           // shouldReset = true;
            currentObject++;
        }
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
           // shouldReset = true;
            currentObject--;
        }
        if (currentObject < -3) currentObject = -3;
        if(currentObject >= obstacles.Count) currentObject = obstacles.Count - 1;

        GameObject currentObj = cam;
        if(currentObject < 0)
        {
            switch(currentObject)
            {
                case -3: currentObj = start; break;
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
        if (Input.GetKey(KeyCode.X)) rotDiff.x -= Time.deltaTime * moveSpeed * 5;
        if (Input.GetKey(KeyCode.C)) rotDiff.y += Time.deltaTime * moveSpeed * 5;
        if (Input.GetKey(KeyCode.V)) rotDiff.y -= Time.deltaTime * moveSpeed * 5;
        if (Input.GetKey(KeyCode.B)) rotDiff.z += Time.deltaTime * moveSpeed * 5;
        if (Input.GetKey(KeyCode.N)) rotDiff.z -= Time.deltaTime * moveSpeed * 5;
        currentObj.transform.rotation = Quaternion.Euler(currentObj.transform.rotation.eulerAngles + rotDiff);
        if (currentObject != -2 && rotDiff != Vector3.zero) shouldReset = true;

        TMP_Text textMeshPro = displayText.GetComponent<TMP_Text>();
        if(currentObject < 0)
        {
            textMeshPro.text = "[" + (currentObject + 3) + "] " + objectNames[-(currentObject + 1)];
        }
        else
        {
             textMeshPro.text = "[" + (currentObject + 3) + "] Obstacle: " + currentObject.ToString();
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            obstacles.Add(Instantiate(obstaclePrefab));
            shouldReset = true;
        }
        if (shouldReset) GeneratePathButton();
    }
}
