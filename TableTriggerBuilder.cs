using UnityEngine;

public class TableTriggerBuilder : MonoBehaviour
{
    [Header("把桌面(或桌面顶面)的 Transform 拖进来")]
    public Transform tableTop;

    [Header("触发平面尺寸（米）  X=宽  Y=长")]
    public Vector2 size = new Vector2(0.5f, 0.3f);

    [Header("离桌面抬高（米，避免与桌面共面闪烁）")]
    public float lift = 0.02f;

    [Header("相对桌面的水平偏移（米）  X=右  Y=前")]
    public Vector2 localOffsetXZ = Vector2.zero;

    [Header("可视化选项")]
    public bool createVisualQuad = true;     // 半透明 Quad（1x1 标准）
    public bool createUnityPlaneMesh = false;// Unity Plane 网格（10x10）
    public Material planeMaterial;           // 可选材质（没有就自动给个 Unlit/Color）

    [ContextMenu("Create Trigger Plane")]
    public void CreateTrigger()
    {
        if (!tableTop)
        {
            Debug.LogWarning("[TableTriggerBuilder] 请把 tableTop 指到桌面变换(Transform)。");
            return;
        }

        // 1) 创建触发器根物体（做桌面的子物体，继承桌面旋转）
        var root = new GameObject("TableTrigger");
        root.transform.SetParent(tableTop, false);
        root.transform.localPosition =
            new Vector3(localOffsetXZ.x, lift, localOffsetXZ.y);
        root.transform.localRotation = Quaternion.identity;

        // 2) BoxCollider 作为触发器（真正生效的区域）
        var box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = Vector3.zero;
        box.size   = new Vector3(size.x, 0.02f, size.y); // X/Z=范围，Y=厚度很薄即可

        // 3) 可见参考：Quad（尺寸= size，面朝上）
        if (createVisualQuad)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "VisualQuad";
            quad.transform.SetParent(root.transform, false);
            // Quad 的法线是 +Z，让它朝“上”（与桌面法线一致）
            quad.transform.localRotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
            quad.transform.localScale    = new Vector3(size.x, size.y, 1f);

            // 删掉自带的 Collider，避免挡住触发
            var c = quad.GetComponent<Collider>();
            if (c) DestroyImmediate(c);

            // 设置半透明 Unlit 材质（URP/内置都尽量兼容）
            var mr = quad.GetComponent<MeshRenderer>();
            mr.sharedMaterial = GetOrMakeMaterial(new Color(0.2f, 0.7f, 1f, 0.35f));
        }

        // 4) 可见参考：Unity 的 Plane 网格（10x10，需要按比例缩放）
        if (createUnityPlaneMesh)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Plane);
            p.name = "VisualPlaneMesh";
            p.transform.SetParent(root.transform, false);
            p.transform.localPosition = Vector3.zero;
            p.transform.localRotation = Quaternion.identity;
            p.transform.localScale    = new Vector3(size.x / 10f, 1f, size.y / 10f);

            var c = p.GetComponent<Collider>();
            if (c) DestroyImmediate(c);

            var mr = p.GetComponent<MeshRenderer>();
            mr.sharedMaterial = planeMaterial ? planeMaterial : GetOrMakeMaterial(new Color(0.2f, 0.7f, 1f, 0.35f));
        }

        Debug.Log("✅ 已在桌面上创建触发平面：TableTrigger（包含 BoxCollider isTrigger）");
    }

    // 生成一个尽量兼容 URP/内置的 Unlit/半透明材质
    Material GetOrMakeMaterial(Color tint)
    {
        if (planeMaterial) return planeMaterial;

        Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (!sh) sh = Shader.Find("Unlit/Color");
        if (!sh) sh = Shader.Find("Standard");

        var m = new Material(sh);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", tint);
        if (m.HasProperty("_Color"))     m.SetColor("_Color",     tint);
        return m;
    }
}

