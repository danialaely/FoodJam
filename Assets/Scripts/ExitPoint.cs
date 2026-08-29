using UnityEngine;

public class ExitPoint : MonoBehaviour
{

    public Vector2Int direction = Vector2Int.up;

    [Header("Exit Settings")]
    public float exitDistance = 2f;

    [Header("Customer")]
    public Customer customer;

}
