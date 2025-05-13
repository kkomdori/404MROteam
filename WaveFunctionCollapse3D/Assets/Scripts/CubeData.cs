using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// 프리팹 당 y 축기준 rotation 적용하여 3개의 변형 프리팹 추가 생성
// 프리팹 회전과 함께 enum value 도 같이 회전. 예) x -> z, z -> xi, xi -> zi
public enum Connection
{
    wall,
    air,
    path,
    window,
    C1,
    C2,
    C3,
}

[CreateAssetMenu(fileName = "CubeData", menuName = "WFCforCube/CubeData")]
public class CubeData : ScriptableObject
{
    public GameObject cubePrefab;
    public int weight = 1;
    public int rNum = 0; // rotation number

    [Header("Constrains")]
    public bool excludeFromOuter;
    // path 연결 확률

    // y축을 기준을 회전하여 
    [Header ("Connection")]
    public Connection x;
    public Connection x_inverse;
    public Connection y;
    public Connection y_inverse;
    public Connection z;
    public Connection z_inverse;
}
