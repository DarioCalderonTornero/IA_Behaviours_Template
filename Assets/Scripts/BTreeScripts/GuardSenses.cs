using UnityEngine;

public class GuardSenses : MonoBehaviour
{
    [Header("Vista")]
    public float viewRange = 10f;
    public float viewAngle = 90f;
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    [Header("Oído")]
    public float hearRange = 4f;

    private Transform player;

    void Start()
    {
        player = GetComponent<GuardFSM>().player;
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer > viewRange) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > viewAngle / 2f) return false;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f,
                            dirToPlayer, distToPlayer, obstacleMask))
            return false;

        return true;
    }

    public bool CanHearPlayer()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= hearRange;
    }

    public Vector3 GetPlayerPosition()
    {
        return player != null ? player.position : Vector3.zero;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRange);

        Vector3 leftBound = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBound = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, leftBound * viewRange);
        Gizmos.DrawRay(transform.position, rightBound * viewRange);
    }
}