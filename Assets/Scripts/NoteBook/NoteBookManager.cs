using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NotebookManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject notebookUI;
    public RawImage notebookImage;
    public Button previousPageButton;
    public Button nextPageButton;
    private List<bool> pagesUsed = new List<bool>(); 
    private bool hasDrawnOnCurrentPage = false;
    [SerializeField] private GameObject closedNoteBook;
    [SerializeField] private GameObject paperNoteBookUI;
    [SerializeField] private GameObject tools;
    [SerializeField] private GameObject openNoteBookUI;

    [Header("Player Settings")]
    [SerializeField] private ThirdPersonController controller;
    [SerializeField] private CameraFreeLook cameraFreeLook;
    
    [Header("Brush Settings")]
    [Tooltip("Taille initiale du pinceau.")]
    [SerializeField] private int brushSize = 4;
    [Tooltip("Couleur initiale du pinceau.")]
    [SerializeField] private Color brushColor = Color.black;

    [Header("Page")]
    private List<Texture2D> pages = new List<Texture2D>();
    private int currentPageIndex = 0;

    [Header("Brush Size Limits")]
    [SerializeField] private int minBrushSize = 1;
    [SerializeField] private int maxBrushSize = 20;

    [Header("Colors Available (optional)")]
    public Color[] availableColors = new Color[]
    {
        Color.black, Color.red, Color.green, Color.blue, Color.magenta, Color.yellow
    };

    private int currentColorIndex = 0;
    private bool isEraserActive = false;
    private bool needsApply = false;
    private Color savedBrushColor;

    public Button penButton;
    public Button eraserButton;

    private bool isNotebookOpen = false;
    private Texture2D drawingTexture;
    private Vector2? lastDrawPos = null;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.PlayerControls.NoteBook.performed += ToggleNotebook;
    }

    private void OnDisable()
    {
        inputActions.PlayerControls.NoteBook.performed -= ToggleNotebook;
        inputActions.Disable();
    }

    private void Start()
    {
        SelectPen();
        penButton.onClick.AddListener(SelectPen);
        eraserButton.onClick.AddListener(SelectEraser);
        notebookUI.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        UpdatePageButtons();
    }

    private void Update()
    {
        if (!isNotebookOpen || notebookImage == null || drawingTexture == null)
            return;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            RectTransform rectTransform = notebookImage.rectTransform;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    mousePos,
                    null,
                    out Vector2 localPoint))
            {
                Rect rect = rectTransform.rect;
                float normX = (localPoint.x - rect.x) / rect.width;
                float normY = (localPoint.y - rect.y) / rect.height;

                bool isInsideDrawingArea = normX >= 0f && normX <= 1f && normY >= 0f && normY <= 1f;

                if (isInsideDrawingArea)
                {
                    int x = Mathf.RoundToInt(normX * drawingTexture.width);
                    int y = Mathf.RoundToInt(normY * drawingTexture.height);

                    Vector2 currentPos = new Vector2(x, y);

                    if (lastDrawPos.HasValue)
                        DrawLine(lastDrawPos.Value, currentPos);
                    else
                        DrawCircle(x, y);

                    lastDrawPos = currentPos;
                }
                else
                {
                    lastDrawPos = null;
                }
            }
        }
        else
        {
            lastDrawPos = null;
        }

        // Appliquer les changements si la texture a été modifiée
        if (needsApply)
        {
            drawingTexture.Apply();
            needsApply = false;
        }
    }

    private void UpdatePageButtons()
    {
        previousPageButton.gameObject.SetActive(true);
        bool isLastPage = currentPageIndex == pages.Count - 1;
        bool canGoNext = !isLastPage || pagesUsed[currentPageIndex];

        nextPageButton.interactable = canGoNext;
    }


    private bool IsPageEmpty(Texture2D tex)
    {
        Color[] pixels = tex.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i] != Color.white)
                return false;
        }

        return true;
    }

    public void OpenNotebook()
    {
        Debug.LogWarning("📓 OpenNotebook() a bien été appelé !");
        if (closedNoteBook != null)
            closedNoteBook.SetActive(false);

        if (paperNoteBookUI != null)
            paperNoteBookUI.SetActive(true);
            tools.SetActive(true);
            openNoteBookUI.SetActive(false);
            
        if (pages.Count == 0)
        {
            drawingTexture = CreateBlankPage();
            pages.Add(drawingTexture);
            pagesUsed.Add(false);
        }

        currentPageIndex = 0;
        LoadPage(currentPageIndex);

        isNotebookOpen = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (cameraFreeLook != null)
            cameraFreeLook.cameraFrozen = true;

        UpdatePageButtons();
    }



    public void CloseNotebook()
    {
        if (closedNoteBook != null)
            closedNoteBook.SetActive(true);

        if (paperNoteBookUI != null)
            paperNoteBookUI.SetActive(false);

        isNotebookOpen = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (cameraFreeLook != null)
            cameraFreeLook.cameraFrozen = false;
    }

    
    private void ToggleNotebook(InputAction.CallbackContext ctx)
    {
        isNotebookOpen = !isNotebookOpen;
        notebookUI.SetActive(isNotebookOpen);

        Cursor.visible = isNotebookOpen;
        Cursor.lockState = isNotebookOpen ? CursorLockMode.None : CursorLockMode.Locked;

        if (cameraFreeLook != null)
            cameraFreeLook.cameraFrozen = isNotebookOpen;
    }
    
    
    public void NextPage()
    {
        Debug.Log($"[NextPage] Current index: {currentPageIndex}, Total pages: {pages.Count}");

        if (currentPageIndex == pages.Count - 1)
        {
            Debug.Log("[NextPage] On est sur la dernière page.");

            if (!pagesUsed[currentPageIndex])
            {
                Debug.LogWarning("[NextPage] Tentative de créer une nouvelle page alors que la dernière est encore vide.");
                return;
            }
            else
            {
                Debug.Log("[NextPage] Dernière page a été utilisée, création autorisée.");
            }
        }

        currentPageIndex++;

        if (currentPageIndex >= pages.Count)
        {
            Debug.Log("[NextPage] Nouvelle page ajoutée.");
            Texture2D newPage = CreateBlankPage();
            pages.Add(newPage);
            pagesUsed.Add(false);
        }

        LoadPage(currentPageIndex);
    }




    public void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            LoadPage(currentPageIndex);
            UpdatePageButtons();
        }
        else
        {
            if (closedNoteBook != null)
                closedNoteBook.SetActive(true);
                tools.SetActive(false);
                openNoteBookUI.SetActive(true);
                
            if (paperNoteBookUI != null)
                paperNoteBookUI.SetActive(false);

            isNotebookOpen = false;
        }
    }

    private void LoadPage(int index)
    {
        currentPageIndex = index;
        drawingTexture = pages[index];
        notebookImage.texture = drawingTexture;
        needsApply = false;

        hasDrawnOnCurrentPage = pagesUsed[index];

        UpdatePageButtons();
    }

    private Texture2D CreateBlankPage()
    {
        Texture2D tex = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
        Color[] fillColor = new Color[tex.width * tex.height];

        for (int i = 0; i < fillColor.Length; i++)
            fillColor[i] = new Color(0, 0, 0, 0);

        tex.SetPixels(fillColor);
        tex.Apply();

        return tex;
    }



    public void SelectPen()
    {
        isEraserActive = false;
    }

    public void SelectEraser()
    {
        isEraserActive = true;
    }

    public void ClearPageUI()
    {
        ClearTexture(new Color(0, 0, 0, 0));
    }

    public void SetBrushSize(float normalizedValue)
    {
        brushSize = Mathf.RoundToInt(Mathf.Lerp(minBrushSize, maxBrushSize, normalizedValue));
        Debug.Log("Brush size set to: " + brushSize);
    }

    public void SetBrushColorByIndex(int index)
    {
        if (index < 0 || index >= availableColors.Length) return;

        isEraserActive = false;
        brushColor = availableColors[index];
    }

    public void SetBrushColor(Color newColor)
    {
        isEraserActive = false;
        brushColor = newColor;
    }

    private void ClearTexture(Color color)
    {
        Color[] fillColor = new Color[drawingTexture.width * drawingTexture.height];
        for (int i = 0; i < fillColor.Length; i++)
        {
            fillColor[i] = color;
        }

        drawingTexture.SetPixels(fillColor);
        drawingTexture.Apply();
    }

    private void DrawCircle(int x, int y)
{
    Color currentColor = isEraserActive ? new Color(0, 0, 0, 0) : brushColor;
    int diameter = brushSize * 2 + 1;
    Color[] pixels = new Color[diameter * diameter];
    bool[] mask = new bool[diameter * diameter];

    int index = 0;
    for (int i = -brushSize; i <= brushSize; i++)
    {
        for (int j = -brushSize; j <= brushSize; j++)
        {
            if (i * i + j * j <= brushSize * brushSize)
            {
                int px = x + i;
                int py = y + j;

                if (px >= 0 && px < drawingTexture.width && py >= 0 && py < drawingTexture.height)
                {
                    pixels[index] = currentColor;
                    mask[index] = true;
                }
            }
            index++;
        }
    }

    // Appliquer uniquement les pixels valides
    index = 0;
    for (int i = -brushSize; i <= brushSize; i++)
    {
        for (int j = -brushSize; j <= brushSize; j++)
        {
            if (mask[index])
            {
                int px = Mathf.Clamp(x + i, 0, drawingTexture.width - 1);
                int py = Mathf.Clamp(y + j, 0, drawingTexture.height - 1);
                drawingTexture.SetPixel(px, py, pixels[index]);
            }
            index++;
        }
    }

    needsApply = true;

    if (!hasDrawnOnCurrentPage)
    {
        Debug.Log($"[DrawCircle] Premier dessin détecté sur la page {currentPageIndex}.");
        pagesUsed[currentPageIndex] = true;
        hasDrawnOnCurrentPage = true;
        UpdatePageButtons();
    }
}


    private void DrawLine(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance / (brushSize * 0.5f));

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            int x = Mathf.RoundToInt(Mathf.Lerp(start.x, end.x, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(start.y, end.y, t));
            DrawCircle(x, y);
        }
    }
}
