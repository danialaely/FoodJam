using System;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    [Header("Order")]
    public List<FoodType> requestedFoods = new List<FoodType>();

    private List<FoodType> remainingFoods = new List<FoodType>();

    public System.Action<FoodType> OnFoodReceived;

    public event Action<Customer> OnOrderCompleted;

    [SerializeField]
    private CustomerOrderUI orderUI;

    [Header("Customer Settings")]
    [SerializeField]
    private float patienceTime = 30f;
    
    [SerializeField]
    private float sadThreshold = 0.5f;

    [SerializeField]
    private float angryThreshold = 0.8f;
    private float currentPatience;
    private bool orderCompleted;

    private bool isServing;

    private float happyReactionTimer = 0f;

    [SerializeField]
    private float happyReactionDuration = 1.5f;

    [SerializeField]
    private CustomerAnimationController animationController;

    [SerializeField]
    private CustomerExpressionController expressionController;

    private void Start()
    {
        Debug.Log( "Customer spawned. Waiting for order." );
        currentPatience = patienceTime;
    }

    private void Update()
    {
        UpdatePatience();
    }

    private void UpdatePatience()
    {
        if (!isServing)
            return;

        if (orderCompleted)
            return;

        if (happyReactionTimer > 0f)
        {
            happyReactionTimer -= Time.deltaTime;
            return;
        }

        if (currentPatience <= 0f)
            return;

        currentPatience -= Time.deltaTime;

        float patiencePercentage =
            currentPatience / patienceTime;

        if (patiencePercentage <= 0.2f)
        {
            expressionController.SetExpression(
                CustomerExpressionController.Expression.Angry
            );
        }
        else if (patiencePercentage <= 0.5f)
        {
            expressionController.SetExpression(
                CustomerExpressionController.Expression.Sad
            );
        }
        else
        {
            expressionController.SetExpression(
                CustomerExpressionController.Expression.Happy
            );
        }
    }

    public void SetServingState(bool isServing)
    {
        this.isServing = isServing;

        if (isServing)
        {
            currentPatience = patienceTime;

            expressionController.SetExpression(
                CustomerExpressionController.Expression.Happy
            );

            Debug.Log(
                name + " is now the serving customer."
            );
        }
        else
        {
            expressionController.SetExpression(
                CustomerExpressionController.Expression.Happy
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
                "Correct food received: " + food.foodType
            );

            Debug.Log(
                "Remaining items: " + remainingFoods.Count
            );

            if (remainingFoods.Count > 0)
            {
                happyReactionTimer = happyReactionDuration;

                expressionController.SetExpression(
                    CustomerExpressionController.Expression.Happy
                );
            }
            else
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
        orderCompleted = true;

        Debug.Log("ORDER COMPLETE!");

        expressionController.SetExpression(
            CustomerExpressionController.Expression.Celebration
        );

        animationController.PlayCelebration();

        OnOrderCompleted?.Invoke(this);
    }

    public void LeaveCustomer(Vector3 exitPosition)
    {
        animationController.SetWalking(true);
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
        animationController.SetWalking(true);

        StartCoroutine(MoveToRoutine(targetPosition));
    }

    private System.Collections.IEnumerator MoveToRoutine(Vector3 targetPosition)
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
        animationController.SetWalking(false);
    }
}
