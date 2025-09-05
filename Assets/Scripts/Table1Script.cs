using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table1Script : MonoBehaviour
{
    [SerializeField] private GameObject _image;

    private bool _isImageActive = false;
    private float _lastTapTime = -999f;
    public float tapDebounce = 0.25f;
    private void OnTriggerEnter(Collider other)
    {
        

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {

            if (Time.time - _lastTapTime < tapDebounce) return;
            _lastTapTime = Time.time;

            _image.SetActive(!_isImageActive);
            _isImageActive = !_isImageActive;
        }
    }


}
