using UnityEngine;
using UnityEngine.Tilemaps;

public class DisappearingTilemap : MonoBehaviour
{
    private Tilemap tilemap;

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger ENTER -> Object: " + other.name + " | Tag: " + other.tag);

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Ignorado: no es Player");
            return;
        }

        Vector3 center = other.bounds.center;
        Vector3 feet = new Vector3(
            other.bounds.center.x,
            other.bounds.min.y + 0.01f,
            0f
        );

        TryRemoveAtWorldPos(center, "CENTER");
        TryRemoveAtWorldPos(feet, "FEET");
    }

    void TryRemoveAtWorldPos(Vector3 worldPos, string label)
    {
        Vector3Int cellPos = tilemap.WorldToCell(worldPos);

        Debug.Log(
            label +
            " world=" + worldPos +
            " cell=" + cellPos +
            " hasTile=" + tilemap.HasTile(cellPos)
        );

        if (tilemap.HasTile(cellPos))
        {
            Debug.Log("Tile encontrado, borrando en " + cellPos);
            tilemap.SetTile(cellPos, null);
            tilemap.RefreshTile(cellPos);
            return;
        }

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector3Int c = new Vector3Int(cellPos.x + dx, cellPos.y + dy, cellPos.z);
                if (tilemap.HasTile(c))
                {
                    Debug.Log("Tile encontrado cerca en " + c + ", borrando");
                    tilemap.SetTile(c, null);
                    tilemap.RefreshTile(c);
                    return;
                }
            }
        }

        Debug.Log("No hay tile en esta celda ni alrededor");
    }
}
