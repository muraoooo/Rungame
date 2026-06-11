using UnityEngine;

public static class CoinSpawner
{
    public static void SpawnLevelCoins(float endX = 50f)
    {
        const float startX = -6.2f;
        float earlyEndX = Mathf.Min(endX, Mathf.Max(18f, endX * 0.48f));
        float middleStartX = earlyEndX + 5f;
        float middleEndX = Mathf.Min(endX, Mathf.Max(middleStartX, endX * 0.72f));

        int earlyIndex = 0;
        for (float x = startX; x <= earlyEndX; x += 1.35f)
        {
            SpawnCoinAtGround(x, 0.95f);

            // Occasional second coins make the opening read as a reward lane
            // without forcing precise jumps for the W charge.
            if (earlyIndex % 5 == 2)
            {
                SpawnCoinAtGround(x + 0.25f, 1.65f);
            }

            earlyIndex++;
        }

        for (float x = middleStartX; x <= middleEndX; x += 8.5f)
        {
            SpawnCoinAtGround(x, 1.05f);
        }
    }

    static void SpawnCoinAtGround(float x, float yOffset)
    {
        Vector2 groundPoint;
        if (!TryFindGround(x, out groundPoint))
        {
            return;
        }

        Coin.Spawn(new Vector3(groundPoint.x, groundPoint.y + yOffset, 0f));
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
