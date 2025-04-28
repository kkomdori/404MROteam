using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

// WFC 셀 하나의 상태 (가능한 타일 후보군 보유)
public class WFCGridCell
{
    public List<TileData> possibleTiles;
    public bool IsCollapsed => possibleTiles.Count == 1;

    public WFCGridCell(TileData[] allTiles)
    {
        possibleTiles = new List<TileData>(allTiles);
    }

    public TileData GetCollapsedTile() => IsCollapsed ? possibleTiles[0] : null;
}

// TileData를 Unity에서 사용 가능한 Tile로 변환
public class ScriptableTile : Tile
{
    public static ScriptableTile From(TileData data)
    {
        var tile = ScriptableObject.CreateInstance<ScriptableTile>();
        tile.sprite = data.sprite;
        return tile;
    }
}

// WFC 알고리즘 실행 클래스
public class WFCGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public TileDatabase database;
    public int mapWidth = 10;
    public int mapHeight = 10;

    private WFCGridCell[,] grid;

    // Unity 인스펙터에서 호출 가능한 버튼
    [ContextMenu("Generate Map")]
    public void Generate()
    {
        InitializeGrid(); // 그리드 초기화

        Queue<Vector2Int> updateQueue = new Queue<Vector2Int>(); // 전파 큐 생성

        while (true)
        {
            var cellPos = FindLowestEntropyCell(); // 가장 낮은 엔트로피 셀 선택
            if (cellPos == null) break;

            Collapse(cellPos.Value); // 셀 확정
            updateQueue.Enqueue(cellPos.Value);
            Propagate(updateQueue); // 변경 전파
        }

        ApplyToTilemap(); // 결과 적용
    }

    // 모든 셀 초기화 (모든 후보 타일을 갖도록)
    void InitializeGrid()
    {
        grid = new WFCGridCell[mapWidth, mapHeight];
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                grid[x, y] = new WFCGridCell(database.tiles);
    }

    // 아직 collapse되지 않은 셀 중 가장 엔트로피가 낮은 셀 위치 반환
    Vector2Int? FindLowestEntropyCell()
    {
        Vector2Int? lowestPos = null;
        int minEntropy = int.MaxValue;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                var cell = grid[x, y];
                if (cell.IsCollapsed || cell.possibleTiles.Count == 0) continue;

                int entropy = cell.possibleTiles.Count;
                if (entropy < minEntropy)
                {
                    minEntropy = entropy;
                    lowestPos = new Vector2Int(x, y);
                }
            }
        }
        return lowestPos;
    }

    // 후보군 중 하나의 타일을 가중치 기반으로 선택하고 collapse
    void Collapse(Vector2Int pos)
    {
        var cell = grid[pos.x, pos.y];
        
        if (cell.possibleTiles.Count == 0)
        {
            Debug.LogError($"Collapse 실패: 후보 타일이 없습니다. 위치: {pos}");
            return;
        }

        int totalWeight = cell.possibleTiles.Sum(t => t.weight);
        int rand = Random.Range(0, totalWeight);
        foreach (var tile in cell.possibleTiles)
        {
            rand -= tile.weight;
            if (rand <= 0)
            {
                cell.possibleTiles = new List<TileData> { tile };
                return;
            }
        }

        // 혹시 rand가 0 이상일 경우 대비
        cell.possibleTiles = new List<TileData> { cell.possibleTiles.Last() };
    }

    // 변경된 셀 기준으로 주변 셀에 전파
    void Propagate(Queue<Vector2Int> updateQueue)
    {
        while (updateQueue.Count > 0)
        {
            bool isConnectDonor = false;
            bool isConnectAcceptor = false;
            bool[] isPass = new bool[4] { true, true, true, true };

            var pos = updateQueue.Dequeue();
            var center = grid[pos.x, pos.y].GetCollapsedTile();
            if (center == null) continue;

            // 방향별 이웃 셀 업데이트
            isConnectDonor = ConnectCheck(pos, Vector2Int.up, center.upD, (t, c) => t.downA == c);
            isConnectAcceptor = ConnectCheck(pos, Vector2Int.up, center.upA, (t, c) => t.downD == c);
            if (isConnectDonor && isConnectAcceptor)
                UpdateNeighborCell(pos, Vector2Int.up, updateQueue);
            //else isPass[0] = false;

            isConnectDonor = ConnectCheck(pos, Vector2Int.down, center.downD, (t, c) => t.upA == c);
            isConnectDonor = ConnectCheck(pos, Vector2Int.down, center.downA, (t, c) => t.upD == c);
            if (isConnectDonor && isConnectAcceptor)
                UpdateNeighborCell(pos, Vector2Int.down, updateQueue);
            //else isPass[1] = false;

            isConnectDonor = ConnectCheck(pos, Vector2Int.left, center.leftD, (t, c) => t.rightA == c);
            isConnectDonor = ConnectCheck(pos, Vector2Int.left, center.leftA, (t, c) => t.rightD == c);
            if (isConnectDonor && isConnectAcceptor)
                UpdateNeighborCell(pos, Vector2Int.left, updateQueue);
            //else isPass[2] = false;

            isConnectDonor = ConnectCheck(pos, Vector2Int.right, center.rightD, (t, c) => t.leftA == c);
            isConnectDonor = ConnectCheck(pos, Vector2Int.right, center.rightA, (t, c) => t.leftD == c);
            if (isConnectDonor && isConnectAcceptor)
                UpdateNeighborCell(pos, Vector2Int.right, updateQueue);
            //else isPass[3] = false;
        }
    }

    void UpdateNeighborCell(Vector2Int centerPos, Vector2Int offset, Queue<Vector2Int> queue)
    {
        Vector2Int nPos = centerPos + offset;
        queue.Enqueue(nPos);
    }

    // 인접 셀 후보군을 필터링하고 줄어들었을 경우 전파 큐에 다시 넣음
    bool ConnectCheck(Vector2Int centerPos, Vector2Int offset, TileConnection connection, System.Func<TileData, TileConnection, bool> predicate)
    {
        Vector2Int nPos = centerPos + offset;
        if (nPos.x < 0 || nPos.y < 0 || nPos.x >= mapWidth || nPos.y >= mapHeight) return false;

        var neighbor = grid[nPos.x, nPos.y];
        if (neighbor.IsCollapsed) return false;

        int before = neighbor.possibleTiles.Count;
        neighbor.possibleTiles = neighbor.possibleTiles.Where(t => predicate(t, connection)).ToList();

        if (neighbor.possibleTiles.Count != before)
        {
            return true;
        }

        //Debug.Log($"ConnectionFail : {nPos}, Before : {before}, After : {neighbor.possibleTiles.Count}" );

        return false;
    }

    // 최종 확정된 타일을 Tilemap에 시각적으로 반영
    void ApplyToTilemap()
    {
        tilemap.ClearAllTiles();

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                TileData tile = grid[x, y].GetCollapsedTile();

                //if (tile == null && !grid[x, y].IsCollapsed)
                //{
                //    // collapse 되지 않았지만 후보가 남아있을 경우 무작위 선택
                //    tile = grid[x, y].possibleTiles[Random.Range(0, grid[x, y].possibleTiles.Count)];
                //    Debug.LogWarning($"셀 ({x},{y}) 임의로 채우기 : {tile.name}");
                //}


                if (tile != null)
                {
                    var t = ScriptableTile.From(tile);
                    tilemap.SetTile(new Vector3Int(x, y, 0), t);
                }
            }
        }

        // Generate 끝난 뒤 검사
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                if (!grid[x, y].IsCollapsed)
                    Debug.LogWarning($"셀 ({x},{y}) 가 collapse 되지 않음. 후보수: {grid[x, y].possibleTiles.Count}");
    }
}

[CustomEditor(typeof(WFCGenerator))]
public class WFCGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WFCGenerator generator = (WFCGenerator)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Map"))
        {
            generator.Generate(); // 버튼 클릭 시 Generate 메서드 실행
        }
    }
}
