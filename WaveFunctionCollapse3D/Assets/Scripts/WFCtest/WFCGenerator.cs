using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

// WFC �� �ϳ��� ���� (������ Ÿ�� �ĺ��� ����)
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

// TileData�� Unity���� ��� ������ Tile�� ��ȯ
public class ScriptableTile : Tile
{
    public static ScriptableTile From(TileData data)
    {
        var tile = ScriptableObject.CreateInstance<ScriptableTile>();
        tile.sprite = data.sprite;
        return tile;
    }
}

// WFC �˰��� ���� Ŭ����
public class WFCGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public TileDatabase database;
    public int mapWidth = 10;
    public int mapHeight = 10;

    private WFCGridCell[,] grid;

    // Unity �ν����Ϳ��� ȣ�� ������ ��ư
    [ContextMenu("Generate Map")]
    public void Generate()
    {
        InitializeGrid(); // �׸��� �ʱ�ȭ

        Queue<Vector2Int> updateQueue = new Queue<Vector2Int>(); // ���� ť ����

        while (true)
        {
            var cellPos = FindLowestEntropyCell(); // ���� ���� ��Ʈ���� �� ����
            if (cellPos == null) break;

            Collapse(cellPos.Value); // �� Ȯ��
            updateQueue.Enqueue(cellPos.Value);
            Propagate(updateQueue); // ���� ����
        }

        ApplyToTilemap(); // ��� ����
    }

    // ��� �� �ʱ�ȭ (��� �ĺ� Ÿ���� ������)
    void InitializeGrid()
    {
        grid = new WFCGridCell[mapWidth, mapHeight];
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                grid[x, y] = new WFCGridCell(database.tiles);
    }

    // ���� collapse���� ���� �� �� ���� ��Ʈ���ǰ� ���� �� ��ġ ��ȯ
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

    // �ĺ��� �� �ϳ��� Ÿ���� ����ġ ������� �����ϰ� collapse
    void Collapse(Vector2Int pos)
    {
        var cell = grid[pos.x, pos.y];
        
        if (cell.possibleTiles.Count == 0)
        {
            Debug.LogError($"Collapse ����: �ĺ� Ÿ���� �����ϴ�. ��ġ: {pos}");
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

        // Ȥ�� rand�� 0 �̻��� ��� ���
        cell.possibleTiles = new List<TileData> { cell.possibleTiles.Last() };
    }

    // ����� �� �������� �ֺ� ���� ����
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

            // ���⺰ �̿� �� ������Ʈ
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

    // ���� �� �ĺ����� ���͸��ϰ� �پ����� ��� ���� ť�� �ٽ� ����
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

    // ���� Ȯ���� Ÿ���� Tilemap�� �ð������� �ݿ�
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
                //    // collapse ���� �ʾ����� �ĺ��� �������� ��� ������ ����
                //    tile = grid[x, y].possibleTiles[Random.Range(0, grid[x, y].possibleTiles.Count)];
                //    Debug.LogWarning($"�� ({x},{y}) ���Ƿ� ä��� : {tile.name}");
                //}


                if (tile != null)
                {
                    var t = ScriptableTile.From(tile);
                    tilemap.SetTile(new Vector3Int(x, y, 0), t);
                }
            }
        }

        // Generate ���� �� �˻�
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                if (!grid[x, y].IsCollapsed)
                    Debug.LogWarning($"�� ({x},{y}) �� collapse ���� ����. �ĺ���: {grid[x, y].possibleTiles.Count}");
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
            generator.Generate(); // ��ư Ŭ�� �� Generate �޼��� ����
        }
    }
}
