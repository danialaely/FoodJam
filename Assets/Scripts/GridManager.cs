using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 7;
    public int height = 9;
    public float cellSize = 1f;

    [Header("Grid Position")]
    public Vector3 gridOrigin;

    [SerializeField]
    private ExitPoint exitPoint;

    private FoodPiece[,] grid;
    private FoodPiece selectedPiece;

    private Vector2 dragStartPosition;

    private bool isDragging;

    [SerializeField]
    private float minimumSwipeDistance = 50f;


    private Vector3 dragStartWorldPosition;
    private Vector2Int dragStartGridPosition;

    private Vector3 currentDragPosition;

    private bool isDraggingPiece;
    private Vector3 dragOffset;
    private Camera mainCamera;

    [SerializeField]
    private LayerMask foodLayer;

    [SerializeField]
    private float dragMoveSpeed = 15f;

    private void Awake()
    {
        grid = new FoodPiece[width, height];
    }

    private void Start()
    {
        RegisterAllFoodPieces();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleSelection();
        //HandleKeyboardMovement();
        //HandleSwipe();
        HandleDragMovement();
    }

    private void HandleSelection()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("No Main Camera found!");
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = cam.ScreenPointToRay(mousePosition);

        RaycastHit[] hits = Physics.RaycastAll(ray);

        Debug.Log("Raycast hits: " + hits.Length);

        FoodPiece closestFood = null;
        float closestDistance = Mathf.Infinity;

        foreach (RaycastHit hit in hits)
        {
            FoodPiece piece =
                hit.collider.GetComponentInParent<FoodPiece>();

            if (piece != null && hit.distance < closestDistance)
            {
                closestFood = piece;
                closestDistance = hit.distance;
            }
        }

        if (closestFood != null)
        {
            // Deselect the previous food
            if (selectedPiece != null && selectedPiece != closestFood)
            {
                selectedPiece.SetSelected(false);
            }

            // Select the new food
            selectedPiece = closestFood;
            selectedPiece.SetSelected(true);

            Debug.Log("SELECTED FOOD: " + selectedPiece.name );
        }
        else
        {
            Debug.Log("No FoodPiece found under mouse.");
        }
    }

    private void HandleDragMovement()
    {
        if (selectedPiece == null)
            return;

        if (Mouse.current == null || mainCamera == null)
            return;

        // START DRAG
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            BeginDrag();
        }

        // CONTINUE DRAG
        if (isDraggingPiece &&
            Mouse.current.leftButton.isPressed)
        {
            DragPiece();
        }

        // END DRAG
        if (isDraggingPiece &&
            Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }

    private void BeginDrag()
    {
        isDraggingPiece = true;

        dragStartGridPosition =
            selectedPiece.gridPosition;

        Vector3 mouseWorldPosition =
            GetMouseWorldPosition();

        dragOffset =
            selectedPiece.transform.position -
            mouseWorldPosition;

        UnregisterPiece(selectedPiece);
    }

    private void DragPiece()
    {
        Vector3 mouseWorldPosition =
            GetMouseWorldPosition();

        Vector3 desiredPosition =
            mouseWorldPosition + dragOffset;

        desiredPosition =
            ClampToGridBounds(
                selectedPiece,
                desiredPosition
            );

        Vector3 currentPosition =
            selectedPiece.transform.position;

        Vector3 movement =
            desiredPosition - currentPosition;

        Vector3 constrainedMovement =
            GetConstrainedMovement(
                selectedPiece,
                currentPosition,
                movement
            );

        selectedPiece.transform.position =
            currentPosition + constrainedMovement;
    }

    private void EndDrag()
    {
        isDraggingPiece = false;

        Vector2Int nearestGridPosition =
            GetGridPosition(
                selectedPiece.transform.position,
                selectedPiece.size
            );

        // Check whether the nearest grid position is valid.
        if (CanMoveTo(
            selectedPiece,
            nearestGridPosition))
        {
            selectedPiece.gridPosition =
                nearestGridPosition;

            selectedPiece.transform.position =
                GetPieceWorldPosition(
                    nearestGridPosition,
                    selectedPiece.size
                );
        }
        else
        {
            // Fall back to its previous valid position.
            selectedPiece.gridPosition =
                dragStartGridPosition;

            selectedPiece.transform.position =
                GetPieceWorldPosition(
                    dragStartGridPosition,
                    selectedPiece.size
                );
        }

        RegisterPieceAtPosition(selectedPiece);

        CheckExitAfterDrag();
    }

    private Vector3 GetConstrainedMovement(
    FoodPiece piece,
    Vector3 currentPosition,
    Vector3 movement)
    {
        if (movement.sqrMagnitude < 0.000001f)
            return Vector3.zero;

        Vector3 direction =
            movement.normalized;

        float distance =
            movement.magnitude;

        Collider pieceCollider =
            piece.GetComponentInChildren<Collider>();

        if (pieceCollider == null)
            return movement;

        Bounds bounds =
            pieceCollider.bounds;

        Vector3 halfExtents =
            bounds.extents;

        RaycastHit hit;

        if (Physics.BoxCast(
            bounds.center,
            halfExtents,
            direction,
            out hit,
            Quaternion.identity,
            distance,
            foodLayer
        ))
        {
            if (hit.collider.GetComponentInParent<FoodPiece>() != piece)
            {
                float safeDistance =
                    Mathf.Max(
                        0f,
                        hit.distance - 0.02f
                    );

                return direction * safeDistance;
            }
        }

        return movement;
    }

    private Vector3 ClampToGridBounds(
    FoodPiece piece,
    Vector3 position)
    {
        float halfWidth =
            piece.size.x * cellSize * 0.5f;

        float halfHeight =
            piece.size.y * cellSize * 0.5f;

        float minX =
            gridOrigin.x + halfWidth;

        float maxX =
            gridOrigin.x +
            width * cellSize -
            halfWidth;

        float minZ =
            gridOrigin.z + halfHeight;

        float maxZ =
            gridOrigin.z +
            height * cellSize -
            halfHeight;

        position.x =
            Mathf.Clamp(
                position.x,
                minX,
                maxX
            );

        position.z =
            Mathf.Clamp(
                position.z,
                minZ,
                maxZ
            );

        return position;
    }

    /*private void HandleDragMovement()
    {
        if (selectedPiece == null)
            return;

        if (Mouse.current == null || mainCamera == null)
            return;

        // --------------------------------
        // START DRAG
        // --------------------------------

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            dragStartWorldPosition =
                GetMouseWorldPosition();

            dragStartGridPosition =
                selectedPiece.gridPosition;

            currentDragPosition =
                selectedPiece.transform.position;

            isDraggingPiece = true;

            // Remove it temporarily from the grid
            UnregisterPiece(selectedPiece);
        }

        // --------------------------------
        // DRAG
        // --------------------------------

        if (isDraggingPiece &&
            Mouse.current.leftButton.isPressed)
        {
            UpdateDraggedPiece();
        }

        // --------------------------------
        // RELEASE
        // --------------------------------

        if (isDraggingPiece &&
            Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDraggingPiece = false;

            // Snap to final grid position
            selectedPiece.transform.position =
                GetPieceWorldPosition(
                    selectedPiece.gridPosition,
                    selectedPiece.size
                );

            RegisterPieceAtPosition(selectedPiece);

            CheckExitAfterDrag();
        }
    }*/

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray =
            mainCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

        Plane plane =
            new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return selectedPiece.transform.position;
    }

    private void UpdateDraggedPiece()
    {
        Vector3 mouseWorldPosition =
            GetMouseWorldPosition();

        Vector3 delta =
            mouseWorldPosition -
            dragStartWorldPosition;

        Vector3 desiredWorldPosition =
            GetPieceWorldPosition(
                dragStartGridPosition,
                selectedPiece.size
            ) + delta;

        Vector2Int desiredGridPosition =
            GetGridPosition(
                desiredWorldPosition,
                selectedPiece.size
            );

        Vector2Int validGridPosition =
            GetValidDragPosition(
                selectedPiece,
                dragStartGridPosition,
                desiredGridPosition
            );

        selectedPiece.gridPosition =
            validGridPosition;

        Vector3 targetWorldPosition =
            GetPieceWorldPosition(
                validGridPosition,
                selectedPiece.size
            );

        selectedPiece.transform.position =
    Vector3.MoveTowards(
        selectedPiece.transform.position,
        targetWorldPosition,
        dragMoveSpeed * Time.deltaTime
    );
    }

    private Vector2Int GetValidDragPosition(
    FoodPiece piece,
    Vector2Int startPosition,
    Vector2Int targetPosition)
    {
        Vector2Int currentPosition = startPosition;

        int deltaX =
            targetPosition.x - startPosition.x;

        int deltaY =
            targetPosition.y - startPosition.y;

        // Determine which direction has the larger movement.
        // This prevents diagonal movement from feeling random.
        if (Mathf.Abs(deltaX) >= Mathf.Abs(deltaY))
        {
            // Horizontal movement first

            int directionX =
                deltaX > 0 ? 1 : -1;

            for (int i = 0; i < Mathf.Abs(deltaX); i++)
            {
                Vector2Int nextPosition =
                    currentPosition +
                    new Vector2Int(directionX, 0);

                if (!CanMoveTo(piece, nextPosition))
                    break;

                currentPosition = nextPosition;
            }

            // Then attempt vertical movement
            int directionY =
                deltaY > 0 ? 1 : -1;

            for (int i = 0; i < Mathf.Abs(deltaY); i++)
            {
                Vector2Int nextPosition =
                    currentPosition +
                    new Vector2Int(0, directionY);

                if (!CanMoveTo(piece, nextPosition))
                    break;

                currentPosition = nextPosition;
            }
        }
        else
        {
            // Vertical movement first

            int directionY =
                deltaY > 0 ? 1 : -1;

            for (int i = 0; i < Mathf.Abs(deltaY); i++)
            {
                Vector2Int nextPosition =
                    currentPosition +
                    new Vector2Int(0, directionY);

                if (!CanMoveTo(piece, nextPosition))
                    break;

                currentPosition = nextPosition;
            }

            // Then attempt horizontal movement
            int directionX =
                deltaX > 0 ? 1 : -1;

            for (int i = 0; i < Mathf.Abs(deltaX); i++)
            {
                Vector2Int nextPosition =
                    currentPosition +
                    new Vector2Int(directionX, 0);

                if (!CanMoveTo(piece, nextPosition))
                    break;

                currentPosition = nextPosition;
            }
        }

        return currentPosition;
    }

    private Vector2Int GetFurthestValidPosition(
    FoodPiece piece,
    Vector2Int startPosition,
    Vector2Int targetPosition)
    {
        Vector2Int currentPosition = startPosition;

        int deltaX = targetPosition.x - startPosition.x;
        int deltaY = targetPosition.y - startPosition.y;

        // Move horizontally
        if (Mathf.Abs(deltaX) > 0)
        {
            int directionX = deltaX > 0 ? 1 : -1;

            for (int i = 0; i < Mathf.Abs(deltaX); i++)
            {
                Vector2Int nextPosition =
                    currentPosition +
                    new Vector2Int(directionX, 0);

                if (!CanMoveTo(piece, nextPosition))
                {
                    break;
                }

                currentPosition = nextPosition;
            }
        }

        // Move vertically
        if (Mathf.Abs(deltaY) > 0)
        {
            int directionY = deltaY > 0 ? 1 : -1;

            for (int i = 0; i < Mathf.Abs(deltaY); i++)
            {
                Vector2Int nextPosition =
                    currentPosition +
                    new Vector2Int(0, directionY);

                if (!CanMoveTo(piece, nextPosition))
                {
                    break;
                }

                currentPosition = nextPosition;
            }
        }

        return currentPosition;
    }

    private Vector2Int FindClosestValidPosition(
    FoodPiece piece,
    Vector2Int desiredPosition)
    {
        if (CanMoveTo(piece, desiredPosition))
        {
            return desiredPosition;
        }

        Vector2Int bestPosition =
            piece.gridPosition;

        int bestDistance =
            ManhattanDistance(
                piece.gridPosition,
                desiredPosition
            );

        // Search nearby positions
        for (int radius = 1; radius <= 10; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector2Int candidate =
                        desiredPosition +
                        new Vector2Int(x, y);

                    if (!CanMoveTo(piece, candidate))
                        continue;

                    int distance =
                        ManhattanDistance(
                            candidate,
                            desiredPosition
                        );

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestPosition = candidate;
                    }
                }
            }

            if (bestDistance == 0)
                break;
        }

        return bestPosition;
    }

    private int ManhattanDistance(
    Vector2Int a,
    Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) +
               Mathf.Abs(a.y - b.y);
    }

    /*private void DragSelectedPiece()
    {
        Ray ray =
            mainCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

        Plane plane =
            new Plane(Vector3.up, Vector3.zero);

        if (!plane.Raycast(ray, out float distance))
            return;

        Vector3 worldPos =
            ray.GetPoint(distance);

        Vector3 delta =
            worldPos - dragStartWorldPosition;

        if (dragDirection == Vector2Int.zero)
        {
            if (Mathf.Abs(delta.x) >
                Mathf.Abs(delta.z))
            {
                dragDirection =
                    delta.x > 0 ?
                    Vector2Int.left:
                    Vector2Int.right;
            }
            else
            {
                dragDirection =
                    delta.z > 0 ?
                    Vector2Int.up :
                    Vector2Int.down;
            }
        }

        float movement;

        if (dragDirection.x != 0)
        {
            movement = delta.x;
        }
        else
        {
            movement = delta.z;
        }

        int cellOffset =
            Mathf.RoundToInt(movement / cellSize);

        Vector2Int targetPosition =
    dragStartGridPosition +
    dragDirection * cellOffset;

        UnregisterPiece(selectedPiece);

        if (CanMoveTo(selectedPiece, targetPosition))
        {
            selectedPiece.gridPosition =
                targetPosition;

            selectedPiece.transform.position =
                GetPieceWorldPosition(
                    targetPosition,
                    selectedPiece.size
                );
        }

        RegisterPieceAtPosition(selectedPiece);
    }*/

    private void CheckExitAfterDrag()
    {
        if (selectedPiece == null)
            return;

        if (exitPoint == null)
            return;

        if (IsAtExit(selectedPiece))
        {
            SendPieceToExit(selectedPiece);
        }
    }

    private void HandleSwipe()
    {
        if (selectedPiece == null)
            return;

        if (Mouse.current == null)
            return;

        // Mouse pressed
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            dragStartPosition =
                Mouse.current.position.ReadValue();

            isDragging = true;
        }

        // Mouse released
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (!isDragging)
                return;

            Vector2 dragEndPosition =
                Mouse.current.position.ReadValue();

            Vector2 swipeVector =
                dragEndPosition - dragStartPosition;

            isDragging = false;

            if (swipeVector.magnitude < minimumSwipeDistance)
            {
                return;
            }

            Vector2Int direction;

            if (Mathf.Abs(swipeVector.x) >
                Mathf.Abs(swipeVector.y))
            {
                // Horizontal swipe

                if (swipeVector.x > 0)
                {
                    direction = Vector2Int.right;
                }
                else
                {
                    direction = Vector2Int.left;
                }
            }
            else
            {
                // Vertical swipe

                if (swipeVector.y > 0)
                {
                    direction = Vector2Int.up;
                }
                else
                {
                    direction = Vector2Int.down;
                }
            }

            Debug.Log(
                "Swipe Direction: " + direction
            );

            MovePiece(
                selectedPiece,
                direction
            );
        }
    }

    private void HandleKeyboardMovement()
    {
        if (selectedPiece == null)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            MovePiece(
                selectedPiece,
                Vector2Int.right
            );
        }

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            MovePiece(
                selectedPiece,
                Vector2Int.left
            );
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            MovePiece(
                selectedPiece,
                Vector2Int.up
            );
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            MovePiece(
                selectedPiece,
                Vector2Int.down
            );
        }
    }

    private void RegisterAllFoodPieces()
    {
        FoodPiece[] pieces = FindObjectsByType<FoodPiece>();

        foreach (FoodPiece piece in pieces)
        {
            RegisterPiece(piece);
        }
    }

    public bool RegisterPiece(FoodPiece piece)
    {
        // Get the actual grid position from the food's current world position
        piece.gridPosition = GetGridPosition(
    piece.transform.position,
    piece.size
);

        for (int x = 0; x < piece.size.x; x++)
        {
            for (int y = 0; y < piece.size.y; y++)
            {
                int gridX = piece.gridPosition.x + x;
                int gridY = piece.gridPosition.y + y;

                if (!IsInsideGrid(gridX, gridY))
                {
                    Debug.LogWarning(
                        piece.name + " is outside the grid!"
                    );

                    return false;
                }

                if (grid[gridX, gridY] != null)
                {
                    Debug.LogWarning(
                        "Grid collision detected at " +
                        gridX + "," + gridY
                    );

                    return false;
                }
            }
        }

        // Occupy the cells
        for (int x = 0; x < piece.size.x; x++)
        {
            for (int y = 0; y < piece.size.y; y++)
            {
                int gridX = piece.gridPosition.x + x;
                int gridY = piece.gridPosition.y + y;

                grid[gridX, gridY] = piece;
            }
        }

        return true;
    }

    private bool IsInsideGrid(int x, int y)
    {
        return x >= 0 &&
               x < width &&
               y >= 0 &&
               y < height;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return gridOrigin + new Vector3(
            (x + 0.5f) * cellSize,
            0,
            (y + 0.5f) * cellSize
        );
    }

    public Vector3 GetPieceWorldPosition(Vector2Int gridPosition, Vector2Int size)
    {
        float centerX = gridPosition.x + (size.x - 1) * 0.5f;
        float centerY = gridPosition.y + (size.y - 1) * 0.5f;

        return gridOrigin + new Vector3(
            (centerX + 0.5f) * cellSize,
            0,
            (centerY + 0.5f) * cellSize
        );
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition, Vector2Int size)
    {
        float centerX = (worldPosition.x - gridOrigin.x) / cellSize;
        float centerY = (worldPosition.z - gridOrigin.z) / cellSize;

        int gridX = Mathf.FloorToInt(
            centerX - (size.x - 1) * 0.5f
        );

        int gridY = Mathf.FloorToInt(
            centerY - (size.y - 1) * 0.5f
        );

        return new Vector2Int(gridX, gridY);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        for (int x = 0; x <= width; x++)
        {
            Vector3 start = gridOrigin +
                            new Vector3(x * cellSize, 0, 0);

            Vector3 end = gridOrigin +
                          new Vector3(x * cellSize, 0, height * cellSize);

            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= height; y++)
        {
            Vector3 start = gridOrigin +
                            new Vector3(0, 0, y * cellSize);

            Vector3 end = gridOrigin +
                          new Vector3(width * cellSize, 0, y * cellSize);

            Gizmos.DrawLine(start, end);
        }

        if (Application.isPlaying && grid != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y] != null)
                    {
                        Gizmos.color = Color.yellow;

                        Vector3 center = GetWorldPosition(x, y);

                        Gizmos.DrawWireCube(
                            center,
                            new Vector3(
                                cellSize * 0.9f,
                                0.05f,
                                cellSize * 0.9f
                            )
                        );
                    }
                }
            }
        }
    }

    public bool CanMoveTo(FoodPiece piece, Vector2Int newPosition)
    {
        for (int x = 0; x < piece.size.x; x++)
        {
            for (int y = 0; y < piece.size.y; y++)
            {
                int gridX = newPosition.x + x;
                int gridY = newPosition.y + y;

                // Outside the board
                if (!IsInsideGrid(gridX, gridY))
                {
                    return false;
                }

                // Another food piece is occupying this cell
                if (grid[gridX, gridY] != null &&
                    grid[gridX, gridY] != piece)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public Vector2Int GetFurthestPosition(
    FoodPiece piece,
    Vector2Int direction)
    {
        Vector2Int currentPosition = piece.gridPosition;

        while (true)
        {
            Vector2Int nextPosition = currentPosition + direction;

            if (!CanMoveTo(piece, nextPosition))
            {
                break;
            }

            currentPosition = nextPosition;
        }

        return currentPosition;
    }

    private void UnregisterPiece(FoodPiece piece)
    {
        for (int x = 0; x < piece.size.x; x++)
        {
            for (int y = 0; y < piece.size.y; y++)
            {
                int gridX = piece.gridPosition.x + x;
                int gridY = piece.gridPosition.y + y;

                if (IsInsideGrid(gridX, gridY) &&
                    grid[gridX, gridY] == piece)
                {
                    grid[gridX, gridY] = null;
                }
            }
        }
    }

    public void MovePiece(FoodPiece piece, Vector2Int direction)
    {
        if (exitPoint != null &&
        direction == exitPoint.direction &&
        IsAtExit(piece))
        {
            SendPieceToExit(piece);
            return;
        }
        
        Vector2Int targetPosition = GetFurthestPosition(piece, direction);

        // It can't move
        if (targetPosition == piece.gridPosition)
        {
            return;
        }

        // Remove old grid occupancy
        UnregisterPiece(piece);

        // Update grid position
        piece.gridPosition = targetPosition;

        // Register new grid occupancy
        RegisterPieceAtPosition(piece);

        // Calculate the target world position
        Vector3 targetWorldPosition =
            GetPieceWorldPosition(
                piece.gridPosition,
                piece.size
            );

        // Move visually
        StartCoroutine(
            MovePieceAnimation(
                piece,
                targetWorldPosition
            )
        );
    }

    private void RegisterPieceAtPosition(FoodPiece piece)
    {
        for (int x = 0; x < piece.size.x; x++)
        {
            for (int y = 0; y < piece.size.y; y++)
            {
                int gridX = piece.gridPosition.x + x;
                int gridY = piece.gridPosition.y + y;

                if (IsInsideGrid(gridX, gridY))
                {
                    grid[gridX, gridY] = piece;
                }
            }
        }
    }

    private System.Collections.IEnumerator MovePieceAnimation(
    FoodPiece piece,
    Vector3 targetPosition)
    {
        Vector3 startPosition = piece.transform.position;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            // Smooth movement
            t = Mathf.SmoothStep(0f, 1f, t);

            piece.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }

        piece.transform.position = targetPosition;
    }

    private bool IsAtExit(FoodPiece piece)
    {
        Vector2Int direction = exitPoint.direction;

        // TOP
        if (direction == Vector2Int.up)
        {
            return piece.gridPosition.y + piece.size.y >= height;
        }

        // BOTTOM
        if (direction == Vector2Int.down)
        {
            return piece.gridPosition.y <= 0;
        }

        // RIGHT
        if (direction == Vector2Int.right)
        {
            return piece.gridPosition.x + piece.size.x >= width;
        }

        // LEFT
        if (direction == Vector2Int.left)
        {
            return piece.gridPosition.x <= 0;
        }

        return false;
    }

    private void SendPieceToExit(FoodPiece piece)
    {
        UnregisterPiece(piece);

        Vector3 exitPosition =
            exitPoint.transform.position;

        StartCoroutine(
            MovePieceToExit(
                piece,
                exitPosition
            )
        );
    }

    private System.Collections.IEnumerator MovePieceToExit(
    FoodPiece piece,
    Vector3 targetPosition)
    {
        Vector3 startPosition =
            piece.transform.position;

        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            piece.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }

        piece.transform.position = targetPosition;

        if (exitPoint.customer != null)
        {
            bool accepted =
                exitPoint.customer.ReceiveFood(piece);

            if (accepted)
            {
                Destroy(piece.gameObject);
            }
            else
            {
                ReturnPieceFromExit(piece);
            }
        }
        else
        {
            Destroy(piece.gameObject);
        }
    }

    private void ReturnPieceFromExit(FoodPiece piece)
    {
        Vector2Int originalGridPosition =
            piece.gridPosition;

        Vector3 returnPosition =
            GetPieceWorldPosition(
                originalGridPosition,
                piece.size
            );

        StartCoroutine(
            ReturnPieceAnimation(
                piece,
                returnPosition
            )
        );
    }

    private System.Collections.IEnumerator ReturnPieceAnimation(
    FoodPiece piece,
    Vector3 targetPosition)
    {
        Vector3 startPosition =
            piece.transform.position;

        float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            piece.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }

        piece.transform.position = targetPosition;

        RegisterPiece(piece);
    }
}