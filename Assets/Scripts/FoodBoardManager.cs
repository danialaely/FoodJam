using System.Collections.Generic;
using UnityEngine;

public class FoodBoardManager : MonoBehaviour
{
    [SerializeField]
    private CustomerQueue customerQueue;

    [SerializeField]
    private CustomerManager customerManager;

    private FoodPiece[] foodPieces;
    private Customer currentServingCustomer;

    public bool IsFoodNeeded(FoodType foodType)
    {
        List<FoodType> requiredFoods =
            customerQueue.GetServingCustomerOrder();

        return requiredFoods.Contains(foodType);
    }

    private void Start()
    {
        foodPieces = FindObjectsByType<FoodPiece>();

        Invoke(nameof(RefreshFoodHighlights), 0.5f);
        Invoke(nameof(TestFoodRequirement), 0.5f);
    }

    private void OnEnable()
    {
        if (customerManager != null)
        {
            customerManager.OnServingCustomerChanged +=
                HandleServingCustomerChanged;
        }
    }

    private void OnDisable()
    {
        if (customerManager != null)
        {
            customerManager.OnServingCustomerChanged -=
                HandleServingCustomerChanged;
        }

        UnsubscribeFromCustomer();
    }

    private void HandleServingCustomerChanged(Customer newCustomer)
    {
        UnsubscribeFromCustomer();

        currentServingCustomer = newCustomer;

        if (currentServingCustomer != null)
        {
            currentServingCustomer.OnFoodReceived +=
                HandleFoodReceived;
        }

        RefreshFoodHighlights();
    }

    private void HandleFoodReceived(FoodType foodType)
    {
        RefreshFoodHighlights();
    }

    private void UnsubscribeFromCustomer()
    {
        if (currentServingCustomer != null)
        {
            currentServingCustomer.OnFoodReceived -=
                HandleFoodReceived;
        }

        currentServingCustomer = null;
    }

    public void RefreshFoodHighlights()
    {
        if (customerQueue == null)
        {
            Debug.LogWarning("CustomerQueue is not assigned.");
            return;
        }

        foreach (FoodPiece food in foodPieces)
        {
            bool needed =
                IsFoodNeeded(food.GetFoodType());

            food.SetHighlighted(needed);
        }
    }

    private void TestFoodRequirement()
    {
        Debug.Log("===== TESTING CURRENT CUSTOMER ORDER =====");

        TestFood(FoodType.Burger);
        TestFood(FoodType.Fries);
        TestFood(FoodType.Pizza);
    }

    private void TestFood(FoodType foodType)
    {
        bool needed = IsFoodNeeded(foodType);

        Debug.Log(
            "Food: " +
            foodType +
            " | Needed: " +
            needed
        );
    }
}