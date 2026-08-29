using UnityEngine;
using UnityEngine.UI;

public class CustomerOrderUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Customer customer;

    [SerializeField]
    private Transform orderContainer;

    [SerializeField]
    private GameObject orderItemPrefab;

    private FoodIconDatabase iconDatabase;

    private void Start()
    {
        //iconDatabase = FindAnyObjectByType<FoodIconDatabase>();

        //RefreshOrderUI();
    }

    public void Initialize()
    {
        iconDatabase = FindAnyObjectByType<FoodIconDatabase>();

        if (iconDatabase == null)
        {
            Debug.LogError("FoodIconDatabase was not found in the scene!");
            return;
        }

        Debug.Log("CustomerOrderUI initialized successfully.");
    }

    public void RefreshOrderUI()
    {
        Debug.Log("Refreshing customer order UI");

        if (customer == null)
        {
            Debug.LogError("CustomerOrderUI: Customer reference is missing!");
            return;
        }

        if (orderContainer == null)
        {
            Debug.LogError("CustomerOrderUI: Order Container is missing!");
            return;
        }

        if (orderItemPrefab == null)
        {
            Debug.LogError("CustomerOrderUI: Order Item Prefab is missing!");
            return;
        }

        if (iconDatabase == null)
        {
            Debug.LogError("CustomerOrderUI: FoodIconDatabase is missing!");
            return;
        }

        foreach (FoodType food in customer.requestedFoods)
        {
            GameObject item =
                Instantiate(
                    orderItemPrefab,
                    orderContainer
                );

            OrderItemUI orderItemUI =
                item.GetComponent<OrderItemUI>();

            Sprite icon = iconDatabase.GetIcon(food);  //Line number 40

            orderItemUI.Setup(
                food,
                icon
            );
        }
    }

    private void OnEnable()
    {
        if (customer != null)
        {
            customer.OnFoodReceived += HandleFoodReceived;
        }
    }

    private void OnDisable()
    {
        if (customer != null)
        {
            customer.OnFoodReceived -= HandleFoodReceived;
        }
    }

    private void HandleFoodReceived(FoodType foodType)
    {
        foreach (Transform child in orderContainer)
        {
            OrderItemUI item =
                child.GetComponent<OrderItemUI>();

            if (item != null &&
                !item.IsCompleted() &&
                item.GetFoodType() == foodType)
            {
                item.MarkComplete();
                break;
            }
        }
    }
}