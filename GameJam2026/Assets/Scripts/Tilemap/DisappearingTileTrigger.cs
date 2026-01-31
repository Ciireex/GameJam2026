using UnityEngine;
using UnityEngine.Tilemaps;

public class DisappearingTileTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Arrastra aquí el Tilemap que contiene los tiles que deben desaparecer")]
    [SerializeField] private Tilemap targetTilemap;

    [Header("Behaviour")]
    [SerializeField] private float fallDelay = 0.15f;
    [SerializeField] private bool disableTriggerAfterUse = true;

    private bool used;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        if (targetTilemap == null)
        {
            Debug.LogError("[DisappearingTileTrigger] targetTilemap no asignado.");
            return;
        }

        // Usamos la posición del trigger (más fiable que bounds del player)
        Vector3Int cellPos = targetTilemap.WorldToCell(transform.position);

        if (!targetTilemap.HasTile(cellPos))
        {
            Debug.LogWarning("[DisappearingTileTrigger] No hay tile en la celda " + cellPos);
            return;
        }

        used = true;
        StartCoroutine(RemoveTileAfterDelay(cellPos));
    }

    private System.Collections.IEnumerator RemoveTileAfterDelay(Vector3Int cellPos)
    {
        yield return new WaitForSeconds(fallDelay);

        targetTilemap.SetTile(cellPos, null);
        targetTilemap.RefreshTile(cellPos);

        if (disableTriggerAfterUse)
            gameObject.SetActive(false);
    }
}
