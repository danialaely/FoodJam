using UnityEngine;

public class FoodPiece : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField]
    private GameObject highlightObject;

    [Header("Food")]
    public FoodType foodType;

    [Header("Grid Size")]
    public Vector2Int size = new Vector2Int(1, 1);

    [Header("Grid Position")]
    public Vector2Int gridPosition;

    [Header("Food Board")]
    [SerializeField]
    private FoodBoardManager foodBoardManager;

    private bool isHighlighted;
    private bool isSelected;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Start()
    {
       // TestFoodRequirement();
    }

    public void SetHighlighted(bool highlighted)
    {
        /*if (highlightObject != null)
        {
            highlightObject.SetActive(highlighted);
        }*/
        isHighlighted = highlighted;

        UpdateVisualState();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selected)
        {
            transform.localScale = originalScale * 1.05f;
        }
        else
        {
            transform.localScale = originalScale;
        }

        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(
                isHighlighted || isSelected
            );
        }
    }

    public FoodType GetFoodType()
    {
        return foodType;
    }

    /* private void TestFoodRequirement()
     {
         if (foodBoardManager == null)
         {
             Debug.LogWarning(
                 name + ": FoodBoardManager is not assigned."
             );

             return;
         }

         bool needed =
             foodBoardManager.IsFoodNeeded(foodType);

         Debug.Log(
             name +
             " | Food: " +
             foodType +
             " | Needed: " +
             needed
         );
     }*/

}