using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{

    [SerializeField] Button generatePathButton;

    Vector3 startPos;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        generatePathButton.onClick.AddListener(GeneratePathButton);
    }

    public void GeneratePathButton()
    {
        transform.position = startPos;
        GetComponent<MissilePath2>().GeneratePath(GetComponent<BoidScript>().goal.transform.position);
        GetComponent<BoidScript>().startTime = Time.time;
        GetComponent<BoidScript>().ResetBoids = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
