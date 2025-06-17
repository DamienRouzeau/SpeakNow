using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

public class RiverTrigger : MonoBehaviour
{
    [Header("Spline Setup")]
    public SplineContainer spline;
    public float followSpeed = 0.1f;
    public float sinkDuration = 1.5f;

    [Header("Immersion Settings")]
    public float immersionDepth = 0.5f;

    [Header("Rotation Dramatique")]
    public float rotationSpeed = 360f;

    [Header("Point de sortie à la fin")]
    public Transform exitPoint;
    public float exitTransitionDuration = 1.2f;

    bool isFollowing;
    float t;
    Transform player;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[RiverTrigger] Trigger détecté : {other.name} (tag: {other.tag}) | parent: {other.transform.root.name}");

        var controller = other.GetComponentInParent<ThirdPersonController>();
        if (controller == null || isFollowing) return;

        controller.enabled = false;
        player = controller.transform;
        StartCoroutine(SinkThenFollow(controller));
    }
    
    float GetImmersionOffset(ThirdPersonController ctrl)
    {
        // Variante 1 : au feeling, via l’énum
        switch (ctrl.GetSize())
        {
            case size.little:  return immersionDepth * 0.12f;
            case size.normal:  return immersionDepth;
            case size.big:     return immersionDepth * 1.4f;
        }
        return immersionDepth;
    }

    IEnumerator SinkThenFollow(ThirdPersonController controller)
    {
        isFollowing = true;

        if (player.TryGetComponent(out CharacterController cc)) cc.enabled = false;

        if (controller.animator != null)
            controller.animator.SetBool("IsSpline", true);

        float immersionOffset = GetImmersionOffset(controller); // ✅

        t = FindClosestT(spline, player.position);
        Vector3 splineStart = spline.EvaluatePosition(t);
        splineStart.y -= immersionOffset; // ✅

        Vector3 start = player.position;
        float elapsed = 0f;

        while (elapsed < sinkDuration)
        {
            player.position = Vector3.Lerp(start, splineStart, elapsed / sinkDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        player.position = splineStart;

        // Suivre la spline
        while (t < 1f)
        {
            Vector3 pos = spline.EvaluatePosition(t);
            pos.y -= immersionOffset; // ✅
            player.position = pos;

            player.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
            t += Time.deltaTime * followSpeed;
            yield return null;
        }

        // Sortie à la fin
        if (exitPoint != null)
        {
            Vector3 from = player.position;
            Vector3 to = exitPoint.position;
            float time = 0f;

            while (time < exitTransitionDuration)
            {
                player.position = Vector3.Lerp(from, to, time / exitTransitionDuration);
                time += Time.deltaTime;
                yield return null;
            }

            player.position = to;
        }

        if (controller.animator != null)
            controller.animator.SetBool("IsSpline", false);
        if (cc != null) cc.enabled = true;
        controller.enabled = true;
        isFollowing = false;
    }


    float FindClosestT(SplineContainer container, Vector3 worldPos, int resolution = 100)
    {
        float closestT = 0f;
        float minSqrDist = float.MaxValue;

        for (int i = 0; i <= resolution; i++)
        {
            float candT = i / (float)resolution;
            Vector3 point = container.EvaluatePosition(candT);
            float dist2 = (point - worldPos).sqrMagnitude;

            if (dist2 < minSqrDist)
            {
                minSqrDist = dist2;
                closestT = candT;
            }
        }
        return closestT;
    }
}
