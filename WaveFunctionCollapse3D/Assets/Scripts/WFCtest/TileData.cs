using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 타일 간 연결 타입 정의
public enum TileConnection
{
    Air,
    Ground,

}

// 개별 타일 데이터 구조 (ScriptableObject로 관리)
[CreateAssetMenu(fileName = "TileData", menuName = "WFC/TileData")]
public class TileData : ScriptableObject
{
    public string tileName;
    public Sprite sprite;
    public int weight = 1;

    [Header("TileConnection")]

    public TileConnection upD;
    public TileConnection upA;
    [Space(5)]

    public TileConnection downD;
    public TileConnection downA;
    [Space(5)]

    public TileConnection leftD;
    public TileConnection leftA;
    [Space(5)]

    public TileConnection rightD;
    public TileConnection rightA;
}
