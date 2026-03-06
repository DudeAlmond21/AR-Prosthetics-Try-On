using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Builds Unity GameObjects for every ARKit body joint and updates them each frame.
/// Uses integer joint indices (ARBodyJoint constants) — compatible with ARFoundation 5.x.
///
/// ── Windows / Editor ──────────────────────────────────────────────────────
/// Automatically creates a mock T-pose skeleton so all prosthetic scripts
/// can be developed and tested without a device.
/// Use Arrow Keys in Play mode to slide the skeleton around.
///
/// ── Device (iPhone 12+, iOS) ──────────────────────────────────────────────
/// Reads live data from ARHumanBodyManager each frame.
///
/// SETUP
///   • Add to the same GameObject as ARHumanBodyManager (XR Origin).
///   • Assign the ARHumanBodyManager reference in the Inspector.
/// </summary>
public class HumanBodyJointController : MonoBehaviour
{
    [Header("AR References (auto-mocked in Editor/Windows)")]
    [SerializeField] ARHumanBodyManager humanBodyManager;

    [Header("Debug — shows joint spheres in scene view")]
    [SerializeField] bool showJointSpheres = false;

    // ── Public ────────────────────────────────────────────────────────────────
    public Transform[] JointTransforms { get; private set; }
    public bool BodyTracked { get; private set; }

    public event Action OnBodyDetected;
    public event Action OnBodyLost;

    // ── Private ───────────────────────────────────────────────────────────────
    GameObject[] jointObjects;
    GameObject   skeletonRoot;
    bool         hierarchyBuilt;
    bool         isMock;

    // T-pose world positions for the joints we actually use in mock mode
    static readonly Dictionary<int, Vector3> MockPositions = new Dictionary<int, Vector3>
    {
        { ARBodyJoint.Root,           new Vector3( 0.00f, 0.00f, 0.00f) },
        { ARBodyJoint.Hips,           new Vector3( 0.00f, 0.95f, 0.00f) },
        { ARBodyJoint.LeftShoulder1,  new Vector3(-0.18f, 1.40f, 0.00f) },
        { ARBodyJoint.LeftArm,        new Vector3(-0.35f, 1.38f, 0.00f) },
        { ARBodyJoint.LeftForearm,    new Vector3(-0.55f, 1.20f, 0.00f) },
        { ARBodyJoint.LeftHand,       new Vector3(-0.72f, 1.05f, 0.00f) },
        { ARBodyJoint.RightShoulder1, new Vector3( 0.18f, 1.40f, 0.00f) },
        { ARBodyJoint.RightArm,       new Vector3( 0.35f, 1.38f, 0.00f) },
        { ARBodyJoint.RightForearm,   new Vector3( 0.55f, 1.20f, 0.00f) },
        { ARBodyJoint.RightHand,      new Vector3( 0.72f, 1.05f, 0.00f) },
    };

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Start()
    {
#if UNITY_EDITOR || !UNITY_IOS
        isMock = true;
        BuildMockSkeleton();
        Debug.Log("[BodyJoints] MOCK skeleton active.");
#endif
    }

    void OnEnable()
    {
        if (!isMock && humanBodyManager != null)
            humanBodyManager.humanBodiesChanged += OnBodiesChanged;
    }

    void OnDisable()
    {
        if (!isMock && humanBodyManager != null)
            humanBodyManager.humanBodiesChanged -= OnBodiesChanged;
    }

    // ── ARKit live callbacks ──────────────────────────────────────────────────
    void OnBodiesChanged(ARHumanBodiesChangedEventArgs args)
    {
        foreach (var body in args.added)
        {
            BuildLiveSkeleton(body);
            BodyTracked = true;
            OnBodyDetected?.Invoke();
        }
        foreach (var body in args.updated)
        {
            if (hierarchyBuilt) UpdateLiveJoints(body);
        }
        foreach (var _ in args.removed)
        {
            BodyTracked = false;
            OnBodyLost?.Invoke();
        }
    }

    void BuildLiveSkeleton(ARHumanBody body)
    {
        var joints = body.joints;
        int count  = joints.Length;

        JointTransforms = new Transform[count];
        jointObjects    = new GameObject[count];

        skeletonRoot = new GameObject("SkeletonRoot");
        skeletonRoot.transform.SetParent(body.transform, false);

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"Joint_{i}");
            if (showJointSpheres) AttachSphere(go);
            jointObjects[i]    = go;
            JointTransforms[i] = go.transform;
        }

        for (int i = 0; i < count; i++)
        {
            int p = joints[i].parentIndex;
            jointObjects[i].transform.SetParent(
                (p >= 0 && p < count) ? jointObjects[p].transform : skeletonRoot.transform,
                worldPositionStays: false);
        }

        UpdateLiveJoints(body);
        hierarchyBuilt = true;
    }

    void UpdateLiveJoints(ARHumanBody body)
    {
        var joints = body.joints;
        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i].tracked && jointObjects[i] != null)
            {
                jointObjects[i].transform.localPosition = joints[i].localPose.position;
                jointObjects[i].transform.localRotation = joints[i].localPose.rotation;
            }
        }
    }

    // ── Mock skeleton ─────────────────────────────────────────────────────────
    void BuildMockSkeleton()
    {
        int count = ARBodyJoint.TotalJoints;

        JointTransforms = new Transform[count];
        jointObjects    = new GameObject[count];

        skeletonRoot = new GameObject("MockSkeletonRoot");
        skeletonRoot.transform.SetParent(transform, false);

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"Joint_{i}");
            go.transform.position = MockPositions.TryGetValue(i, out var pos) ? pos : Vector3.zero;
            go.transform.SetParent(skeletonRoot.transform, worldPositionStays: true);
            if (showJointSpheres) AttachSphere(go);
            jointObjects[i]    = go;
            JointTransforms[i] = go.transform;
        }

        hierarchyBuilt = true;
        BodyTracked    = true;
        OnBodyDetected?.Invoke();
    }

    void Update()
    {
        if (!isMock || skeletonRoot == null) return;
        float s = 0.5f * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftArrow))  skeletonRoot.transform.position += Vector3.left  * s;
        if (Input.GetKey(KeyCode.RightArrow)) skeletonRoot.transform.position += Vector3.right * s;
        if (Input.GetKey(KeyCode.UpArrow))    skeletonRoot.transform.position += Vector3.up    * s;
        if (Input.GetKey(KeyCode.DownArrow))  skeletonRoot.transform.position += Vector3.down  * s;
    }

    // ── Public API ────────────────────────────────────────────────────────────
    /// <summary>Get a joint Transform by its ARBodyJoint integer index.</summary>
    public Transform GetJointTransform(int jointIndex)
    {
        if (!hierarchyBuilt || JointTransforms == null) return null;
        return (jointIndex >= 0 && jointIndex < JointTransforms.Length)
            ? JointTransforms[jointIndex] : null;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    void AttachSphere(GameObject parent)
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        s.transform.SetParent(parent.transform, false);
        s.transform.localScale = Vector3.one * 0.03f;
        Destroy(s.GetComponent<Collider>());
    }

    void OnDestroy()
    {
        if (skeletonRoot != null) Destroy(skeletonRoot);
    }
}
