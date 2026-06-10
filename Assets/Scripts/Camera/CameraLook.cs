using MilkShake;
using UnityEngine;

/// <summary>
/// FPS 카메라 마우스 룩 컴포넌트.
/// 이 스크립트는 어느 오브젝트에 부착해도 되지만,
/// 인스펙터에서 아래 세 참조를 반드시 올바르게 할당해야 합니다.
///
/// 구조:
///   Player        ← playerBody 슬롯에 할당 (수평 Yaw 회전)
///   └─ CameraHolder ← cameraHolder 슬롯에 할당 (수직 Pitch 회전)
///       └─ Main Camera ← shaker 슬롯에 할당 (Shaker 컴포넌트)
/// </summary>
public class CameraLook : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // 참조
    // ──────────────────────────────────────────────
    [Header("참조")]
    [Tooltip("Player Transform — 좌우(Yaw) 회전에 사용됩니다.")]
    [SerializeField] private Transform playerBody;

    [Tooltip("CameraHolder Transform — 상하(Pitch) 회전에 사용됩니다.")]
    [SerializeField] private Transform cameraHolder;

    [Tooltip("Main Camera에 부착된 Shaker 컴포넌트")]
    [SerializeField] private Shaker shaker;

    // ──────────────────────────────────────────────
    // 마우스 감도 & 클램프
    // ──────────────────────────────────────────────
    [Header("감도")]
    [SerializeField] private float sensitivityX = 2f;
    [SerializeField] private float sensitivityY = 2f;

    [Header("수직 회전 제한 (도)")]
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch =  80f;

    // ──────────────────────────────────────────────
    // 커서 설정
    // ──────────────────────────────────────────────
    [Header("커서")]
    [SerializeField] private bool lockCursorOnStart = true;

    // ──────────────────────────────────────────────
    // Shake 프리셋 (인스펙터에서 ShakePreset 에셋 할당)
    // ──────────────────────────────────────────────
    [Header("Shake 프리셋")]
    [Tooltip("폭발 등 충격 효과용 Shake Preset")]
    [SerializeField] private ShakePreset explosionShake;

    [Tooltip("착지 시 Shake Preset")]
    [SerializeField] private ShakePreset landShake;

    [Tooltip("발걸음 Shake Preset")]
    [SerializeField] private ShakePreset footstepShake;

    // ──────────────────────────────────────────────
    // 내부 상태
    // ──────────────────────────────────────────────
    private float _pitchAngle;   // 수직(X) 누적 회전값

    // ──────────────────────────────────────────────
    // 초기화
    // ──────────────────────────────────────────────
    private void Awake()
    {
        if (lockCursorOnStart)
            SetCursorLock(true);
    }

    // ──────────────────────────────────────────────
    // 매 프레임 업데이트
    // ──────────────────────────────────────────────
    private void Update()
    {
        HandleLook();
        HandleCursorToggle();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY;

        // Player Body: 수평(Yaw) 회전
        playerBody.Rotate(Vector3.up, mouseX, Space.Self);

        // CameraHolder: 수직(Pitch) 회전 (위·아래 클램프)
        _pitchAngle -= mouseY;
        _pitchAngle   = Mathf.Clamp(_pitchAngle, minPitch, maxPitch);
        cameraHolder.localRotation = Quaternion.Euler(_pitchAngle, 0f, 0f);
    }

    /// <summary>ESC 키로 커서 잠금/해제 토글</summary>
    private void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SetCursorLock(Cursor.lockState != CursorLockMode.Locked);
    }

    private static void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !locked;
    }

    // ──────────────────────────────────────────────
    // 공개 Shake 헬퍼 메서드
    // (외부 스크립트에서 바로 호출 가능)
    // ──────────────────────────────────────────────

    /// <summary>폭발 카메라 쉐이크</summary>
    public ShakeInstance ShakeExplosion()
    {
        if (shaker == null || explosionShake == null) return null;
        return shaker.Shake(explosionShake);
    }

    /// <summary>착지 카메라 쉐이크</summary>
    public ShakeInstance ShakeLand()
    {
        if (shaker == null || landShake == null) return null;
        return shaker.Shake(landShake);
    }

    /// <summary>발걸음 카메라 쉐이크</summary>
    public ShakeInstance ShakeFootstep()
    {
        if (shaker == null || footstepShake == null) return null;
        return shaker.Shake(footstepShake);
    }

    /// <summary>
    /// 임의의 ShakePreset으로 쉐이크 재생.
    /// 예: cameraLook.PlayShake(myPreset);
    /// </summary>
    public ShakeInstance PlayShake(ShakePreset preset)
    {
        if (shaker == null || preset == null) return null;
        return shaker.Shake(preset);
    }
}
