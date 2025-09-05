using UnityEngine;

public class OrbitAround : MonoBehaviour
{
    public Transform center;            // 绕着谁转
    public Vector3 axis = Vector3.up;   // 绕的轴（Y 轴 = 水平绕圈）
    public float speed = 45f;           // 转速（度/秒）
    public float radius = 2f;           // 半径
    public bool keepFacingCenter = true;// 是否始终面向中心（仅水平朝向）

    void Start()
    {
        if (!center) return;

        // 如果一开始和中心重合，给一个默认方向
        Vector3 dir = (transform.position - center.position);
        if (dir.sqrMagnitude < 1e-6f) dir = Vector3.forward;

        // 只用水平向量来放置初始位置，并保持当前高度不变
        dir.y = 0f;
        if (dir.sqrMagnitude < 1e-6f) dir = Vector3.forward;

        Vector3 pos = new Vector3(center.position.x, transform.position.y, center.position.z)
                      + dir.normalized * radius;
        transform.position = pos;
    }

    void Update()
    {
        if (!center) return;

        // 绕圈（确保轴是单位向量；轴为零则回退为 Vector3.up）
        Vector3 ax = axis.sqrMagnitude < 1e-6f ? Vector3.up : axis.normalized;
        transform.RotateAround(center.position, ax, speed * Time.deltaTime);

        if (keepFacingCenter)
        {
            // 只在水平面上朝向中心，避免抬头/低头导致“歪”
            Vector3 toCenter = center.position - transform.position;
            toCenter.y = 0f;
            if (toCenter.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.LookRotation(toCenter.normalized, Vector3.up);
        }
    }
}

