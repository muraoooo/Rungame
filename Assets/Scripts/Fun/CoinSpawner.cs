using UnityEngine;

public static class CoinSpawner
{
    public static void SpawnLevelCoins()
    {
        float startX = -4f;
        float endX = 50f;
        float step = 2.4f;
        int column = 0;

        for (float x = startX; x <= endX; x += step, column++)
        {
            Vector2 groundPoint;
            if (!TryFindGround(x, out groundPoint))
            {
                continue;
            }

            Coin.Spawn(new Vector3(groundPoint.x, groundPoint.y + 0.95f, 0f));

            if (column % 4 == 1)
            {
                Coin.Spawn(new Vector3(groundPoint.x, groundPoint.y + 2.6f, 0f));
            }
        }
    }

    public static bool TryFindGround(float x, out Vector2 groundPoint)
    {
        groundPoint = Vector2.zero;
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(x, 14f), Vector2.down, 30f);
        bool found = false;
        float bestY = float.NegativeInfinity;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider.isTrigger)
            {
                continue;
            }

            string name = hit.collider.gameObject.name;
            if (!name.StartsWith("Ground") && !name.StartsWith("Platform"))
            {
                continue;
            }

            // Moving lifts are only sometimes at the raycast position.
            if (name.Contains("Lift"))
            {
                continue;
            }

            if (hit.point.y > bestY)
            {
                bestY = hit.point.y;
                groundPoint = hit.point;
                found = true;
            }
        }

        return found;
    }
}
