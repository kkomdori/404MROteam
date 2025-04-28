using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 전체 타일 목록을 저장하는 데이터베이스
[CreateAssetMenu(fileName = "TileDatabase", menuName = "WFC/TileDatabase")]
public class TileDatabase : ScriptableObject
{
    public TileData[] tiles;
}
