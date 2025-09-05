using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeBrightness : MonoBehaviour
{
    public Light dirLight;         
    [SerializeField] float step = 0.2f;
    [SerializeField] float minIntensity = 0f;
    [SerializeField] float maxIntensity = 5f;

    // Update is called once per frame
    void Update()
    {
        if (dirLight == null) return;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            dirLight.intensity = Mathf.Clamp(dirLight.intensity + step * Time.deltaTime * 10, minIntensity, maxIntensity);
        }

        // Decrease intensity with Down arrow
        if (Input.GetKey(KeyCode.DownArrow))
        {
            dirLight.intensity = Mathf.Clamp(dirLight.intensity - step * Time.deltaTime * 10, minIntensity, maxIntensity);
        }
    }
}
