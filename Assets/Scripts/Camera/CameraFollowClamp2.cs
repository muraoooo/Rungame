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

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        FindPlayerIfNeeded();
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
        Vector3 targetPosition = player.position + offset;
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Max(targetPosition.y, minY);
        targetPosition.z = offset.z;
        return targetPosition;
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
