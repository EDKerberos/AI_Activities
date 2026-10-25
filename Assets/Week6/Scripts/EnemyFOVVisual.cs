using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class EnemyFOVVisual : MonoBehaviour
{
    [Header("Vision Shape")]
    [SerializeField] private Transform eyePoint;
    [SerializeField] private float viewDistance;

    [SerializeField, Range(0f, 360f)]
    private float fieldOfView;

    [SerializeField, Range(2, 100)]
    private int rayCount;

    [Header("Layers")]
    [SerializeField] private LayerMask obstructionMask;

    private Mesh visionMesh;

    private void Awake()
    {
        visionMesh = new Mesh
        {
            name = "FOV Mesh"
        };

        GetComponent<MeshFilter>().sharedMesh = visionMesh;
    }

    private void LateUpdate()
    {
        UpdateVisionMesh();
    }

    private void UpdateVisionMesh()
    {
        Vector3 origin = eyePoint != null
            ? eyePoint.position
            : transform.position + Vector3.up * 1.5f;

        Vector3 forward = Vector3.ProjectOnPlane(
            transform.forward, Vector3.up).normalized;

        if (forward.sqrMagnitude < 0.001f)
            return;

        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        // The mesh is built in the visualizer's local space.
        vertices[0] = transform.InverseTransformPoint(origin);

        float startAngle = -fieldOfView * 0.5f;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle +
                fieldOfView * i / rayCount;

            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f) * forward;

            Vector3 endpoint = origin + direction * viewDistance;

            if (Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                viewDistance,
                obstructionMask,
                QueryTriggerInteraction.Ignore))
            {
                endpoint = hit.point;
            }

            // Keep the fan flat at eye height.
            endpoint.y = origin.y;

            vertices[i + 1] =
                transform.InverseTransformPoint(endpoint);
        }

        for (int i = 0; i < rayCount; i++)
        {
            int triangle = i * 3;

            triangles[triangle] = 0;
            triangles[triangle + 1] = i + 1;
            triangles[triangle + 2] = i + 2;
        }

        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateBounds();
        visionMesh.RecalculateNormals();
    }

    private void OnDestroy()
    {
        if (visionMesh != null)
            Destroy(visionMesh);
    }
}