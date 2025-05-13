using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// ������ �� y ����� rotation �����Ͽ� 3���� ���� ������ �߰� ����
// ������ ȸ���� �Բ� enum value �� ���� ȸ��. ��) x -> z, z -> xi, xi -> zi
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
    // path ���� Ȯ��

    // y���� ������ ȸ���Ͽ� 
    [Header ("Connection")]
    public Connection x;
    public Connection x_inverse;
    public Connection y;
    public Connection y_inverse;
    public Connection z;
    public Connection z_inverse;
}
