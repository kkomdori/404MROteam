using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

public enum DoorDirection
{
    Forward,
    Backward,
    Left,
    Right,
    Up,
    Down
}

[CreateAssetMenu(fileName = "VoidRoomData", menuName = "WFCforCube/VoidRoomData")]
public class VoidRoomData : ScriptableObject
{

    [System.Serializable]
    public class DoorInfo
    {
        public Vector3Int position;
        public DoorDirection direction;
        public Connection connection;
    }

    public GameObject voidRoomPrefab;
    public int weight = 1;

    [Header("RoomTransforms")]
    public Vector3Int roomStartPosition; 
    public Vector3Int roomScale;
    public bool isRandomLocation = false;
    public bool isRotatable = false;
    public int rNum = 0; // rotation number

    public List<DoorInfo> doorPositions = new List<DoorInfo>();
}
