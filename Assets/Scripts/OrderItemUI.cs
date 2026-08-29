using UnityEngine;
using UnityEngine.UI;

public class OrderItemUI : MonoBehaviour
{
    [SerializeField]
    private Image foodIcon;

    [SerializeField]
    private Image checkMark;

    private FoodType foodType;

    private bool isCompleted = false;

    public FoodType GetFoodType()
    {
        return foodType;
    }

    public void Setup(
        FoodType type,
        Sprite icon
    )
    {
        foodType = type;

        foodIcon.sprite = icon;

        checkMark.gameObject.SetActive(false);
    }

    public void MarkComplete()
    {
        if (isCompleted)
            return;

        isCompleted = true;

        foodIcon.gameObject.SetActive(false);
        checkMark.gameObject.SetActive(true);
    }

    public bool IsCompleted()
    {
        return isCompleted;
    }
}