#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TilemapColorRepair
{
    [MenuItem("Tools/Tilemap/Reset Cell Colors")]
    private static void ResetCellColors()
    {
        Tilemap tilemap = Selection.activeGameObject
            ?.GetComponent<Tilemap>();

        if (tilemap == null)
        {
            Debug.LogWarning("Tilemap 오브젝트를 선택하세요.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(
            tilemap,
            "Reset Tilemap Cell Colors"
        );

        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(position))
                continue;

            TileFlags flags = tilemap.GetTileFlags(position);

            tilemap.SetTileFlags(
                position,
                flags & ~TileFlags.LockColor
            );

            tilemap.SetColor(position, Color.white);

            tilemap.SetTileFlags(position, flags);
        }

        tilemap.RefreshAllTiles();
        EditorUtility.SetDirty(tilemap);
    }
}
#endif