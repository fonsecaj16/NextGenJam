using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ColorChange : MonoBehaviour
{
    public Light dirLight;
    private Color White = new Color(1f, 1f, 1f); // white
    private Color Purple = new Color(0.835f, 0.784f, 1.000f); // good for sea, possibly beach
    private Color Yellow = new Color(0.996f, 0.996f, 0.741f); // good for forest; natural light
    private Color Blue = new Color(0.486f, 0.522f, 0.871f);
    private List<Color> colors = new List<Color>();
    public int i = 1;
    private float updateInterval = .5f; // cap color change to max once per this interval, else it flickers on hold
    private float lastUpdateTime = 0f;

    public static Action CycleRequested;

    void Start()
    {
        // this is the order of the colors, perhaps the ground scenes can match this order
        colors.Add(White);
        colors.Add(Yellow);
        colors.Add(Blue);
        colors.Add(Purple);
        colors.Add(White);

    }

    private void OnEnable()
    {
        ColorChange.CycleRequested += Cycle;
    }

    private void OnDisable()
    {
        ColorChange.CycleRequested -= Cycle;
    }

    private void Cycle()
    {
        if (dirLight == null) return;
        dirLight.color = colors[i];
        Debug.Log("Color" + colors[i]);
        i++;
        if (i == colors.Count)
           i = 0;
        lastUpdateTime = Time.time;
    }

    void Update()
    {
        if (dirLight == null) return;

        if (Time.time - lastUpdateTime >= updateInterval)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                dirLight.color = colors[i];
                Debug.Log("Color" + colors[i]);
                i++;
                if (i == colors.Count)
                    i = 0;
                lastUpdateTime = Time.time;
            }
        }

    }
}
