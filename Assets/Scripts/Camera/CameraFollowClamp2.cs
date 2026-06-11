using UnityEngine;

public class CameraFollowClamp2 : MonoBehaviour
{
    [Header("追いかける対象")]
    public Transform player;

    [Header("カメラのズレ")]
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    [Header("ステージの左端と右端")]
    public float minX = 0f;
    public float maxX = 100f;

    [Header("カメラが下がれる最低ライン")]
    public float minY = 0f;

    [Header("背景")]
    public Transform background;

    [Header("なめらかさ")]
    public float smoothTime = 0.15f;
    public float verticalDeadZoneUp = 2.2f;
    public float verticalFollowStrength = 0.35f;

    private Vector3 velocity = Vector3.zero;
    private float baselinePlayerY;
    private float baselineCameraY;
    private bool hasBaseline;

    void Start()
    {
        FindPlayerIfNeeded();
        CaptureBaseline();
        SnapToPlayer();
    }

    void FindPlayerIfNeeded()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (player == null)
        {
            FindPlayerIfNeeded();
        }

        if (player == null)
        {
            return;
        }

        Vector3 targetPosition = GetTargetPosition();

        if (!GameSession.CanControlPlayer)
        {
            transform.position = targetPosition;
            velocity = Vector3.zero;
            MoveBackgroundWithCamera();
            return;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        MoveBackgroundWithCamera();
    }

    void SnapToPlayer()
    {
        if (player == null)
        {
            return;
        }

        transform.position = GetTargetPosition();
        velocity = Vector3.zero;
        MoveBackgroundWithCamera();
    }

    Vector3 GetTargetPosition()
    {
        if (!hasBaseline)
        {
            CaptureBaseline();
        }

        Vector3 targetPosition = player.position + offset;
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        float heightAboveBaseline = Mathf.Max(0f, player.position.y - baselinePlayerY - verticalDeadZoneUp);
        targetPosition.y = Mathf.Max(baselineCameraY + heightAboveBaseline * verticalFollowStrength, minY);
        targetPosition.z = offset.z;
        return targetPosition;
    }

    void CaptureBaseline()
    {
        if (player == null)
        {
            return;
        }

        baselinePlayerY = player.position.y;
        baselineCameraY = Mathf.Max(player.position.y + offset.y, minY);
        hasBaseline = true;
    }

    void MoveBackgroundWithCamera()
    {
        if (background != null)
        {
            background.position = new Vector3(
                transform.position.x,
                background.position.y,
                background.position.z
            );
        }
    }
}
