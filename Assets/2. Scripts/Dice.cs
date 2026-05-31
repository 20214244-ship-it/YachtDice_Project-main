using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dice : MonoBehaviour
{
    public int  Value     { get; private set; } = 1;
    public bool IsKept    { get; private set; } = false;
    public bool IsRolling { get; private set; } = false;

    [Header("고정 시 이동 방향 (startPosition 기준 상대좌표)")]
    public Vector3 keptOffset = new Vector3(0f, 0f, -3f);

    [Header("색상")]
    public Color normalColor = Color.white;
    public Color keptColor   = new Color(1f, 0.85f, 0.25f);

    private Rigidbody rb;
    private Renderer  rend;
    private Vector3   startPosition;

    private static readonly (Vector3 dir, int val)[] FaceMap =
    {
        (Vector3.up,      1),
        (Vector3.down,    6),
        (Vector3.forward, 2),
        (Vector3.back,    5),
        (Vector3.right,   3),
        (Vector3.left,    4),
    };

    void Awake()
    {
        rb            = GetComponent<Rigidbody>();
        rend          = GetComponent<Renderer>();
        startPosition = transform.position;
        rb.isKinematic = true;
    }

    void Update()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (Camera.main == null) return;
        if (!GameManager.Instance.CanPlayerClick()) return;

        Ray        ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            ToggleKept();
    }

    public void Roll()
    {
        if (IsKept) return;
        StartCoroutine(RollCoroutine());
    }

    private IEnumerator RollCoroutine()
    {
        IsRolling = true;

        // ★ isKinematic 먼저 false로 → 그 다음 velocity 초기화 (순서 중요!)
        rb.isKinematic   = false;
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = startPosition + Vector3.up * 1.5f;

        rb.AddForce(new Vector3(
            Random.Range(-1f,   1f),
            Random.Range( 0.5f, 1.5f),
            Random.Range(-1f,   1f)), ForceMode.Impulse);
        rb.AddTorque(new Vector3(
            Random.Range(-12f, 12f),
            Random.Range(-12f, 12f),
            Random.Range(-12f, 12f)), ForceMode.Impulse);

        yield return new WaitForSeconds(0.5f);
        float timeout = 3f;
        while (timeout > 0 &&
               (rb.linearVelocity.magnitude > 0.05f || rb.angularVelocity.magnitude > 0.05f))
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        // ★ 멈춤 처리: isKinematic true 먼저 → velocity는 이미 물리가 0에 가깝게 만들었으므로 OK
        rb.isKinematic = true;

        SnapToSlot();
        Value     = ReadTopFace();
        Debug.Log(gameObject.name + " : " + Value);
        IsRolling = false;
    }

    private void ToggleKept()
    {
        IsKept = !IsKept;
        StopAllCoroutines();
        IsRolling = false;

        // ★ isKinematic true 먼저 → 그 다음 위치 이동
        rb.isKinematic = true;

        if (IsKept)
        {
            transform.position = startPosition + keptOffset;
            transform.rotation = Quaternion.identity;
            SetColor(keptColor);
        }
        else
        {
            transform.position = startPosition + Vector3.up * 0.5f;
            transform.rotation = Quaternion.identity;
            SetColor(normalColor);
        }
    }

    public void ResetForNewTurn()
    {
        StopAllCoroutines();
        IsKept    = false;
        IsRolling = false;

        // ★ isKinematic true 먼저 → 그 다음 나머지
        rb.isKinematic = true;

        transform.position = startPosition + Vector3.up * 0.5f;
        transform.rotation = Quaternion.identity;
        SetColor(normalColor);
    }

    private void SnapToSlot()
    {
        Vector3 p = transform.position;
        p.x = startPosition.x;
        p.z = startPosition.z;
        transform.position = p;

        Vector3 e = transform.eulerAngles;
        e.x = Mathf.Round(e.x / 90f) * 90f;
        e.y = Mathf.Round(e.y / 90f) * 90f;
        e.z = Mathf.Round(e.z / 90f) * 90f;
        transform.eulerAngles = e;
    }

    private int ReadTopFace()
    {
        float maxDot = -Mathf.Infinity;
        int   result = 1;
        foreach (var (dir, val) in FaceMap)
        {
            float dot = Vector3.Dot(transform.TransformDirection(dir), Vector3.up);
            if (dot > maxDot) { maxDot = dot; result = val; }
        }
        return result;
    }

    private void SetColor(Color c)
    {
        if (rend != null) rend.material.color = c;
    }
}
