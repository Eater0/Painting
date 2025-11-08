using UnityEngine;

public class Canvas : MonoBehaviour
{
    [SerializeField] ComputeShader _computeShader;
    [SerializeField] Paintbrush _paintbrush;

    int _threadGroupssX;
    GraphicsBuffer _meshVertices;
    GraphicsBuffer _Color;

    void Awake()
    {
        _computeShader = Instantiate(_computeShader);
    }

    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        _threadGroupssX = Mathf.CeilToInt(mesh.vertexCount / 64f);

        Vector3[] defaultColors = new Vector3[mesh.vertexCount];
        for (int i = 0; i < defaultColors.Length; i++)
            defaultColors[i] = Vector3.one;

        SetupBuffers(mesh, defaultColors);
        SetupComputeShader();

        Material _material = GetComponent<MeshRenderer>().material;
        _material.SetBuffer("_Color", _Color);
    }

    void Update()
    {
        _computeShader.SetVector("_PaintingBrushPosition", _paintbrush.transform.position);
        _computeShader.Dispatch(0, _threadGroupssX, 1, 1);
    }

    void SetupBuffers(Mesh mesh, Vector3[] defaultColors)
    {
        _meshVertices = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.vertexCount, sizeof(float) * 3);
        _meshVertices.SetData(mesh.vertices);

        _Color = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.vertexCount, sizeof(float) * 3);
        _Color.SetData(defaultColors);
    }

    void SetupComputeShader()
    {
        _computeShader.SetBuffer(0, "_Vertices", _meshVertices);
        _computeShader.SetBuffer(0, "_Color", _Color);

        _computeShader.SetInt("_VertexCount", _meshVertices.count);
        _computeShader.SetMatrix("_Object", transform.localToWorldMatrix);
        _computeShader.SetFloat("_Radius", _paintbrush._radius);
        _computeShader.SetFloat("_Power", 1 - _paintbrush._power);
    }

    void OnDisable()
    {
        _meshVertices?.Dispose();
        _meshVertices = null;
        _Color?.Dispose();
        _Color = null;
    }
}
