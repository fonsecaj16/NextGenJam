using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ColorChange : MonoBehaviour
{
    public Light dirLight;
    private Color White = new Color(1f, 1f, 1f); // white
    private Color Blue = new Color(0.4157f, 0.3765f, 0.7843f); // good for sea, possibly beach
    private Color Yellow = new Color(0.8431f, 0.7529f, 0.2667f); // good for forest; natural light
    private List<Color> colors = new List<Color>();
    public int i = 1;
    private float updateInterval = .5f;
    private float lastUpdateTime = 0f;

    void Start()
    {
        colors.Add(White);
        colors.Add(Blue);
        colors.Add(Yellow);
    }

    void Update()
    {
        if (dirLight == null) return;

        if (Time.time - lastUpdateTime >= updateInterval)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                dirLight.color = colors[i];
                i++;
                if (i == colors.Count)
                    i = 0;
                lastUpdateTime = Time.time;
            }
        }

    }
}
