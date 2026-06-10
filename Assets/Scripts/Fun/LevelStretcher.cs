using System.Reflection;
using UnityEngine;

public static class LevelStretcher
{
    const float GroundCloneX = 98f;
    static readonly float[] targetGoalXByStage = { 0f, 52f, 100f, 105f, 100f, 110f };
    static readonly float[] parTimesByStage = { 0f, 35f, 70f, 80f, 80f, 95f };

    public static void Apply(int stage)
    {
        stage = Mathf.Clamp(stage, 1, LevelManager.MaxStage);
        float targetGoalX = TargetGoalX(stage);

        ExtendGroundIfNeeded(stage);
        MoveGoal(targetGoalX);
        MoveRightWall(targetGoalX + 8.5f);
        UpdateCameraClamp(targetGoalX - 0.5f);
        UpdateParTimes();
    }

    public static float TargetGoalX(int stage)
    {
        stage = Mathf.Clamp(stage, 1, targetGoalXByStage.Length - 1);
        return targetGoalXByStage[stage];
    }

    public static float CoinEndXForStage(int stage)
    {
        stage = Mathf.Clamp(stage, 1, LevelManager.MaxStage);
        if (stage >= LevelManager.MaxStage)
        {
            return 36f;
        }

        return TargetGoalX(stage) - 2f;
    }

    static void ExtendGroundIfNeeded(int stage)
    {
        if (stage <= 1 || GameObject.Find("Ground_Ext") != null)
        {
            return;
        }

        GameObject ground = GameObject.Find("Ground_0");
        if (ground == null)
        {
            Debug.LogWarning("LevelStretcher could not find Ground_0.");
            return;
        }

        GameObject extension = Object.Instantiate(ground, new Vector3(GroundCloneX, ground.transform.position.y, ground.transform.position.z), ground.transform.rotation);
        extension.name = "Ground_Ext";
        extension.transform.localScale = ground.transform.localScale;
    }

    static void MoveGoal(float targetGoalX)
    {
        GameObject goal = GameObject.Find("GoalFlag_0");
        if (goal == null)
        {
            Debug.LogWarning("LevelStretcher could not find GoalFlag_0.");
            return;
        }

        goal.transform.position = new Vector3(targetGoalX, goal.transform.position.y, goal.transform.position.z);
        SnapColliderBottomToGround(goal);
    }

    static void MoveRightWall(float x)
    {
        GameObject wall = GameObject.Find("wall");
        if (wall == null)
        {
            Debug.LogWarning("LevelStretcher could not find right wall.");
            return;
        }

        wall.transform.position = new Vector3(x, wall.transform.position.y, wall.transform.position.z);
    }

    static void UpdateCameraClamp(float maxX)
    {
        CameraFollowClamp2 cameraClamp = Object.FindAnyObjectByType<CameraFollowClamp2>();
        if (cameraClamp != null)
        {
            cameraClamp.maxX = maxX;
        }
    }

    static void UpdateParTimes()
    {
        FieldInfo field = typeof(ScoreSystem).GetField("parTimes", BindingFlags.NonPublic | BindingFlags.Static);
        float[] parTimes = field != null ? field.GetValue(null) as float[] : null;
        if (parTimes == null)
        {
            Debug.LogWarning("LevelStretcher could not update ScoreSystem par times.");
            return;
        }

        int count = Mathf.Min(parTimes.Length, parTimesByStage.Length);
        for (int i = 1; i < count; i++)
        {
            parTimes[i] = parTimesByStage[i];
        }
    }

    static void SnapColliderBottomToGround(GameObject target)
    {
        Collider2D collider = target.GetComponent<Collider2D>();
        if (collider == null)
        {
            return;
        }

        Physics2D.SyncTransforms();
        Bounds bounds = collider.bounds;
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(bounds.center.x, bounds.min.y - 0.02f), Vector2.down, 30f);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider == collider || hit.collider.isTrigger || hit.normal.y < 0.5f)
            {
                continue;
            }

            string objectName = hit.collider.gameObject.name;
            if (!objectName.StartsWith("Ground") && !objectName.StartsWith("Platform"))
            {
                continue;
            }

            float yOffset = hit.point.y - bounds.min.y;
            target.transform.position += new Vector3(0f, yOffset, 0f);
            Physics2D.SyncTransforms();
            return;
        }
    }
}
