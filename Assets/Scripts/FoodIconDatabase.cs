using UnityEngine;

[System.Serializable]
public class FoodIconEntry
{
    public FoodType foodType;
    public Sprite icon;
}

public class FoodIconDatabase : MonoBehaviour
{
    public FoodIconEntry[] icons;

    public Sprite GetIcon(FoodType foodType)
    {
        foreach (FoodIconEntry entry in icons)
        {
            if (entry.foodType == foodType)
            {
                return entry.icon;
            }
        }

        Debug.LogWarning(
            "No icon found for " + foodType
        );

        return null;
    }
}