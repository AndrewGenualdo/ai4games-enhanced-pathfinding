using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIScript : MonoBehaviour // everything in the file is made by Andrew except for blocks specifically labeled as being made by Anders
{
    [SerializeField] Button resetButton;
    [SerializeField] bool mouseControllingRotation;
    [SerializeField] Button restartButton;
    [SerializeField] Button addWallButton;
    [SerializeField] Slider colorSlider;
    public LineDrawer drawer;
    [SerializeField] Toggle autoUpdateToggle;
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
    void Awake()
    {
        this.gameObject.transform.position = start.transform.position;

        startPos = transform.position;
        generatePathButton.onClick.AddListener(GeneratePathButton);
        resetButton.onClick.AddListener(Reset);

        restartButton.onClick.AddListener(restart);
        colorSlider.onValueChanged.AddListener(updateColors); // anders
    }

    private void restart()
    {
        this.gameObject.transform.position = start.transform.position;
        GetComponent<MissilePath2>().ClearPath();
    }
    private void Reset()
    {
        SceneManager.LoadScene(0);
    }
    public void GeneratePathButton()
    {
        Vector3 startPos;
        if(autoUpdateToggle.isOn) {startPos = this.transform.position; }else {startPos = start.transform.position; }

        GetComponent<MissilePath2>().GeneratePath(startPos, goal.transform.position);
        GetComponent<BoidScript>().resetOffsets();
    }

    private void updateColors(float value) // added by anders to make the color switch when you update them with the UI
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

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            mouseControllingRotation = !mouseControllingRotation;
            Cursor.visible = !mouseControllingRotation;

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

        var forward = cam.transform.forward;// added by Anders - moves the object relative to the axis that the camera is to avoid input confusion, used this unity discussion post as a resource https://discussions.unity.com/t/moving-character-relative-to-camera/614923
        var right = cam.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();


        Vector3 posDiff = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) posDiff.x += Time.deltaTime * moveSpeed;
        if (Input.GetKey(KeyCode.S)) posDiff.x -= Time.deltaTime * moveSpeed;
        if (Input.GetKey(KeyCode.D)) posDiff.z += Time.deltaTime * moveSpeed;
        if (Input.GetKey(KeyCode.A)) posDiff.z -= Time.deltaTime * moveSpeed;
        if (Input.GetKey(KeyCode.E)) posDiff.y += Time.deltaTime * moveSpeed;
        if (Input.GetKey(KeyCode.Q)) posDiff.y -= Time.deltaTime * moveSpeed;

        var desiredMoveDirection = forward * posDiff.x + right * posDiff.z;  // the math for the angle adjusted movement

        currentObj.transform.position += desiredMoveDirection; 
        Debug.Log(posDiff + ", " + desiredMoveDirection);
        if (currentObject != -2 && posDiff !=  Vector3.zero) shouldReset = true;

        Vector3 rotDiff = Vector3.zero;



        if (mouseControllingRotation) //  Anders - Removed keyboard roations inputs in favor of mouse inputs
        {
            rotDiff.y += Input.GetAxis("Horizontal") / 10;
            rotDiff.x -= Input.GetAxis("Vertical") / 10;
            if (currentObject != -2) { rotDiff.z += Input.GetAxis("Mouse ScrollWheel") * 100; }
        }
        //if (Input.GetKey(KeyCode.Z)) rotDiff.x += Time.deltaTime * moveSpeed * 5;
        //if (Input.GetKey(KeyCode.X)) rotDiff.x -= Time.deltaTime * moveSpeed * 5;
        //if (Input.GetKey(KeyCode.C)) rotDiff.y += Time.deltaTime * moveSpeed * 5;
        //if (Input.GetKey(KeyCode.V)) rotDiff.y -= Time.deltaTime * moveSpeed * 5;
        //if (Input.GetKey(KeyCode.B)) rotDiff.z += Time.deltaTime * moveSpeed * 5;
        //if (Input.GetKey(KeyCode.N)) rotDiff.z -= Time.deltaTime * moveSpeed * 5;


        currentObj.transform.rotation = Quaternion.Euler(currentObj.transform.rotation.eulerAngles + rotDiff);
        if (currentObject != -2 && rotDiff != Vector3.zero) shouldReset = true;

        TMP_Text textMeshPro = displayText.GetComponent<TMP_Text>();

        string controllingText;
        if (mouseControllingRotation) // anders - UI so you know what you're controlling with your mouse
        {
            controllingText = "Mouse Controlling Rotation";
        }
        else
        {
            controllingText = "Mouse Controlling Cursor";

        }

        if (currentObject < 0)
        {
            textMeshPro.text = "[" + (currentObject + 3) + "] " + objectNames[-(currentObject + 1)] + "\n" + controllingText;
        }
        else
        {
            textMeshPro.text = "[" + (currentObject + 3) + "] Obstacle: " + currentObject.ToString() + "\n" + controllingText;
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            obstacles.Add(Instantiate(obstaclePrefab));
            shouldReset = true;
        }
        if (shouldReset && GetComponent<MissilePath2>().GetPathLength() != 0) GeneratePathButton();
    }
}
