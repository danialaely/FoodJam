using System;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    [Header("Order")]
    public List<FoodType> requestedFoods = new List<FoodType>();

    private List<FoodType> remainingFoods =
        new List<FoodType>();

    public System.Action<FoodType> OnFoodReceived;

    public event Action<Customer> OnOrderCompleted;

    [SerializeField]
    private CustomerOrderUI orderUI;

    [Header("Customer Settings")]
    [SerializeField]
    private float patienceTime = 30f;

    private float currentPatience;

    private void Start()
    {
        Debug.Log( "Customer spawned. Waiting for order." );
        currentPatience = patienceTime;
    }

    public void SetServingState(bool isServing)
    {
        // Temporary visual feedback.
        // We will replace this with proper animation/UI later.

        if (isServing)
        {
            Debug.Log(
                name + " is now the serving customer."
            );
        }
    }

    public List<FoodType> GetRemainingFoods()
    {
        return new List<FoodType>(remainingFoods);
    }

    public void SetOrder(List<FoodType> newOrder)
    {
        requestedFoods = new List<FoodType>(newOrder);
        remainingFoods = new List<FoodType>(requestedFoods);

        orderUI.Initialize();
        orderUI.RefreshOrderUI();
    }

    public bool ReceiveFood(FoodPiece food)
    {
        if (remainingFoods.Contains(food.foodType))
        {
            remainingFoods.Remove(food.foodType);

            OnFoodReceived?.Invoke(food.foodType);

            Debug.Log(
                "Correct food received: "
                + food.foodType
            );

            Debug.Log(
                "Remaining items: "
                + remainingFoods.Count
            );

            if (remainingFoods.Count == 0)
            {
                CompleteOrder();
            }

            return true;
        }

        Debug.Log(
            "Wrong food! Customer does not need: "
            + food.foodType
        );

        return false;
    }

    private void CompleteOrder()
    {
        Debug.Log("ORDER COMPLETE!");

        OnOrderCompleted?.Invoke(this);
    }

    public void LeaveCustomer(Vector3 exitPosition)
    {
        StartCoroutine(LeaveRoutine(exitPosition));
    }

    private System.Collections.IEnumerator LeaveRoutine(
    Vector3 exitPosition
)
    {
        float duration = 1f;
        float elapsed = 0f;

        Vector3 startPosition = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    exitPosition,
                    t
                );

            yield return null;
        }

        transform.position = exitPosition;

        Destroy(gameObject);
    }

    public void MoveTo(Vector3 targetPosition)
    {
        StartCoroutine(
            MoveToRoutine(targetPosition)
        );
    }

    private System.Collections.IEnumerator MoveToRoutine(
    Vector3 targetPosition
)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        Vector3 startPosition = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }

        transform.position = targetPosition;
    }
}
