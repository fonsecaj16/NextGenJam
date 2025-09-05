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

    private int _tapCount = 0;
    private float _lastTapTime = -999f;
    public float tapDebounce = 0.15f;
    private const float DOUBLE_TAP_TIME = 0.8f;

    private Coroutine _tapRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        // Debounce to avoid spam
        if (Time.time - _lastTapTime < tapDebounce) return;
        _lastTapTime = Time.time;

        _tapCount++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        if (_tapCount == 1)
        {
            // Start waiting for possible double tap
            if (_tapRoutine != null) StopCoroutine(_tapRoutine);
            _tapRoutine = StartCoroutine(SingleOrDoubleTap());
        }
    }

    private IEnumerator SingleOrDoubleTap()
    {
        float timer = 0f;

        while (timer < DOUBLE_TAP_TIME)
        {
            if (_tapCount == 2)
            {
                HandleDoubleTap();
                ResetTap();
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // No second tap → it's a single
        HandleSingleTap();
        ResetTap();
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
        if (_tapRoutine != null)
        {
            StopCoroutine(_tapRoutine);
            _tapRoutine = null;
        }
    }

    private void ChangeState(ScreenState screenState)
    {
        currentState = screenState;

        for (int i = 0; i < Screens.Count; i++)
            Screens[i].SetActive(false);

        if ((int)screenState < Screens.Count)
            Screens[(int)screenState].SetActive(true);

        OnStateChanged?.Invoke(currentState);
    }
}
