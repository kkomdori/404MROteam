using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ��ü Ÿ�� ����� �����ϴ� �����ͺ��̽�
[CreateAssetMenu(fileName = "TileDatabase", menuName = "WFC/TileDatabase")]
public class TileDatabase : ScriptableObject
{
    public TileData[] tiles;
}
