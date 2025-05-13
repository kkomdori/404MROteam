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

    public Cell[,,] grid; // 3���� �迭
    CubeData[] allCubes; // ȸ���� ������ ����ǰ���� �����ϴ� prefabs
    
    public int[,,] voidGrid;
    string[] tags = new string[] {"MarkerVertex", "MarkerEdge", "MarkerFace", "MarkerPath" };

    private void Start()
    {
        MakeCubeCopies(); // cell prefabs �� ������ ���� �����Ͽ� ����
        PaintPrefabs(); // �����տ� ���� ���̱�

        CubeMapGenerate();

        DecorationMap(); // �� ���, ������ġ
    }

    // ť�� �����ͼ� ���̱�
    public void CubeMapGenerate()
    {
        InitializeGrid(); // �׸��� ���� ��ø�� cell �迭�� ������ �ʱ�ȭ

        Queue<Vector3Int> updateQueue = new Queue<Vector3Int>(); // ���� ť ����; ��Ʈ���� �پ�� �� ��ġ ����

        while (true)
        {
            var cellPos = FindLowestEntropyCell(); // ���� ���� ��Ʈ���� �� ����
            if (cellPos == null) break;

            Collapse(cellPos.Value); // �� Ȯ��
            updateQueue.Enqueue(cellPos.Value);
            Propagate(updateQueue); // ���� ����; �ֺ� �׸��忡�� ���� ������ cell �� ����� ����
        }

        ApplyToCubeMap(); // ��� ����
    }

    void MakeCubeCopies()
    {
        CubeData[,] rotationPool;
        rotationPool = new CubeData[cubeDB.cubeData.Length, 4]; // 4 : xz ��� 90���� ȸ���� �� ����

        for (int i = 0; i < cubeDB.cubeData.Length; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                // �� SO �ν��Ͻ� ����; SO �� new Ű���� ��� �Ұ�
                var tempCubeData = ScriptableObject.CreateInstance<CubeData>();

                // ȸ������ ���� Connection ����
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
                        tempCubeData.y_inverse = cubeDB.cubeData[i].y_inverse; // y�� ���� 
                        tempCubeData.y = cubeDB.cubeData[i].y; // y�� ����
                        tempCubeData.x_inverse = cubeDB.cubeData[i].z_inverse;
                        tempCubeData.x = cubeDB.cubeData[i].z;
                        tempCubeData.rNum = 1;
                        break;
                    case 2: // 180f
                        tempCubeData.z_inverse = cubeDB.cubeData[i].z;
                        tempCubeData.z = cubeDB.cubeData[i].z_inverse;
                        tempCubeData.y_inverse = cubeDB.cubeData[i].y_inverse; // y�� ���� 
                        tempCubeData.y = cubeDB.cubeData[i].y; // y�� ����
                        tempCubeData.x_inverse = cubeDB.cubeData[i].x;
                        tempCubeData.x = cubeDB.cubeData[i].x_inverse;
                        tempCubeData.rNum = 2;
                        break;
                    case 3: // 270f
                        tempCubeData.z_inverse = cubeDB.cubeData[i].x_inverse;
                        tempCubeData.z = cubeDB.cubeData[i].x;
                        tempCubeData.y_inverse = cubeDB.cubeData[i].y_inverse; // y�� ���� 
                        tempCubeData.y = cubeDB.cubeData[i].y; // y�� ����
                        tempCubeData.x_inverse = cubeDB.cubeData[i].z;
                        tempCubeData.x = cubeDB.cubeData[i].z_inverse;
                        tempCubeData.rNum = 3;
                        break;
                }

                // Prefab ���� (��� �׸��� �ʱ�ȭ�ؾ� ���� �ȳ�); ���� ������ �Ѽ� ����
                GameObject go = Instantiate(cubeDB.cubeData[i].cubePrefab, Vector3Int.down * 100, Quaternion.identity);
                tempCubeData.cubePrefab = go;
                tempCubeData.weight = cubeDB.cubeData[i].weight;
                tempCubeData.excludeFromOuter = cubeDB.cubeData[i].excludeFromOuter;
                rotationPool[i, j] = tempCubeData;
            }
        }
        allCubes = rotationPool.Cast<CubeData>().ToArray(); // 2���� �迭 -> 1���� �迭

        stud = Instantiate(stud, Vector3Int.down * 100, Quaternion.identity);
        node = Instantiate(node, Vector3Int.down * 100, Quaternion.identity);
    }

    void VoidInitialize()
    {
        voidGrid = new int[mapSize.x, mapSize.y, mapSize.z];


        // voidRoom copy
        VoidRoomData[,] VoidRoomPool;
        VoidRoomPool = new VoidRoomData[voidRoomDB.voidRoomData.Length, 4]; // 4 : xz ��� 90���� ȸ���� �� ����

        for (int i = 0; i < voidRoomDB.voidRoomData.Length; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                // �� SO �ν��Ͻ� ����; SO �� new Ű���� ��� �Ұ�
                var tempVoidRoomData = ScriptableObject.CreateInstance<VoidRoomData>();

                // ȸ������ ���� Connection ����
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

    // ��� �� �ʱ�ȭ (�ϳ��� cell �� ��� �ĺ� cube�� ������)
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

        //constrains: cell ���� ��Ģ ���� �ʴ� connection �� ���� cube ����
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

                    // �Ա�, �ⱸ ����
                }
    }

    // ���� collapse���� ���� �� �� ���� ��Ʈ���ǰ� ���� �� ��ġ ��ȯ
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

    // Cell ���� �ĺ��� �� �ϳ��� Cube�� ����ġ �ݿ��ؼ� ����
    void Collapse(Vector3Int pos)
    {
        var cell = grid[pos.x, pos.y, pos.z];

        if (cell.possibleCubes.Count == 0)
        {
            Debug.LogError($"Collapse ����: �ĺ� Cube�� �����ϴ�. ��ġ: {pos}");
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

        // Ȥ�� rand�� 0 �̻��� ��� ���
        cell.possibleCubes = new List<CubeData> { cell.possibleCubes.Last() };
    }

    // ����� �� �������� �ֺ� ���� ����
    void Propagate(Queue<Vector3Int> updateQueue)
    {

        while (updateQueue.Count > 0)
        {
            bool isConnect = false;

            Vector3Int pos = updateQueue.Dequeue();
            CubeData center = grid[pos.x, pos.y, pos.z].GetCollapsedCube();
            if (center == null) continue;

            // ���⺰ �̿� �� ������Ʈ; offset ��ġ�� target pos �� center �� ����Ǵ��� üũ
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

    // ���� �� �ĺ����� ���͸��ϰ� �پ����� ��� ���� ť�� �ٽ� ����
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




    // ���� Ȯ���� Cube�� Grid�� �ݿ�
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
                        // ��ǥ�� Cube ����
                        GameObject go = Instantiate(cube.cubePrefab, new Vector3Int(x, y, z), Quaternion.Euler(0, 90f * cube.rNum, 0));
                        //GameObject go = Instantiate(cube.cubePrefab, new Vector3Int(x, y, z), Quaternion.identity);

                        // �� ���� ����
                        CubeDecorationWithTag(go, cube);
                    }
                }

        // Generate ���� �� �˻�
        for (int z = 0; z < mapSize.z; z++)
            for (int y = 0; y < mapSize.y; y++)
                for (int x = 0; x < mapSize.x; x++)
                {
                    if (!grid[x, y, z].IsCollapsed)
                    {
                        Debug.LogWarning($"�� ({x},{y},{z}) �� collapse ���� ����. �ĺ���: {grid[x, y, z].possibleCubes.Count}");
                        GameObject go = Instantiate(cubeDB.cubeData[0].cubePrefab, new Vector3Int(x, y, z), Quaternion.identity);
                    }
                }

        
        // ����ȭ: �� ���� �Ϸ� �� Combine ����
        CombineAllMeshesByMaterial();
    }

    void CombineAllMeshesByMaterial()
    {
        // material���� MeshRenderer�� Transform ����
        Dictionary<Material, List<MeshFilter>> groups = new Dictionary<Material, List<MeshFilter>>();

        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();

        // MeshRenderer ����
        foreach (var r in renderers)
        {
            if (r.gameObject.layer != LayerMask.NameToLayer("MapUnit")) continue;

            // Marker ����
            if (tags.Any(tag => r.CompareTag(tag))) continue;

            Material mat = r.sharedMaterial;
            if (!groups.ContainsKey(mat)) groups[mat] = new List<MeshFilter>();

            var mf = r.GetComponent<MeshFilter>();
            if (mf != null) groups[mat].Add(mf);

            // ���� ������Ʈ�� ��Ȱ��ȭ (�ʿ� �� Destroy ����)
            r.gameObject.SetActive(false);
        }

        // �� Material �׷캰�� Combine
        foreach (var group in groups)
        {
            List<MeshFilter> filters = group.Value;
            CombineInstance[] combine = new CombineInstance[filters.Count]; // Mesh ��ġ��� unity ����ü

            for (int i = 0; i < filters.Count; i++)
            {
                combine[i].mesh = filters[i].sharedMesh;
                combine[i].transform = filters[i].transform.localToWorldMatrix;
            }

            // mesh ��ġ��
            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // ����뷮 �ø���: 65,535 vertex -> 4,294,967,295 vertex     
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
        MeshRenderer[] me = go.GetComponentsInChildren<MeshRenderer>(); // MarkerEdge ��������

        // Cube ���� �ٸ� local ȸ������ ������ ��� ��ġ
        Vector3 dir = cube.rNum % 2 == 0 ? Vector3.forward : Vector3.right;
        Vector3 dir2 = cube.rNum % 2 == 0 ? Vector3.right : Vector3.forward;

        foreach (var e in me)
        {
            if (e.CompareTag(tags[1])) // Stud ��ġ
            {
                if (e.transform.localPosition.y == 0.0f)
                    Instantiate(stud, e.transform.position, Quaternion.identity);
                else if (e.transform.localPosition.x == 0.0f)
                    Instantiate(stud, e.transform.position, Quaternion.Euler(dir * 90f));
                else if (e.transform.localPosition.z == 0.0f)
                    Instantiate(stud, e.transform.position, Quaternion.Euler(dir2 * 90f));
                else
                    Debug.Log("stud object ���� ����");
            }
            else if (e.CompareTag(tags[0])) // Node ��ġ
            {
                // Vertex marker�� ��� ť�꿡 �����ϹǷ� üũ �� �� �������� ��ġ���� ����
                if (Physics.OverlapSphere(e.transform.position, 0.1f).Length > 1) 
                    Instantiate(node, e.transform.position, Quaternion.identity);
            }
        }
    }

    void PaintPrefabs()
    {
        if (isPainting)
        {
            // �����տ� ���� ���̱�
            foreach (CubeData cube in allCubes)
            {
                MeshRenderer[] renderers = cube.cubePrefab.GetComponentsInChildren<MeshRenderer>();
            
                foreach (MeshRenderer r in renderers)
                {
                    // Marker ����
                    if (tags.Any(tag => r.CompareTag(tag))) continue;

                    r.transform.localScale = r.transform.localScale * scaleOffset; // ������ ����, ���� ����
                    r.material = wallpaper;
                }       
            }

            // �����ӿ� ��ĥ
            MeshRenderer mr = stud.GetComponentInChildren<MeshRenderer>();
            mr.material = studpaper;

            // ��忡 ��ĥ
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

        // �ܺ� ��ġ
        // overlap ���� ���� MarkerFace �� �ܺ� Prefab ��ġ, rNum ���

        // õ�� ���� ��ġ
        foreach (var e in mf)
        {
            if (e.name == "MarkerF_y")
            {
                LightSetup.ls.AddCeilingLight(e.transform.position);
            }
        }

        // ��� ������ ��ġ

        // ���� ������ ��ġ

        // marker �����
        if (!isVisibleMarker)
        {
            foreach (var e in me) e.SetActive(false);
            foreach (var v in mv) v.SetActive(false);
            foreach (var f in mf) f.SetActive(false);
            foreach (var p in mp) p.SetActive(false);
        }
    }


}

// ����, ���, ���� ����Ʈ, ���Ӽ�
// pathfinding