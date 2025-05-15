using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;


public class WFCgeneratorForCube : MonoBehaviour
{

    #region Singleton
    public static WFCgeneratorForCube wfcGen;
    private void Awake()
    {
        if (wfcGen == null)
            wfcGen = this;
        else
            Destroy(this);
    }
    #endregion

    //public Vector3 startingPosition;
    [Header("Map Setting")]
    public Vector3Int mapSize;

    [Header("Bulding Materials")]
    public CubeDatabase cubeDB;
    public GameObject stud;
    public GameObject node;
    public VoidRoomDatabase voidRoomDB;


    [Header("Painting")]
    public bool isPainting = false;
    public Material wallpaper;
    public Material studpaper;
    public float scaleOffset;

    [Header("etc")]
    public bool isVisibleMarker = false;

    public Cell[,,] grid; // 3차원 배열
    CubeData[] allCubes; // 회전값 적용한 복제품까지 포함하는 prefabs
    
    public int[,,] voidGrid;
    string[] tags = new string[] {"MarkerVertex", "MarkerEdge", "MarkerFace", "MarkerPath" };

    private void Start()
    {
        MakeCubeCopies(); // cell prefabs 을 방위각 별로 복사하여 저장
        PaintPrefabs(); // 프리팹에 벽지 붙이기

        CubeMapGenerate();

        DecorationMap(); // 맵 기둥, 가구배치
    }

    // 큐브 가져와서 붙이기
    public void CubeMapGenerate()
    {
        InitializeGrid(); // 그리드 마다 중첩된 cell 배열을 갖도록 초기화

        Queue<Vector3Int> updateQueue = new Queue<Vector3Int>(); // 전파 큐 생성; 엔트로피 줄어든 셀 위치 저장

        while (true)
        {
            var cellPos = FindLowestEntropyCell(); // 가장 낮은 엔트로피 셀 선택
            if (cellPos == null) break;

            Collapse(cellPos.Value); // 셀 확정
            updateQueue.Enqueue(cellPos.Value);
            Propagate(updateQueue); // 변경 전파; 주변 그리드에서 선택 가능한 cell 만 남기는 과정
        }

        ApplyToCubeMap(); // 결과 적용
    }

    void MakeCubeCopies()
    {
        CubeData[,] rotationPool;
        rotationPool = new CubeData[cubeDB.cubeData.Length, 4]; // 4 : xz 평면 90도씩 회전한 것 저장

        for (int i = 0; i < cubeDB.cubeData.Length; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                // 빈 SO 인스턴스 생성; SO 는 new 키워드 사용 불가
                var tempCubeData = ScriptableObject.CreateInstance<CubeData>();

                // 회전으로 인한 Connection 갱신
                switch (j)
                {
                    case 0: // 0f
                        tempCubeData.z_inverse = cubeDB.cubeData[i].z_inverse;
                        tempCubeData.z = cubeDB.cubeData[i].z;
                        tempCubeData.y_inverse = cubeDB.cubeData[i].y_inverse;
                        tempCubeData.y = cubeDB.cubeData[i].y; 
                        tempCubeData.x_inverse = cubeDB.cubeData[i].x_inverse;
                        tempCubeData.x = cubeDB.cubeData[i].x;
                        tempCubeData.rNum = 0;
                        break;
                    case 1: // 90f
                        tempCubeData.z_inverse = cubeDB.cubeData[i].x;
                        tempCubeData.z = cubeDB.cubeData[i].x_inverse;
                        tempCubeData.y_inverse = cubeDB.cubeData[i].y_inverse; // y는 고정 
                        tempCubeData.y = cubeDB.cubeData[i].y; // y는 고정
                        tempCubeData.x_inverse = cubeDB.cubeData[i].z_inverse;
                        tempCubeData.x = cubeDB.cubeData[i].z;
                        tempCubeData.rNum = 1;
                        break;
                    case 2: // 180f
                        tempCubeData.z_inverse = cubeDB.cubeData[i].z;
                        tempCubeData.z = cubeDB.cubeData[i].z_inverse;
                        tempCubeData.y_inverse = cubeDB.cubeData[i].y_inverse; // y는 고정 
                        tempCubeData.y = cubeDB.cubeData[i].y; // y는 고정
                        tempCubeData.x_inverse = cubeDB.cubeData[i].x;
                        tempCubeData.x = cubeDB.cubeData[i].x_inverse;
                        tempCubeData.rNum = 2;
                        break;
                    case 3: // 270f
                        tempCubeData.z_inverse = cubeDB.cubeData[i].x_inverse;
                        tempCubeData.z = cubeDB.cubeData[i].x;
                        tempCubeData.y_inverse = cubeDB.cubeData[i].y_inverse; // y는 고정 
                        tempCubeData.y = cubeDB.cubeData[i].y; // y는 고정
                        tempCubeData.x_inverse = cubeDB.cubeData[i].z;
                        tempCubeData.x = cubeDB.cubeData[i].z_inverse;
                        tempCubeData.rNum = 3;
                        break;
                }

                // Prefab 복제 (모든 항목을 초기화해야 오류 안남); 원본 프리팹 훼손 방지
                GameObject go = Instantiate(cubeDB.cubeData[i].cubePrefab, Vector3Int.down * 100, Quaternion.identity);
                tempCubeData.cubePrefab = go;
                tempCubeData.weight = cubeDB.cubeData[i].weight;
                tempCubeData.excludeFromOuter = cubeDB.cubeData[i].excludeFromOuter;
                rotationPool[i, j] = tempCubeData;
            }
        }
        allCubes = rotationPool.Cast<CubeData>().ToArray(); // 2차원 배열 -> 1차원 배열

        stud = Instantiate(stud, Vector3Int.down * 100, Quaternion.identity);
        node = Instantiate(node, Vector3Int.down * 100, Quaternion.identity);
    }

    void VoidInitialize()
    {
        voidGrid = new int[mapSize.x, mapSize.y, mapSize.z];


        // voidRoom copy
        VoidRoomData[,] VoidRoomPool;
        VoidRoomPool = new VoidRoomData[voidRoomDB.voidRoomData.Length, 4]; // 4 : xz 평면 90도씩 회전한 것 저장

        for (int i = 0; i < voidRoomDB.voidRoomData.Length; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                // 빈 SO 인스턴스 생성; SO 는 new 키워드 사용 불가
                var tempVoidRoomData = ScriptableObject.CreateInstance<VoidRoomData>();

                // 회전으로 인한 Connection 갱신
                switch (j)
                {
                    case 0: // 0f
                        tempVoidRoomData.rNum = 0;
                        break;
                    case 1: // 90f

                        tempVoidRoomData.rNum = 1;
                        break;
                    case 2: // 180f

                        tempVoidRoomData.rNum = 2;
                        break;
                    case 3: // 270f

                        tempVoidRoomData.rNum = 3;
                        break;
                }

                tempVoidRoomData.weight = voidRoomDB.voidRoomData[i].weight;


            }
        }
    }

    // 모든 셀 초기화 (하나의 cell 이 모든 후보 cube를 갖도록)
    void InitializeGrid()
    {
        grid = new Cell[mapSize.x, mapSize.y, mapSize.z];
        for (int z = 0; z < mapSize.z; z++)
            for (int y = 0; y < mapSize.y; y++)
                for (int x = 0; x < mapSize.x; x++)
                {
                    grid[x, y, z] = new Cell(allCubes);
                    //Debug.Log(grid[x, y, z].possibleCubes.Count);
                }

        //constrains: cell 에서 규칙 맞지 않는 connection 을 갖는 cube 제거
        for (int z = 0; z < mapSize.z; z++)
            for (int y = 0; y < mapSize.y; y++)
                for (int x = 0; x < mapSize.x; x++)
                {
                    if (z == 0)
                    {
                        ConstrainCheck(new Vector3Int(x, y, z), (t) => !t.excludeFromOuter);
                        SelfConnectCheck(new Vector3Int(x, y, z), Connection.path, (t, c) => t.z_inverse != c);
                        SelfConnectCheck(new Vector3Int(x, y, z), Connection.air, (t, c) => t.z_inverse != c);
                    }
                    if (z == mapSize.z - 1)
                    {
                        ConstrainCheck(new Vector3Int(x, y, z), (t) => !t.excludeFromOuter);
                        SelfConnectCheck(new Vector3Int(x, y, z), Connection.path, (t, c) => t.z != c);
                        SelfConnectCheck(new Vector3Int(x, y, z), Connection.air, (t, c) => t.z != c);
                    }
                    if (y == 0)
                    {
                        //ConstrainCheck(new Vector3Int(x, y, z), (t) => !t.excludeFromOuter);
                        SelfConnectCheck(new Vector3Int(x, y, z), Connection.path, (t, c) => t.y_inverse != c);
                        SelfConnectCheck(new Vector3Int(x, y, z), Connection.air, (t, c) => t.y_inverse != c);
                    }
                    if (y == mapSize.y - 1)
                    {
                        //ConstrainCheck(new Vector3Int(x, y, z), (t) => !t.excludeFromOuter);
                        SelfConnectCheck(new Vector3Int(x, y, z), Connection.path, (t, c) => t.y != c);
                        //SelfConnectCheck(new Vector3Int(x, y, z), Connection.air, (t, c) => t.y != c);
                    }
                    if (x == 0)
                    {
                        ConstrainCheck(new Vector3Int(x, y, z), (t) => !t.excludeFromOuter);
                        SelfConnectCheck(new Vector3Int(x, y, z), Connection.path, (t, c) => t.x_inverse != c);
                        SelfConnectCheck(new Vector3Int(x, y, z), Connection.air, (t, c) => t.x_inverse != c);
                    }
                    if (x == mapSize.x - 1)
                    {
                        ConstrainCheck(new Vector3Int(x, y, z), (t) => !t.excludeFromOuter);
                        SelfConnectCheck(new Vector3Int(x, y, z), Connection.path, (t, c) => t.x != c);
                        SelfConnectCheck(new Vector3Int(x, y, z), Connection.air, (t, c) => t.x != c);
                    }

                    // 입구, 출구 설정
                }
    }

    // 아직 collapse되지 않은 셀 중 가장 엔트로피가 낮은 셀 위치 반환
    Vector3Int? FindLowestEntropyCell()
    {
        Vector3Int? lowestPos = null;
        int minEntropy = int.MaxValue;

        for (int z = 0; z < mapSize.z; z++)
            for (int y = 0; y < mapSize.y; y++)
                for (int x = 0; x < mapSize.x; x++)
                {
                    var cell = grid[x, y, z];
                    if (cell.IsCollapsed || cell.possibleCubes.Count == 0) continue;

                    int entropy = cell.possibleCubes.Count;
                    if (entropy < minEntropy)
                    {
                        minEntropy = entropy;
                        lowestPos = new Vector3Int(x, y, z);
                    }
                }

        return lowestPos;
    }

    // Cell 내의 후보군 중 하나의 Cube를 가중치 반영해서 선정
    void Collapse(Vector3Int pos)
    {
        var cell = grid[pos.x, pos.y, pos.z];

        if (cell.possibleCubes.Count == 0)
        {
            Debug.LogError($"Collapse 실패: 후보 Cube가 없습니다. 위치: {pos}");
            return;
        }

        int totalWeight = cell.possibleCubes.Sum(t => t.weight);
        int rand = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var cube in cell.possibleCubes)
        {
            cumulative += cube.weight;
            if (rand < cumulative)
            {
                cell.possibleCubes = new List<CubeData> { cube };
                return;
            }
        }

        // 혹시 rand가 0 이상일 경우 대비
        cell.possibleCubes = new List<CubeData> { cell.possibleCubes.Last() };
    }

    // 변경된 셀 기준으로 주변 셀에 전파
    void Propagate(Queue<Vector3Int> updateQueue)
    {

        while (updateQueue.Count > 0)
        {
            bool isConnect = false;

            Vector3Int pos = updateQueue.Dequeue();
            CubeData center = grid[pos.x, pos.y, pos.z].GetCollapsedCube();
            if (center == null) continue;

            // 방향별 이웃 셀 업데이트; offset 위치의 target pos 가 center 와 연결되는지 체크
            isConnect = NeighborConnectCheck(pos, Vector3Int.up, center.y, (t, c) => t.y_inverse == c);
            if (isConnect) UpdateNeighborCell(pos, Vector3Int.up, updateQueue);
            isConnect = NeighborConnectCheck(pos, Vector3Int.down, center.y_inverse, (t, c) => t.y == c);
            if (isConnect) UpdateNeighborCell(pos, Vector3Int.down, updateQueue);
            isConnect = NeighborConnectCheck(pos, Vector3Int.left, center.x_inverse, (t, c) => t.x == c);
            if (isConnect) UpdateNeighborCell(pos, Vector3Int.left, updateQueue);
            isConnect = NeighborConnectCheck(pos, Vector3Int.right, center.x, (t, c) => t.x_inverse == c);
            if (isConnect) UpdateNeighborCell(pos, Vector3Int.right, updateQueue);
            isConnect = NeighborConnectCheck(pos, Vector3Int.forward, center.z, (t, c) => t.z_inverse == c);
            if (isConnect) UpdateNeighborCell(pos, Vector3Int.forward, updateQueue);
            isConnect = NeighborConnectCheck(pos, Vector3Int.back, center.z_inverse, (t, c) => t.z == c);
            if (isConnect) UpdateNeighborCell(pos, Vector3Int.back, updateQueue);
        }
    }

    void UpdateNeighborCell(Vector3Int centerPos, Vector3Int offset, Queue<Vector3Int> queue)
    {
        Vector3Int nPos = centerPos + offset;
        queue.Enqueue(nPos);
    }

    // 인접 셀 후보군을 필터링하고 줄어들었을 경우 전파 큐에 다시 넣음
    bool NeighborConnectCheck(Vector3Int centerPos, Vector3Int offset, Connection connection, System.Func<CubeData, Connection, bool> predicate)
    {
        Vector3Int nPos = centerPos + offset;
        if (nPos.x < 0 || nPos.y < 0 || nPos.z < 0 || nPos.x >= mapSize.x || nPos.y >= mapSize.y || nPos.z >= mapSize.z ) return false;

        var neighbor = grid[nPos.x, nPos.y, nPos.z];
        if (neighbor.IsCollapsed) return false;

        int before = neighbor.possibleCubes.Count;
        neighbor.possibleCubes = neighbor.possibleCubes.Where(t => predicate(t, connection)).ToList();

        if (neighbor.possibleCubes.Count != before) return true;

        //Debug.Log($"ConnectionFail : {nPos}, Before : {before}, After : {neighbor.possibleTiles.Count}" );
        return false;
    }

    bool SelfConnectCheck(Vector3Int centerPos, Connection connection, System.Func<CubeData, Connection, bool> predicate)
    {
        var targetGrid = grid[centerPos.x, centerPos.y, centerPos.z];
        if (targetGrid.IsCollapsed) return false;

        int before = targetGrid.possibleCubes.Count;
        targetGrid.possibleCubes = targetGrid.possibleCubes.Where(t => predicate(t, connection)).ToList();

        //Debug.Log($"ConnectCheck : {centerPos} | {before} -> {targetGrid.possibleCubes.Count}");
        if (targetGrid.possibleCubes.Count != before) return true;

        //Debug.Log($"ConnectionFail : {centerPos}");
        return false;
    }

    bool ConstrainCheck(Vector3Int Pos, System.Func<CubeData, bool> predicate)
    {
        var targetGrid = grid[Pos.x, Pos.y, Pos.z];
        if (targetGrid.IsCollapsed) return false;

        int before = targetGrid.possibleCubes.Count;
        targetGrid.possibleCubes = targetGrid.possibleCubes.Where(t => predicate(t)).ToList();

        if (targetGrid.possibleCubes.Count != before)
        {
            //Debug.Log($"Costrained : {Pos} | {before} -> {targetGrid.possibleCubes.Count}");
            return true;
        } 

        //Debug.Log($"Costrained fail : {Pos}");
        return false;
    }




    // 최종 확정된 Cube을 Grid에 반영
    void ApplyToCubeMap()
    {
        for (int z = 0; z < mapSize.z; z++)
            for (int y = 0; y < mapSize.y; y++)
                for (int x = 0; x < mapSize.x; x++)
                {
                    CubeData cube = grid[x, y, z].GetCollapsedCube();
                    //Debug.Log($"cube.name : {cube.name} | cube.rNum : {cube.rNum}");

                    if (cube != null)
                    {
                        // 좌표에 Cube 생성
                        GameObject go = Instantiate(cube.cubePrefab, new Vector3Int(x, y, z), Quaternion.Euler(0, 90f * cube.rNum, 0));
                        //GameObject go = Instantiate(cube.cubePrefab, new Vector3Int(x, y, z), Quaternion.identity);

                        // 벽 사이 마감
                        CubeDecorationWithTag(go, cube);
                    }
                }

        // Generate 끝난 뒤 검사
        for (int z = 0; z < mapSize.z; z++)
            for (int y = 0; y < mapSize.y; y++)
                for (int x = 0; x < mapSize.x; x++)
                {
                    if (!grid[x, y, z].IsCollapsed)
                    {
                        Debug.LogWarning($"셀 ({x},{y},{z}) 가 collapse 되지 않음. 후보수: {grid[x, y, z].possibleCubes.Count}");
                        GameObject go = Instantiate(cubeDB.cubeData[0].cubePrefab, new Vector3Int(x, y, z), Quaternion.identity);
                    }
                }

        
        // 최적화: 맵 생성 완료 후 Combine 적용
        CombineAllMeshesByMaterial();
    }

    void CombineAllMeshesByMaterial()
    {
        // material별로 MeshRenderer와 Transform 수집
        Dictionary<Material, List<MeshFilter>> groups = new Dictionary<Material, List<MeshFilter>>();

        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();

        // MeshRenderer 수집
        foreach (var r in renderers)
        {
            if (r.gameObject.layer != LayerMask.NameToLayer("MapUnit")) continue;

            // Marker 제외
            if (tags.Any(tag => r.CompareTag(tag))) continue;

            Material mat = r.sharedMaterial;
            if (!groups.ContainsKey(mat)) groups[mat] = new List<MeshFilter>();

            var mf = r.GetComponent<MeshFilter>();
            if (mf != null) groups[mat].Add(mf);

            // 기존 오브젝트는 비활성화 (필요 시 Destroy 가능)
            r.gameObject.SetActive(false);
        }

        // 각 Material 그룹별로 Combine
        foreach (var group in groups)
        {
            List<MeshFilter> filters = group.Value;
            CombineInstance[] combine = new CombineInstance[filters.Count]; // Mesh 합치기용 unity 구조체

            for (int i = 0; i < filters.Count; i++)
            {
                combine[i].mesh = filters[i].sharedMesh;
                combine[i].transform = filters[i].transform.localToWorldMatrix;
            }

            // mesh 합치기
            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // 저장용량 늘리기: 65,535 vertex -> 4,294,967,295 vertex     
            combinedMesh.CombineMeshes(combine);

            GameObject combinedObj = new GameObject("CombinedMesh_" + group.Key.name);
            combinedObj.AddComponent<MeshFilter>().mesh = combinedMesh;
            combinedObj.AddComponent<MeshRenderer>().material = group.Key;
            combinedObj.isStatic = true;
            combinedObj.AddComponent<MeshCollider>().sharedMesh = combinedMesh;
        }
    }



    void CubeDecorationWithTag(GameObject go, CubeData cube)
    {
        //CubeData cd = go.GetComponent<CubeData>();
        MeshRenderer[] me = go.GetComponentsInChildren<MeshRenderer>(); // MarkerEdge 가져오기

        // Cube 마다 다른 local 회전값을 보정한 기둥 설치
        Vector3 dir = cube.rNum % 2 == 0 ? Vector3.forward : Vector3.right;
        Vector3 dir2 = cube.rNum % 2 == 0 ? Vector3.right : Vector3.forward;

        foreach (var e in me)
        {
            if (e.CompareTag(tags[1])) // Stud 설치
            {
                if (e.transform.localPosition.y == 0.0f)
                    Instantiate(stud, e.transform.position, Quaternion.identity);
                else if (e.transform.localPosition.x == 0.0f)
                    Instantiate(stud, e.transform.position, Quaternion.Euler(dir * 90f));
                else if (e.transform.localPosition.z == 0.0f)
                    Instantiate(stud, e.transform.position, Quaternion.Euler(dir2 * 90f));
                else
                    Debug.Log("stud object 생성 오류");
            }
            else if (e.CompareTag(tags[0])) // Node 설치
            {
                // Vertex marker는 모든 큐브에 존재하므로 체크 후 빈 공간에는 설치하지 않음
                if (Physics.OverlapSphere(e.transform.position, 0.1f).Length > 1) 
                    Instantiate(node, e.transform.position, Quaternion.identity);
            }
        }
    }

    void PaintPrefabs()
    {
        if (isPainting)
        {
            // 프리팹에 벽지 붙이기
            foreach (CubeData cube in allCubes)
            {
                MeshRenderer[] renderers = cube.cubePrefab.GetComponentsInChildren<MeshRenderer>();
            
                foreach (MeshRenderer r in renderers)
                {
                    // Marker 제외
                    if (tags.Any(tag => r.CompareTag(tag))) continue;

                    r.transform.localScale = r.transform.localScale * scaleOffset; // 스케일 조정, 빛샘 방지
                    r.material = wallpaper;
                }       
            }

            // 프레임에 색칠
            MeshRenderer mr = stud.GetComponentInChildren<MeshRenderer>();
            mr.material = studpaper;

            // 노드에 색칠
            mr = node.GetComponentInChildren<MeshRenderer>();
            mr.material = studpaper;
        }
    }

    void DecorationMap()
    {
        GameObject[] me = GameObject.FindGameObjectsWithTag(tags[0]);
        GameObject[] mv = GameObject.FindGameObjectsWithTag(tags[1]);
        GameObject[] mf = GameObject.FindGameObjectsWithTag(tags[2]);
        GameObject[] mp = GameObject.FindGameObjectsWithTag(tags[3]);

        // 외벽 배치
        // overlap 되지 않은 MarkerFace 에 외벽 Prefab 배치, rNum 고려

        // 천장 조명 설치
        foreach (var e in mf)
        {
            if (e.name == "MarkerF_y")
            {
                LightSetup.ls.AddCeilingLight(e.transform.position);
            }
        }

        // 계단 프리팹 배치

        // 가구 프리팹 배치

        // marker 숨기기
        if (!isVisibleMarker)
        {
            foreach (var e in me) e.SetActive(false);
            foreach (var v in mv) v.SetActive(false);
            foreach (var f in mf) f.SetActive(false);
            foreach (var p in mp) p.SetActive(false);
        }
    }


}

// 조명, 계단, 스폰 포인트, 연속성
// pathfinding