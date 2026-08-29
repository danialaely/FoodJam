using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public event System.Action<Customer> OnServingCustomerChanged;

    [Header("Customer Setup")]
    [SerializeField]
    private Customer customerPrefab;

    [SerializeField]
    private Transform customerSpawnPoint;

    private Customer currentCustomer;

    [SerializeField]
    private ExitPoint exitPoint;

    [SerializeField]
    private Transform customerExitPoint;

    //private int customerNumber = 0;
    private int currentOrderIndex = 0;

    [SerializeField]
    private LevelData currentLevel;

    [SerializeField]
    private CustomerQueue customerQueue;

    private void Start()
    {
        FillQueue();
    }

    private void FillQueue()
    {
        while (!customerQueue.IsFull())
        {
            if (currentOrderIndex >=
                currentLevel.customerOrders.Count)
            {
                Debug.Log("Level orders completed.");
                Debug.Log("🎉 LEVEL COMPLETE!");
                return;
            }

            SpawnCustomer();
        }
    }

    public void SpawnCustomer()
    {
        if (customerQueue.IsFull())
        {
            Debug.Log("Queue is full. Cannot spawn customer.");
            return;
        }

        currentCustomer = Instantiate(
            customerPrefab,
            customerSpawnPoint.position,
            customerSpawnPoint.rotation
        );

        List<FoodType> order = GetNextOrder();

        currentCustomer.SetOrder(order);

        currentCustomer.OnOrderCompleted += HandleOrderCompleted;

        customerQueue.AddCustomer(currentCustomer);

        UpdateServingCustomer();
    }

    private void UpdateServingCustomer()
    {
        Customer servingCustomer = customerQueue.GetServingCustomer();

        if (servingCustomer == null)
        {
            exitPoint.customer = null;
            return;
        }

        exitPoint.customer = servingCustomer;

        Debug.Log("Serving customer updated: "+ servingCustomer.name);
        OnServingCustomerChanged?.Invoke(servingCustomer);
    }

    private List<FoodType> GetNextOrder()
    {
        if (currentLevel == null)
        {
            Debug.LogError("No LevelData assigned!");
            return new List<FoodType>();
        }

        if (currentOrderIndex >=
            currentLevel.customerOrders.Count)
        {
            Debug.Log(
                "All customer orders in this level are complete."
            );

            return new List<FoodType>();
        }

        List<FoodType> order =
            currentLevel.customerOrders[currentOrderIndex].foods;

        currentOrderIndex++;

        return new List<FoodType>(order);
    }

    private void HandleOrderCompleted(Customer completedCustomer)
    {
        StartCoroutine(
            CustomerCompletedRoutine(completedCustomer)
        );
    }

    private IEnumerator CustomerCompletedRoutine(
    Customer completedCustomer
)
    {
        yield return new WaitForSeconds(1f);

        customerQueue.RemoveCustomer(
            completedCustomer
        );

        completedCustomer.LeaveCustomer(
            customerExitPoint.position
        );

        UpdateServingCustomer();

        yield return new WaitForSeconds(0.5f);

        FillQueue();
    }
}