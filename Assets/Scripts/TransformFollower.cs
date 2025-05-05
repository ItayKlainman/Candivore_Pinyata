using UnityEngine;

public class TransformFollower : MonoBehaviour
{
    private Transform target;
    private bool isFollowing = false;
    private float followDuration = 0.2f;
    private float followTimer;

    public void SnapTo(Transform followTarget, float duration = 0.2f)
    {
        target = followTarget;
        isFollowing = true;
        followDuration = duration;
        followTimer = 0f;
        transform.position = target.position; // initial snap
    }

    void Update()
    {
        if (isFollowing && target != null)
        {
            followTimer += Time.deltaTime;
            transform.position = target.position;

            if (followTimer >= followDuration)
            {
                isFollowing = false;
                target = null;
            }
        }
    }
}