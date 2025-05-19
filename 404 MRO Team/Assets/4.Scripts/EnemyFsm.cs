using UnityEngine;
using UnityEngine.AI;

public class EnemyFSM : MonoBehaviour
{
    // 상태 정의
    private enum EnemyState { Patrol, Chase, Search }
    private EnemyState currentState;

    private NavMeshAgent agent;
    private Transform targetPlayer;
    private Vector3 lastSeenPosition;

    // 수색
    [Header("수색")]
    public float searchDuration = 5f;
    private float searchStartTime;
    public float searchRadius = 3f;

    // 감지
    [Header("감지")]
    public float detectionRange = 7f;       // 일반 감지 거리 (원형)
    public float fieldOfView = 120f;        // 시야각
    public float viewDistance = 10f;        // 시야 거리
    public LayerMask viewObstacleMask;      // 시야를 가리는 오브젝트 레이어

    // 맵 범위 설정
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
                Debug.Log("순찰 중");
                break;

            case EnemyState.Chase:
                if (targetPlayer != null)
                {
                    agent.SetDestination(targetPlayer.position);
                    Debug.Log("추적 중");

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
                Debug.Log("수색 중");

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

    // FOV 감지: 시야각 + 거리 + 레이캐스트로 감지
    void DetectPlayersBySight()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in players)
        {
            Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, player.transform.position);

            // 시야 범위 각도 체크
            if (Vector3.Angle(transform.forward, dirToPlayer) < fieldOfView * 0.5f && distance < viewDistance)
            {
                // 장애물 차단 체크 (레이캐스트)
                if (!Physics.Linecast(transform.position + Vector3.up, player.transform.position + Vector3.up, viewObstacleMask))
                {
                    targetPlayer = player.transform;
                    currentState = EnemyState.Chase;
                    return;
                }
            }
        }
    }

    // 외부에서 호출: 소리 발생 시 호출됨
    public void HearNoise(Vector3 noisePosition)
    {
        float dist = Vector3.Distance(transform.position, noisePosition);
        if (dist < detectionRange && currentState == EnemyState.Patrol)
        {
            lastSeenPosition = noisePosition;
            currentState = EnemyState.Search;
            searchStartTime = Time.time;
            Debug.Log("소리 감지로 수색 시작");
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
