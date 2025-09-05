using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenController : MonoBehaviour
{
    public enum ScreenState
    {
        Off,
        On,
        Passthrough
    }

    public Action<ScreenState> OnStateChanged;

    [SerializeField] private ScreenState currentState = ScreenState.Off;
    [SerializeField] private List<GameObject> Screens;

    private float _tapTimer = 0f;
    private const float DOUBLE_TAP_TIME = 0.8f;
    private int _tapCount = 0;

    void Update()
    {
        // Countdown timer if a tap started
        if (_tapCount > 0)
        {
            _tapTimer += Time.deltaTime;

            if (_tapTimer > DOUBLE_TAP_TIME)
            {
                // Time expired → treat it as a single tap
                HandleSingleTap();
                ResetTap();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _tapCount++;
            if (_tapCount == 1)
            {
                _tapTimer = 0f; // start waiting for possible double tap
            }
            else if (_tapCount == 2 && _tapTimer <= DOUBLE_TAP_TIME)
            {
                HandleDoubleTap();
                ResetTap();
            }
        }
    }

    private void HandleSingleTap()
    {
        // Toggle On <-> Off
        if (currentState == ScreenState.Off)
        {
            ChangeState(ScreenState.On);
        }
        else if (currentState == ScreenState.On)
        {
            ChangeState(ScreenState.Off);
        }
        else if (currentState == ScreenState.Passthrough)
        {
            ChangeState(ScreenState.On); // passthrough → video on
        }
    }

    private void HandleDoubleTap()
    {
        // Toggle Passthrough <-> Off
        if (currentState == ScreenState.Passthrough)
        {
            ChangeState(ScreenState.Off);
        }
        else
        {
            ChangeState(ScreenState.Passthrough);
        }
    }

    private void ResetTap()
    {
        _tapCount = 0;
        _tapTimer = 0f;
    }

    private void ChangeState(ScreenState screenState)
    {
        currentState = screenState;

        for (int i = 0; i < Screens.Count; i++)
        {
            Screens[i].SetActive(i == (int)screenState);
        }

        OnStateChanged?.Invoke(currentState);

        Debug.Log("State changed to: " + currentState);
    }
}
