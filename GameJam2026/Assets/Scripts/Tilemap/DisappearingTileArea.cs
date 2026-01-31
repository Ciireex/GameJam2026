using UnityEngine;
using UnityEngine.Tilemaps;

public class DisappearingTileArea : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Arrastra aquí el Tilemap que contiene los tiles que deben desaparecer")]
    [SerializeField] private Tilemap targetTilemap;

    [Header("Area size (in cells)")]
    [SerializeField] private Vector2Int areaSize = new Vector2Int(2, 1);

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
            Debug.LogError("[DisappearingTileArea] targetTilemap no asignado.");
            return;
        }

        used = true;
        StartCoroutine(RemoveAreaAfterDelay());
    }

    private System.Collections.IEnumerator RemoveAreaAfterDelay()
    {
        yield return new WaitForSeconds(fallDelay);

        Vector3Int centerCell = targetTilemap.WorldToCell(transform.position);

        int startX = centerCell.x - areaSize.x / 2;
        int startY = centerCell.y - areaSize.y / 2;

        for (int x = 0; x < areaSize.x; x++)
        {
            for (int y = 0; y < areaSize.y; y++)
            {
                Vector3Int cell = new Vector3Int(startX + x, startY + y, centerCell.z);

                if (targetTilemap.HasTile(cell))
                {
                    targetTilemap.SetTile(cell, null);
                    targetTilemap.RefreshTile(cell);
                }
            }
        }

        if (disableTriggerAfterUse)
            gameObject.SetActive(false);
    }
}
