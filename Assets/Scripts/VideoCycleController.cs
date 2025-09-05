using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;
using System.IO;
using UnityEngine.InputSystem;

[RequireComponent(typeof(VideoPlayer))]
public class VideoCycleController : MonoBehaviour
{
    [Header("Output")]
    public RenderTexture targetRT;
    public Renderer[] targetRenderers;

    [Header("Playlist (choose ONE: clips OR urls)")]
    public List<VideoClip> clips = new List<VideoClip>();

    [Header("Playback")]
    public bool loopEach = true;      // Loop the current video
    public float startDelay = 0f;      // Delay (seconds) after prepare before starting playback
    public bool playFirstOnStart = true; // Play the first item when enabled


    [Header("VR Tap - Trigger Collider")]
    public bool enableTriggerTap = true;   // If true, OnTriggerEnter will count as a tap
    public LayerMask triggerLayers = 0;    // Layers that can trigger a tap (set to Player layer, etc.)
    public float tapDebounce = 0.25f;      // Debounce to avoid multiple taps from one contact

    private VideoPlayer _vp;
    private int _idx = 0;
    private VideoPlayer.EventHandler _onPrepared; // Keep a reference so we can unsubscribe properly
    private float _lastTapTime = -999f;
    private bool _isFirstTap = true;

    [SerializeField] private TriggerActivator _triggerActivator;
    [SerializeField] private GameObject _plane;

    void Awake()
    {
        _vp = GetComponent<VideoPlayer>();

        // Output to a RenderTexture
        if (targetRT == null) targetRT = _vp.targetTexture; // Fallback: use the VideoPlayer's target if not assigned
        _vp.renderMode = VideoRenderMode.RenderTexture;
        _vp.targetTexture = targetRT;
        _vp.isLooping = loopEach;

        // Play only after the clip/URL is prepared (smoother switching)
        _onPrepared = (vp) =>
        {
            if (startDelay > 0f) Invoke(nameof(PlayNow), startDelay);
            else _vp.Play();
        };
        _vp.prepareCompleted += _onPrepared;

        // Bind the RT to all target renderers/material slots via MaterialPropertyBlock
        BindRTToMaterials();

        
    }

    void OnEnable()
    {
        _triggerActivator.OnTriggered += OnTriggered;
    }

    void OnDisable()
    {
        if (_onPrepared != null) _vp.prepareCompleted -= _onPrepared;
        _triggerActivator.OnTriggered -= OnTriggered;
    }

    // Public entry so XR/UI can call a "tap" (e.g., XRSimpleInteractable activated ¡ú RegisterTap())
    public void RegisterTap()
    {
        if (_isFirstTap)
        {
            _plane.SetActive(true);
            _isFirstTap = false;
            return;
        }
        NextVideo();
    }

    // VR "poke" via trigger collider
    private void OnTriggered(Collider other)
    {

        if (!enableTriggerTap) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Check layer mask
            if (((1 << other.gameObject.layer) & triggerLayers) == 0) return;

            // Debounce
            if (Time.time - _lastTapTime < tapDebounce) return;
            _lastTapTime = Time.time;

            ColorChange.CycleRequested?.Invoke();
            Debug.Log("Color Change Triggered");
            RegisterTap();
        }
    }

    void PlayNow() => _vp.Play();

    void BindRTToMaterials()
    {
        if (targetRenderers == null) return;

        foreach (var r in targetRenderers)
        {
            if (!r) continue;

            // Property name compatibility: URP/Lit uses _BaseMap; Legacy/Standard often uses _MainTex
            string texProp = (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_BaseMap")) ? "_BaseMap" : "_MainTex";
            int propId = Shader.PropertyToID(texProp);

            // Handle multiple material slots
            var mats = r.sharedMaterials;
            int count = Mathf.Max(1, mats != null ? mats.Length : 1);

            for (int i = 0; i < count; i++)
            {
                var mpb = new MaterialPropertyBlock();
                r.GetPropertyBlock(mpb, i);
                mpb.SetTexture(propId, targetRT);
                r.SetPropertyBlock(mpb, i);
            }
        }
    }

    public void NextVideo()
    {
        _idx++;

        Debug.Log("First"+_idx);
        // Stop the current one (if any)
        if (_vp.isPlaying) _vp.Stop();

        // Prefer clips over urls
        if (_idx == clips.Count)
        {
            _plane.SetActive(false);
        }else if(_idx > clips.Count)
        {
            _plane.SetActive(true);
            _idx = 0;
        }
        if (clips != null && clips.Count > 0 && _idx < clips.Count)
        {
            _vp.source = VideoSource.VideoClip;
            _vp.clip = clips[_idx];
            _vp.Prepare(); // Asynchronous; will auto-Play in prepareCompleted handler
        }
        else
        {
            Debug.LogWarning("[VideoCycleController] No video sources provided (Clips or Urls).");
        }
    }
}