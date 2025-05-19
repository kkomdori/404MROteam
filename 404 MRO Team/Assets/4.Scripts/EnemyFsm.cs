using UnityEngine;
using UnityEngine.AI;

public class EnemyFSM : MonoBehaviour
{
    // ���� ����
    private enum EnemyState { Patrol, Chase, Search }
    private EnemyState currentState;

    private NavMeshAgent agent;
    private Transform targetPlayer;
    private Vector3 lastSeenPosition;

    // ����
    [Header("����")]
    public float searchDuration = 5f;
    private float searchStartTime;
    public float searchRadius = 3f;

    // ����
    [Header("����")]
    public float detectionRange = 7f;       // �Ϲ� ���� �Ÿ� (����)
    public float fieldOfView = 120f;        // �þ߰�
    public float viewDistance = 10f;        // �þ� �Ÿ�
    public LayerMask viewObstacleMask;      // �þ߸� ������ ������Ʈ ���̾�

    // �� ���� ����
    private Vector3 navMeshMin = new Vector3(-100f, 0f, -100f);
    private Vector3 navMeshMax = new Vector3(100f, 0f, 100f);

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = EnemyState.Patrol;
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolBehavior();
                DetectPlayersBySight();
                Debug.Log("���� ��");
                break;

            case EnemyState.Chase:
                if (targetPlayer != null)
                {
                    agent.SetDestination(targetPlayer.position);
                    Debug.Log("���� ��");

                    float distance = Vector3.Distance(transform.position, targetPlayer.position);
                    if (distance > viewDistance * 1.5f)
                    {
                        lastSeenPosition = targetPlayer.position;
                        targetPlayer = null;
                        currentState = EnemyState.Search;
                        searchStartTime = Time.time;
                    }
                }
                break;

            case EnemyState.Search:
                SearchLastSeenArea();
                Debug.Log("���� ��");

                if (Time.time - searchStartTime > searchDuration)
                {
                    currentState = EnemyState.Patrol;
                }
                break;
        }
    }

    void PatrolBehavior()
    {
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint;
            if (GetRandomPointOnNavMesh(out randomPoint))
            {
                agent.SetDestination(randomPoint);
            }
        }
    }

    bool GetRandomPointOnNavMesh(out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(navMeshMin.x, navMeshMax.x),
                0f,
                Random.Range(navMeshMin.z, navMeshMax.z)
            );

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = Vector3.zero;
        return false;
    }

    // FOV ����: �þ߰� + �Ÿ� + ����ĳ��Ʈ�� ����
    void DetectPlayersBySight()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in players)
        {
            Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, player.transform.position);

            // �þ� ���� ���� üũ
            if (Vector3.Angle(transform.forward, dirToPlayer) < fieldOfView * 0.5f && distance < viewDistance)
            {
                // ��ֹ� ���� üũ (����ĳ��Ʈ)
                if (!Physics.Linecast(transform.position + Vector3.up, player.transform.position + Vector3.up, viewObstacleMask))
                {
                    targetPlayer = player.transform;
                    currentState = EnemyState.Chase;
                    return;
                }
            }
        }
    }

    // �ܺο��� ȣ��: �Ҹ� �߻� �� ȣ���
    public void HearNoise(Vector3 noisePosition)
    {
        float dist = Vector3.Distance(transform.position, noisePosition);
        if (dist < detectionRange && currentState == EnemyState.Patrol)
        {
            lastSeenPosition = noisePosition;
            currentState = EnemyState.Search;
            searchStartTime = Time.time;
            Debug.Log("�Ҹ� ������ ���� ����");
        }
    }

    void SearchLastSeenArea()
    {
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 randomPos = lastSeenPosition + Random.insideUnitSphere * searchRadius;
            randomPos.y = 0f;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }
}
