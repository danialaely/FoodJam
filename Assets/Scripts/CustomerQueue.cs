using System.Collections.Generic;
using UnityEngine;

public class CustomerQueue : MonoBehaviour
{
    
    [Header("Queue Settings")]
    [SerializeField]
    private int maxCustomers = 3;

    [SerializeField]
    private Transform[] queuePositions;

    private List<Customer> customers =
        new List<Customer>();

    public bool IsFull()
    {
        return customers.Count >= maxCustomers;
    }

    public void AddCustomer(Customer customer)
    {
        if (IsFull())
        {
            Debug.Log("Customer queue is full.");
            return;
        }

        customers.Add(customer);

        UpdateQueuePositions();
    }

    public Customer GetServingCustomer()
    {
        if (customers.Count == 0)
        {
            return null;
        }

        return customers[0];
    }

    public List<FoodType> GetServingCustomerOrder()
    {
        Customer servingCustomer = GetServingCustomer();

        if (servingCustomer == null)
        {
            return new List<FoodType>();
        }

        return servingCustomer.GetRemainingFoods();
    }

    public void RemoveCustomer(Customer customer)
    {
        if (!customers.Contains(customer))
        {
            return;
        }

        customers.Remove(customer);

        UpdateQueuePositions();
    }

    private void UpdateQueuePositions()
    {
        for (int i = 0; i < customers.Count; i++)
        {
            if (i < queuePositions.Length)
            {
                customers[i].MoveTo(
                    queuePositions[i].position
                );

                customers[i].SetServingState(i == 0);
            }
        }
    }
}