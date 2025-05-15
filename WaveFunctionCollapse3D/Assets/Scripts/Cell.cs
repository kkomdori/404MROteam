using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// WFC 셀 하나의 상태 (가능한 타일 후보군 보유)
public class Cell
{
    public List<CubeData> possibleCubes; // 후보군 cube 저장소

    // collapse 상태 저장
    public bool IsCollapsed => possibleCubes.Count == 1;

    public Cell(CubeData[] allCubes)
    {
        possibleCubes = new List<CubeData>(allCubes);
    }

    public CubeData GetCollapsedCube() => IsCollapsed ? possibleCubes[0] : null;
}