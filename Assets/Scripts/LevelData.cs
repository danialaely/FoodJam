using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "LevelData",
    menuName = "Food Sort/Level Data"
)]
public class LevelData : ScriptableObject
{
    public List<CustomerOrderData> customerOrders =
        new List<CustomerOrderData>();
}