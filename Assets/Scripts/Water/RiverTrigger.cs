using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

public class RiverTrigger : MonoBehaviour
{
    [Header("Spline Setup")]
    public SplineContainer spline;
    public float followSpeed = 0.1f;
    public float sinkTargetY = -70f;
    public float sinkDuration = 1.5f;
    [Header("Immersion Settings")]
    public float immersionDepth = 0.5f;

    bool   isFollowing;
    float  t;
    Transform player;

    void OnTriggerEnter (Collider other)
    {
        var controller = other.GetComponentInParent<ThirdPersonController>();
        if (controller == null || isFollowing) return;

        controller.enabled = false;
        player             = controller.transform;
        StartCoroutine(SinkThenFollow(controller));
    }

    IEnumerator SinkThenFollow(ThirdPersonController controller)
    {
        isFollowing = true;

        // Désactive juste le CharacterController
        if (player.TryGetComponent(out CharacterController cc)) cc.enabled = false;

        // Joue l’animation de saut pendant la glissade
        if (controller.animator != null)
        {
            controller.animator.SetTrigger("Jumping");
        }

        t = FindClosestT(spline, player.position);
        Vector3 splineStart = spline.EvaluatePosition(t);
        splineStart.y -= 0.8f;

        Vector3 start = player.position;
        float elapsed = 0f;

        while (elapsed < sinkDuration)
        {
            player.position = Vector3.Lerp(start, splineStart, elapsed / sinkDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        player.position = splineStart;

        while (t < 1f)
        {
            Vector3 pos = spline.EvaluatePosition(t);
            pos.y -= immersionDepth;
            player.position = pos;
            t += Time.deltaTime * followSpeed;
            yield return null;
        }

        if (cc != null) cc.enabled = true;
        controller.enabled = true;
        isFollowing = false;
    }


    /* ---------- UTILITAIRE ---------- */
    float FindClosestT(SplineContainer container, Vector3 worldPos, int resolution = 100)
    {
        float closestT   = 0f;
        float minSqrDist = float.MaxValue;

        for (int i = 0; i <= resolution; i++)
        {
            float   candT  = i / (float)resolution;
            Vector3 point  = container.EvaluatePosition(candT);  // world space
            float   dist2  = (point - worldPos).sqrMagnitude;

            if (dist2 < minSqrDist)
            {
                minSqrDist = dist2;
                closestT   = candT;
            }
        }
        return closestT;
    }
}
