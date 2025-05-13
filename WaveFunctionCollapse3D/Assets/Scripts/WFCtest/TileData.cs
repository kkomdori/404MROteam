using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ÿ�� �� ���� Ÿ�� ����
public enum TileConnection
{
    Air,
    Ground,

}

// ���� Ÿ�� ������ ���� (ScriptableObject�� ����)
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
