using UnityEngine;

public class Anchor : MonoBehaviour
{
    [Header("Critères d’acceptation")]
    [Tooltip("Nom de l’objet attendu (ex : 'Diamant')")]
    public string expectedItemName;

    [Tooltip("Taille attendue de l’objet")]
    public size acceptedSize;

    [Tooltip("Position locale une fois ancré")]
    public Vector3 anchoredLocalOffset;

    private bool isOccupied = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isOccupied) return;

        CollectibleObject collectible = other.GetComponent<CollectibleObject>();
        if (collectible == null) return;

        if (InventorySystem.instance.itemInHand == collectible) return;

        if (collectible.GetItemName() != expectedItemName) return;
        if (collectible.GetSize() != acceptedSize) return;

        AnchorObject(collectible);
    }

    private void AnchorObject(CollectibleObject obj)
    {
        obj.rb.isKinematic = true;
        obj.rb.useGravity = false;
        obj.transform.SetParent(transform);
        obj.transform.localPosition = anchoredLocalOffset;
        obj.transform.localRotation = Quaternion.identity;

        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        isOccupied = true;

        Debug.Log($"✅ Objet '{obj.GetItemName()}' (taille : {obj.GetSize()}) ancré sur {name}");
    }
}