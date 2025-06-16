using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WaterFlow : MonoBehaviour
{
    [Header("Material Setup")]
    public string textureProperty = "_MainTex";  // Nom du champ UV dans le shader

    [Header("Horizontal Scroll (Sinusoidal)")]
    public float scrollSpeedX = 0.2f;
    public float scrollAmplitudeX = 1f;

    [Header("Vertical Scroll (Linéaire)")]
    public bool enableVerticalScroll = false;
    public float scrollSpeedY = 0.1f;

    private Renderer rend;
    private MaterialPropertyBlock block;
    private float yOffset;

    void Start()
    {
        rend = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
    }

    void Update()
    {
        float xOffset = Mathf.Sin(Time.time * scrollSpeedX * Mathf.PI * 2f) * scrollAmplitudeX;

        if (enableVerticalScroll)
            yOffset += Time.deltaTime * scrollSpeedY;

        Vector2 offset = new Vector2(xOffset, yOffset % 1f);

        rend.GetPropertyBlock(block);
        block.SetVector(textureProperty + "_ST", new Vector4(1, 1, offset.x, offset.y));
        rend.SetPropertyBlock(block);
    }
}