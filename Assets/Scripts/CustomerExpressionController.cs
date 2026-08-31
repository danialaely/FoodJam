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
        SetExpression(Expression.Sad);
    }

    public void SetExpression(Expression expression)
    {
        switch (expression)
        {
            case Expression.Happy:
                characterRenderer.material = happyFace;
                break;

            case Expression.Sad:
                characterRenderer.material = sadFace;
                break;

            case Expression.Angry:
                characterRenderer.material = angryFace;
                break;

            case Expression.Celebration:
                characterRenderer.material = celebrationFace;
                break;
        }
    }
}