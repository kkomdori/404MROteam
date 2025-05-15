using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// WFC �� �ϳ��� ���� (������ Ÿ�� �ĺ��� ����)
public class Cell
{
    public List<CubeData> possibleCubes; // �ĺ��� cube �����

    // collapse ���� ����
    public bool IsCollapsed => possibleCubes.Count == 1;

    public Cell(CubeData[] allCubes)
    {
        possibleCubes = new List<CubeData>(allCubes);
    }

    public CubeData GetCollapsedCube() => IsCollapsed ? possibleCubes[0] : null;
}