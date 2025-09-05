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
    public List<string> urls = new List<string>(); // Absolute paths, http(s), or relative paths under StreamingAssets
    public bool urlsAreInStreamingAssets = true;   // If true, resolve URLs relative to StreamingAssets

    [Header("Playback")]
    public bool loopEach = true;      // Loop the current video
    public float startDelay = 0f;      // Delay (seconds) after prepare before starting playback
    public bool playFirstOnStart = true; // Play the first item when enabled

    [Header("Input (Old Input Manager)")]
    public KeyCode nextKey = KeyCode.Q; // Key to switch to the next video

    [Header("VR Tap - Trigger Collider")]
    public bool enableTriggerTap = true;   // If true, OnTriggerEnter will count as a tap
    public LayerMask triggerLayers = 0;    // Layers that can trigger a tap (set to Player layer, etc.)
    public float tapDebounce = 0.25f;      // Debounce to avoid multiple taps from one contact

    private VideoPlayer _vp;
    private int _idx = -1;
    private VideoPlayer.EventHandler _onPrepared; // Keep a reference so we can unsubscribe properly
    private float _lastTapTime = -999f;

    void Awake()
    {
        _vp = GetComponent<VideoPlayer>();

        // (Optional) Audio via AudioSource:
        // var audio = GetComponent<AudioSource>();
        // _vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        // _vp.EnableAudioTrack(0, true);
        // _vp.SetTargetAudioSource(0, audio);

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
        if (playFirstOnStart) NextVideo();
    }

    void OnDisable()
    {
        if (_onPrepared != null) _vp.prepareCompleted -= _onPrepared;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            NextVideo();
        }
    }

    // Public entry so XR/UI can call a "tap" (e.g., XRSimpleInteractable activated ¡ú RegisterTap())
    public void RegisterTap()
    {
        NextVideo();
    }

    // VR "poke" via trigger collider
    private void OnTriggerEnter(Collider other)
    {
        if (!enableTriggerTap) return;

        // Check layer mask
        if (((1 << other.gameObject.layer) & triggerLayers) == 0) return;

        // Debounce
        if (Time.time - _lastTapTime < tapDebounce) return;
        _lastTapTime = Time.time;

        RegisterTap();
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
        // Stop the current one (if any)
        if (_vp.isPlaying) _vp.Stop();

        // Prefer clips over urls
        if (clips != null && clips.Count > 0)
        {
            _idx = (_idx + 1) % clips.Count;
            _vp.source = VideoSource.VideoClip;
            _vp.clip = clips[_idx];
            _vp.Prepare(); // Asynchronous; will auto-Play in prepareCompleted handler
        }
        else if (urls != null && urls.Count > 0)
        {
            _idx = (_idx + 1) % urls.Count;
            _vp.source = VideoSource.Url;
            _vp.url = ResolveUrl(urls[_idx]);
            _vp.Prepare();
        }
        else
        {
            Debug.LogWarning("[VideoCycleController] No video sources provided (Clips or Urls).");
        }
    }

    string ResolveUrl(string raw)
    {
        if (!urlsAreInStreamingAssets) return raw;

        // File under Assets/StreamingAssets/, e.g., "videos/a.mp4"
        // On Android and some platforms this may be inside the jar, but VideoPlayer supports Application.streamingAssetsPath.
        return Path.Combine(Application.streamingAssetsPath, raw);
    }
}