using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// BLE Manager — single point of contact for IMU data in Unity.
///
/// On iOS device  → calls the native Objective-C CoreBluetooth plugin.
/// On Windows/Editor → uses mock oscillating values (or keyboard overrides).
///
/// ── How the data flows ────────────────────────────────────────────────────
///   ESP32 (BLE Server)
///     └─ Service UUID:       matching BLE_SERVICE_UUID
///     └─ Characteristic UUID matching BLE_CHAR_UUID
///     └─ Notifies: "pitch,roll\n"  e.g. "23.4,-11.2\n"
///
///   iOS native plugin (BLENativePlugin.mm)
///     └─ Scans, connects, reads notifications
///     └─ Calls back into Unity:  BLEManager.OnBLEData("23.4,-11.2")
///
///   BLEManager (this file)
///     └─ Parses string → Pitch / Roll floats
///     └─ IMUGestureDetector reads Pitch/Roll each frame
///
/// ── Inspector Setup ───────────────────────────────────────────────────────
///   • Put this on a DontDestroyOnLoad GameObject (or the AR Session Origin).
///   • Match BLE_SERVICE_UUID and BLE_CHAR_UUID with your Arduino sketch.
/// </summary>
public class BLEManager : MonoBehaviour
{
    // ── UUIDs — MUST match the Arduino sketch ─────────────────────────────────
    // You can keep the ones below or generate your own at uuidgenerator.net
    const string BLE_SERVICE_UUID = "12345678-1234-1234-1234-123456789abc";
    const string BLE_CHAR_UUID    = "abcdefab-cdef-abcd-efab-cdefabcdefab";

    // ── Public State ──────────────────────────────────────────────────────────
    public float Pitch      { get; private set; }
    public float Roll       { get; private set; }
    public bool  Connected  { get; private set; }

    public event Action    OnConnected;
    public event Action    OnDisconnected;
    public event Action<string> OnError;

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Mock mode (auto-active in Editor/Windows)")]
    [SerializeField] bool  mockOscillate   = true;
    [SerializeField] float mockOscillateHz = 0.3f;   // cycles per second

    // ── Singleton ─────────────────────────────────────────────────────────────
    public static BLEManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Start()
    {
#if UNITY_IOS && !UNITY_EDITOR
        // Native plugin initialises CoreBluetooth and starts scanning
        _BLE_Initialize(gameObject.name, BLE_SERVICE_UUID, BLE_CHAR_UUID);
        Debug.Log("[BLE] iOS native plugin initialised.");
#else
        // Windows / Editor: fake a connected state for testing
        Connected = true;
        OnConnected?.Invoke();
        Debug.Log("[BLE] MOCK mode active — oscillating pitch/roll values.");
#endif
    }

    void Update()
    {
#if UNITY_EDITOR || !UNITY_IOS
        if (mockOscillate)
        {
            float t   = Time.time * mockOscillateHz * Mathf.PI * 2f;
            Pitch = Mathf.Sin(t)        * 45f;   // ±45°
            Roll  = Mathf.Sin(t + 1.2f) * 30f;   // ±30°, offset phase
        }
#endif
    }

    // ── Public API ────────────────────────────────────────────────────────────
    /// <summary>Start BLE scan (call from BLEConnectionScreen UI button).</summary>
    public void StartScan()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _BLE_StartScan();
#else
        Debug.Log("[BLE] StartScan() — mock, no-op.");
#endif
    }

    /// <summary>Disconnect cleanly (call on app pause/exit).</summary>
    public void Disconnect()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _BLE_Disconnect();
#else
        Connected = false;
        OnDisconnected?.Invoke();
#endif
    }

    // ── Callbacks from native plugin (UnitySendMessage) ──────────────────────
    // The Objective-C plugin calls these by name via UnitySendMessage.
    // The GameObject name must match the name passed to _BLE_Initialize.

    /// <summary>Called by native plugin when device connects.</summary>
    public void OnBLEConnected(string deviceName)
    {
        Connected = true;
        Debug.Log($"[BLE] Connected to: {deviceName}");
        OnConnected?.Invoke();
    }

    /// <summary>Called by native plugin when device disconnects.</summary>
    public void OnBLEDisconnected(string message)
    {
        Connected = false;
        Debug.Log($"[BLE] Disconnected: {message}");
        OnDisconnected?.Invoke();
    }

    /// <summary>
    /// Called by native plugin on each BLE notification.
    /// Expected format from ESP32: "pitch,roll"  e.g. "23.4,-11.2"
    /// </summary>
    public void OnBLEData(string data)
    {
        var parts = data.Trim().Split(',');
        if (parts.Length < 2) return;

        if (float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                           System.Globalization.CultureInfo.InvariantCulture, out float p))
            Pitch = p;

        if (float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                           System.Globalization.CultureInfo.InvariantCulture, out float r))
            Roll = r;
    }

    /// <summary>Called by native plugin on error.</summary>
    public void OnBLEError(string error)
    {
        Debug.LogError($"[BLE] Error: {error}");
        OnError?.Invoke(error);
    }

    // ── Native plugin extern declarations ─────────────────────────────────────
    // These are implemented in BLENativePlugin.mm (Plugins/iOS folder).
    // The #if guard means they are ONLY declared on iOS builds, so Windows
    // compilation never sees them and won't throw linker errors.
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void _BLE_Initialize(string unityObjectName,
                                       string serviceUUID,
                                       string characteristicUUID);

    [DllImport("__Internal")]
    static extern void _BLE_StartScan();

    [DllImport("__Internal")]
    static extern void _BLE_Disconnect();
#endif
}
