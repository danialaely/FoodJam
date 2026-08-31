using UnityEngine;

public class CustomerExpressionController : MonoBehaviour
{
    public enum Expression
    {
        Happy,
        Sad,
        Angry,
        Celebration
    }

    [Header("Character Mesh")]
    [SerializeField]
    private Renderer characterRenderer;

    [Header("Face Material Index")]
    [SerializeField]
    private int faceMaterialIndex = 1;

    [Header("Face Materials")]
    [SerializeField]
    private Material happyFace;

    [SerializeField]
    private Material sadFace;

    [SerializeField]
    private Material angryFace;

    [SerializeField]
    private Material celebrationFace;

    private void Start()
    {
        SetExpression(Expression.Happy);
    }

    public void SetExpression(Expression expression)
    {
        Material faceMaterial = null;

        switch (expression)
        {
            case Expression.Happy:
                faceMaterial = happyFace;
                break;

            case Expression.Sad:
                faceMaterial = sadFace;
                break;

            case Expression.Angry:
                faceMaterial = angryFace;
                break;

            case Expression.Celebration:
                faceMaterial = celebrationFace;
                break;
        }

        if (faceMaterial == null)
        {
            Debug.LogWarning("Face material is not assigned!");
            return;
        }

        // Get all material slots
        Material[] materials = characterRenderer.materials;

        // Make sure the index exists
        if (faceMaterialIndex < 0 || faceMaterialIndex >= materials.Length)
        {
            Debug.LogError(
                $"Face Material Index {faceMaterialIndex} is invalid. " +
                $"Renderer has {materials.Length} material slots."
            );
            return;
        }

        // Change ONLY the face material
        materials[faceMaterialIndex] = faceMaterial;

        // Apply the updated material array
        characterRenderer.materials = materials;
    }
}