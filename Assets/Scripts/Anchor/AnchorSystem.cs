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

    [Header("Comportement supplémentaire")]
    [Tooltip("Collider à désactiver une fois l’objet ancré")]
    public Collider colliderToDisableOnSet;

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

        // 🛠️ Conserver la taille visible (scale monde)
        Vector3 worldScaleBefore = obj.transform.lossyScale;

        obj.transform.SetParent(transform);

        // 🔧 Recalculer la scale locale pour compenser celle du parent
        Vector3 parentScale = transform.lossyScale;
        obj.transform.localScale = new Vector3(
            worldScaleBefore.x / parentScale.x,
            worldScaleBefore.y / parentScale.y,
            worldScaleBefore.z / parentScale.z
        );

        obj.transform.localPosition = anchoredLocalOffset;
        obj.transform.localRotation = Quaternion.identity;

        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        isOccupied = true;

        if (colliderToDisableOnSet != null)
        {
            colliderToDisableOnSet.enabled = false;
            Debug.Log($"[AnchorSystem] Collider '{colliderToDisableOnSet.name}' désactivé après ancrage.");
        }

        Debug.Log($"Objet '{obj.GetItemName()}' (taille : {obj.GetSize()}) ancré sur {name}");
    }


    public bool GetOccupied() => isOccupied;
}
