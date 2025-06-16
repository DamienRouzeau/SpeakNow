using UnityEngine;

public class AnchorSystem : MonoBehaviour
{
    [Header("Critères d’acceptation")]
    [Tooltip("Nom de l’objet attendu (ex : 'Diamant')")]
    public string expectedItemName;

    [Tooltip("Taille attendue de l’objet")]
    public size acceptedSize;

    [Tooltip("Position locale une fois ancré")]
    public Vector3 anchoredLocalOffset;

    private bool isOccupied = false;

    [Header("Paramètres d’interaction")]
    [Tooltip("L'objet ancré peut-il être ramassé après coup ?")]
    public bool isRecoverable = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isOccupied) return;

        CollectibleObject collectible = other.GetComponent<CollectibleObject>();
        if (collectible == null) return;

        if (InventorySystem.instance.itemInHand == collectible && collectible.transform.parent != null) return;

        if (expectedItemName != "all")
            if (collectible.GetItemName() != expectedItemName) return;

        if (collectible.GetSize() != acceptedSize) return;
        AnchorObject(collectible);
    }

    public void SetOccupied(bool state)
    {
        isOccupied = state;
    }

    private void AnchorObject(CollectibleObject obj)
    {
        obj.currentAnchor = this;
        obj.rb.isKinematic = true;
        obj.rb.useGravity = false;
        obj.transform.SetParent(transform);
        obj.transform.localPosition = anchoredLocalOffset;
        obj.transform.localRotation = Quaternion.identity;

        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        isOccupied = true;

        Debug.Log($" Objet '{obj.GetItemName()}' (taille : {obj.GetSize()}) ancré sur {name}");
    }
    public bool GetOccupied() { return isOccupied; }

}