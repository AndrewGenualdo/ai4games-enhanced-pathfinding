using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineDrawer : MonoBehaviour // everything in the file was made by Andrew, with the exception of setColor
{
    [SerializeField] Toggle lineToggle;

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
        if (!lineToggle.isOn) { return; }

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

    public void setColor(Color color) // made by anders
    {
        foreach(LineRenderer line in lines)
        {
            line.endColor = color;
            line.startColor = color;

        }

    }

}
