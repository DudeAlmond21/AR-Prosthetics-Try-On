using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;

public class ARProstheticAttacher : MonoBehaviour
{
    [Header("References")]
    public ARHumanBodyManager humanBodyManager;
    public GameObject prostheticPrefab;

    private GameObject spawnedProsthetic;

    // ARKit joint indices for AR Foundation 5.2.x
    private const int LEFT_SHOULDER = 9;
    private const int LEFT_ELBOW = 11;

    private const int RIGHT_SHOULDER = 10;
    private const int RIGHT_ELBOW = 12;

    private void OnEnable()
    {
        if (humanBodyManager != null)
            humanBodyManager.humanBodiesChanged += OnHumanBodiesChanged;
    }

    private void OnDisable()
    {
        if (humanBodyManager != null)
            humanBodyManager.humanBodiesChanged -= OnHumanBodiesChanged;
    }

    private void OnHumanBodiesChanged(ARHumanBodiesChangedEventArgs args)
    {
        foreach (var body in args.added)
        {
            SpawnIfNeeded();
            UpdateProsthetic(body);
        }

        foreach (var body in args.updated)
        {
            UpdateProsthetic(body);
        }

        foreach (var body in args.removed)
        {
            if (spawnedProsthetic != null)
            {
                Destroy(spawnedProsthetic);
                spawnedProsthetic = null;
            }
        }
    }

    private void SpawnIfNeeded()
    {
        if (prostheticPrefab == null)
            return;

        if (spawnedProsthetic == null)
            spawnedProsthetic = Instantiate(prostheticPrefab);
    }

    private void UpdateProsthetic(ARHumanBody body)
    {
        if (spawnedProsthetic == null)
            return;

        if (!body.joints.IsCreated || body.joints.Length == 0)
            return;

        int jointIndex = GetTargetJointIndex();

        if (jointIndex < 0 || jointIndex >= body.joints.Length)
            return;

        XRHumanBodyJoint joint = body.joints[jointIndex];

        if (!joint.tracked)
            return;

        // Since pivot is at stump, we directly attach
        spawnedProsthetic.transform.SetPositionAndRotation(
            joint.anchorPose.position,
            joint.anchorPose.rotation
        );
    }

    private int GetTargetJointIndex()
    {
        if (AppState.Instance == null)
            return RIGHT_ELBOW;

        ArmSide side = AppState.Instance.SelectedArmSide;
        AmputationLevel level = AppState.Instance.CurrentLevel;

        if (side == ArmSide.Right)
        {
            if (level == AmputationLevel.AboveElbow)
                return RIGHT_SHOULDER;

            return RIGHT_ELBOW; // Default for BelowElbow
        }
        else
        {
            if (level == AmputationLevel.AboveElbow)
                return LEFT_SHOULDER;

            return LEFT_ELBOW; // Default for BelowElbow
        }
    }
}