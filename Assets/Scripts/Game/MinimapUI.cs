using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dark Deception 스타일 미니맵 UI.
///
/// ▶ 동작 방식
///   - 사전 제작된 맵 스프라이트 이미지를 사용합니다 (렌더 텍스처 없음).
///   - 맵 이미지 전체가 이동/회전하여 플레이어가 항상 뷰포트 중앙·위방향에 위치합니다.
///   - 링 도트는 맵 이미지와 함께 회전하므로 상대 위치가 정확하게 유지됩니다.
///   - 플레이어 도트는 항상 뷰포트 정중앙에 고정됩니다.
///
/// ▶ 부착 위치
///   이 스크립트는 씬의 빈 게임오브젝트(MinimapManager)에 부착하세요.
///   미니맵 패널 캔버스(World Space)는 CameraHolder의 자식으로 배치하면
///   카메라가 회전할 때 자연스럽게 같이 움직입니다.
/// </summary>
public class MinimapUI : MonoBehaviour
{
    public static MinimapUI Instance { get; private set; }

    // ──────────────────────────────────────────────
    // 월드 좌표 → 맵 이미지 변환 기준
    // ──────────────────────────────────────────────
    [Header("── 맵 월드 범위 설정 ──────────────────")]
    [Tooltip("맵 스프라이트 이미지의 왼쪽 아래 모서리에 해당하는 월드 좌표 (X, Z).\n" +
             "예) 맵이 X:-100 ~ X:100, Z:-100 ~ Z:100 이라면 (-100, -100) 입력.")]
    [SerializeField] private Vector2 worldMin = new Vector2(-50f, -50f);

    [Tooltip("맵 스프라이트 이미지의 오른쪽 위 모서리에 해당하는 월드 좌표 (X, Z).\n" +
             "예) 맵이 X:-100 ~ X:100, Z:-100 ~ Z:100 이라면 (100, 100) 입력.")]
    [SerializeField] private Vector2 worldMax = new Vector2(50f, 50f);

    // ──────────────────────────────────────────────
    // UI 오브젝트 참조
    // ──────────────────────────────────────────────
    [Header("── UI 오브젝트 참조 ───────────────────")]
    [Tooltip("[필수] MapRoot RectTransform.\n" +
             "계층: MapViewport(RectMask2D) > MapRoot > MapImage, DotContainer\n" +
             "MapRoot 전체가 이동·회전하여 플레이어 위치와 방향을 표현합니다.")]
    [SerializeField] private RectTransform mapRoot;

    [Tooltip("[필수] 맵 스프라이트를 표시하는 Image 컴포넌트.\n" +
             "MapRoot의 직속 자식 오브젝트에 부착되어 있어야 합니다.")]
    [SerializeField] private Image mapImage;

    [Tooltip("[필수] 링 도트들이 생성될 부모 RectTransform.\n" +
             "MapRoot의 자식이어야 합니다 (MapImage와 형제 관계).")]
    [SerializeField] private RectTransform dotContainer;

    [Tooltip("[필수] 항상 뷰포트 중앙에 고정되는 플레이어 도트 RectTransform.\n" +
             "MapViewport의 자식(MapRoot와 형제)으로 배치하세요.\n" +
             "위를 가리키는 삼각형/화살표 스프라이트를 사용하면 방향감이 생깁니다.")]
    [SerializeField] private RectTransform playerDot;

    [Tooltip("[선택] 링 남은 개수를 표시할 TextMeshPro 텍스트.")]
    [SerializeField] private TMP_Text ringCountText;

    [Tooltip("[선택] 기울기 효과를 적용할 최상위 패널 RectTransform.\n" +
             "이 오브젝트 전체가 마우스 움직임에 따라 살짝 기울어집니다.")]
    [SerializeField] private RectTransform tiltPanel;

    // ──────────────────────────────────────────────
    // 플레이어 참조
    // ──────────────────────────────────────────────
    [Header("── 플레이어 참조 ──────────────────────")]
    [Tooltip("[필수] 플레이어의 Transform (위치 계산에 사용).")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("[필수] 좌우(Yaw) 회전을 읽을 Transform.\n" +
             "보통 Player 루트 오브젝트. 이 Y 회전값을 기반으로 맵이 회전합니다.")]
    [SerializeField] private Transform playerYawTransform;

    // ──────────────────────────────────────────────
    // 맵 스케일
    // ──────────────────────────────────────────────
    [Header("── 맵 표시 크기 ──────────────────────")]
    [Tooltip("MapRoot(=맵 이미지)의 픽셀 크기.\n" +
             "월드 전체(worldMin~worldMax)를 이 크기(px)로 표현합니다.\n" +
             "값이 클수록 맵이 세밀하게 보입니다. 보통 512~1024.")]
    [SerializeField] private float mapPixelSize = 512f;

    // ──────────────────────────────────────────────
    // 도트 스타일
    // ──────────────────────────────────────────────
    [Header("── 링 도트 스타일 ───────────────────")]
    [Tooltip("링 위치 표시 스프라이트. 비워두면 흰 사각형.")]
    [SerializeField] private Sprite ringDotSprite;

    [Tooltip("링 도트 색상. 기본값: 보라색")]
    [SerializeField] private Color ringDotColor = new Color(0.55f, 0.25f, 1f, 1f);

    [Tooltip("링 도트 크기 (픽셀)")]
    [SerializeField] private Vector2 ringDotSize = new Vector2(12f, 12f);

    // ──────────────────────────────────────────────
    // 기울기 효과
    // ──────────────────────────────────────────────
    [Header("── 기울기 효과 ──────────────────────")]
    [Tooltip("마우스 이동 시 패널이 기울어지는 최대 각도(도).\n0이면 효과 없음.")]
    [SerializeField] private float tiltAmount = 5f;

    [Tooltip("기울기 보간 속도. 값이 클수록 빠르게 반응.")]
    [SerializeField] private float tiltSmoothing = 6f;

    // ──────────────────────────────────────────────
    // 줌
    // ──────────────────────────────────────────────
    [Header("── 줌 ────────────────────────────────")]
    [Tooltip("맵 확대 배율. 1 = 기본, 2 = 2배 확대 (플레이어 주변을 더 크게 표시).\n값을 높일수록 좁은 범위를 크게 보여줍니다.")]
    [SerializeField] private float zoomScale = 2f;

    // ──────────────────────────────────────────────
    // 런타임 상태
    // ──────────────────────────────────────────────
    private readonly Dictionary<Transform, RectTransform> _ringDots = new();
    private int _remainingRings;
    private int _totalRings;
    private Vector2 _currentTilt;

    // ══════════════════════════════════════════════
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // MapRoot 크기 설정
        if (mapRoot != null)
            mapRoot.sizeDelta = new Vector2(mapPixelSize, mapPixelSize);

        // MapImage 크기도 동일하게
        if (mapImage != null)
            mapImage.rectTransform.sizeDelta = new Vector2(mapPixelSize, mapPixelSize);

        // 씬의 ring 태그 오브젝트 자동 탐색 및 도트 생성
        GameObject[] rings = GameObject.FindGameObjectsWithTag("ring");
        _totalRings     = rings.Length;
        _remainingRings = _totalRings;

        foreach (var r in rings)
        {
            RectTransform dot = CreateRingDot(r.transform);
            _ringDots[r.transform] = dot;
        }

        UpdateCountText();
    }

    private void LateUpdate()
    {
        if (playerTransform == null || mapRoot == null) return;

        // 1) 플레이어의 월드 좌표 → 맵 픽셀 좌표
        Vector2 playerMapPos = WorldToMapPixel(playerTransform.position);

        // 2) 회전각: 플레이어 진행 방향이 항상 위(↑)를 향하도록
        float yaw = playerYawTransform != null ? playerYawTransform.eulerAngles.y : 0f;
        Quaternion rot = Quaternion.Euler(0f, 0f, yaw);

        // 3) MapRoot 이동: 줌 스케일을 반영해 플레이어가 항상 뷰포트 중앙에 오도록
        mapRoot.localRotation = rot;
        mapRoot.anchoredPosition = -(rot * playerMapPos) * zoomScale;

        // 4) 줌 스케일 적용
        mapRoot.localScale = Vector3.one * zoomScale;

        // 5) 기울기 효과
        ApplyTilt();
    }

    // ──────────────────────────────────────────────
    // 공개 메서드
    // ──────────────────────────────────────────────

    /// <summary>
    /// RingCollectible에서 호출. 해당 링의 도트를 지도에서 제거합니다.
    /// </summary>
    public void RemoveRingDot(Transform ringTransform)
    {
        if (_ringDots.TryGetValue(ringTransform, out var dot))
        {
            Destroy(dot.gameObject);
            _ringDots.Remove(ringTransform);
        }
        _remainingRings = Mathf.Max(0, _remainingRings - 1);
        UpdateCountText();
    }

    // ──────────────────────────────────────────────
    // 내부 메서드
    // ──────────────────────────────────────────────

    /// <summary>
    /// 월드 좌표(X, Z) → 맵 이미지 픽셀 좌표 변환.
    /// 맵 이미지 중앙이 원점(0,0).
    /// X/Z 월드 범위가 다를 경우 각각 비율에 맞게 픽셀 크기를 계산합니다.
    /// </summary>
    private Vector2 WorldToMapPixel(Vector3 worldPos)
    {
        float worldWidth  = worldMax.x - worldMin.x;
        float worldHeight = worldMax.y - worldMin.y;

        // 긴 쪽을 mapPixelSize 기준으로 잡고 짧은 쪽은 비율에 맞게 축소
        float maxRange = Mathf.Max(worldWidth, worldHeight);
        float pixelPerUnit = mapPixelSize / maxRange;

        float nx = Mathf.InverseLerp(worldMin.x, worldMax.x, worldPos.x);
        float ny = Mathf.InverseLerp(worldMin.y, worldMax.y, worldPos.z);

        float px = (nx - 0.5f) * worldWidth  * pixelPerUnit;
        float py = (ny - 0.5f) * worldHeight * pixelPerUnit;

        return new Vector2(px, py);
    }

    private RectTransform CreateRingDot(Transform ringTransform)
    {
        var go = new GameObject("RingDot_" + ringTransform.name, typeof(Image));
        go.transform.SetParent(dotContainer, false);

        var img = go.GetComponent<Image>();
        if (ringDotSprite != null) img.sprite = ringDotSprite;
        img.color = ringDotColor;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta   = ringDotSize;
        rt.anchorMin   = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot       = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = WorldToMapPixel(ringTransform.position);
        return rt;
    }

    private void UpdateCountText()
    {
        if (ringCountText != null)
            ringCountText.text = _remainingRings.ToString();
    }

    private void ApplyTilt()
    {
        if (tiltPanel == null || tiltAmount <= 0f) return;
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");
        Vector2 target   = new Vector2(-my * tiltAmount, mx * tiltAmount);
        _currentTilt     = Vector2.Lerp(_currentTilt, target, Time.deltaTime * tiltSmoothing);
        tiltPanel.localRotation = Quaternion.Euler(_currentTilt.x, _currentTilt.y, 0f);
    }
}
