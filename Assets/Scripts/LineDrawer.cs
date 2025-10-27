using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{

    public Material lineMaterial;
    private List<LineRenderer> lines = new List<LineRenderer>();
    private int currentLine = 0;

    private void Awake()
    {
        if (lineMaterial == null)
        {
            lineMaterial = new Material(Shader.Find("Sprites/Default"));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void BeginFrame()
    {
        currentLine = 0;
        for (int i = 0; i < lines.Count; i++)
        {
            Destroy(lines[i].gameObject);
        }
        lines.Clear();
    }

    public void DrawLine(Vector3 start, Vector3 end, Color startColor, Color endColor, float width = 0.02f)
    {
        LineRenderer line;
        if (currentLine < lines.Count)
        {
            line = lines[currentLine];
        }
        else
        {
            GameObject go = new GameObject("Line " + currentLine);
            go.transform.parent = transform;
            line = go.AddComponent<LineRenderer>();
            line.material = lineMaterial;
            line.positionCount = 2;
            lines.Add(line);
        }

        line.startWidth = width;
        line.endWidth = width;
        line.startColor = startColor;
        line.endColor = endColor;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.enabled = true;

        currentLine++;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
