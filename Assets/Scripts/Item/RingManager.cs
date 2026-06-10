using TMPro;
using UnityEngine;

/// <summary>
/// 씬 내 Ring 수집 현황을 관리하는 싱글턴 매니저.
/// 빈 게임오브젝트에 부착하세요.
///
/// 총 링 개수 결정 방식:
///   ▸ autoCountRings = true  : 씬 시작 시 태그 "ring" 오브젝트를 자동으로 카운트
///   ▸ autoCountRings = false : 인스펙터에서 totalRings 값을 직접 입력
/// </summary>
public class RingManager : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // 싱글턴
    // ──────────────────────────────────────────────
    public static RingManager Instance { get; private set; }

    // ──────────────────────────────────────────────
    // 인스펙터 설정
    // ──────────────────────────────────────────────
    [Header("링 개수 설정")]
    [Tooltip("true: 씬에서 태그 'ring' 오브젝트를 자동 카운트 / false: 아래 값 직접 입력")]
    [SerializeField] private bool autoCountRings = true;

    [Tooltip("autoCountRings가 false일 때 사용할 총 링 개수")]
    [SerializeField] private int totalRings = 10;

    [Header("UI 연결")]
    [Tooltip("남은 링 개수를 표시할 TextMeshPro 컴포넌트")]
    [SerializeField] private TMP_Text ringCountText;

    // ──────────────────────────────────────────────
    // 런타임 상태
    // ──────────────────────────────────────────────
    private int _remainingRings;

    // 외부에서 읽기 전용으로 접근 가능
    public int RemainingRings => _remainingRings;
    public int TotalRings     => totalRings;

    // ──────────────────────────────────────────────
    // 초기화
    // ──────────────────────────────────────────────
    private void Awake()
    {
        // 싱글턴 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 총 개수 결정
        if (autoCountRings)
            totalRings = GameObject.FindGameObjectsWithTag("ring").Length;

        _remainingRings = totalRings;
    }

    private void Start()
    {
        UpdateUI();
    }

    // ──────────────────────────────────────────────
    // 공개 메서드
    // ──────────────────────────────────────────────

    /// <summary>Ring이 수집될 때 RingCollectible에서 호출합니다.</summary>
    public void OnRingCollected()
    {
        _remainingRings = Mathf.Max(0, _remainingRings - 1);
        UpdateUI();

        if (_remainingRings == 0)
            OnAllRingsCollected();
    }

    // ──────────────────────────────────────────────
    // 내부 메서드
    // ──────────────────────────────────────────────
    private void UpdateUI()
    {
        if (ringCountText != null)
            ringCountText.text = $"{totalRings}";
    }

    private void OnAllRingsCollected()
    {
        // TODO: 모든 링 수집 완료 이벤트 처리
        Debug.Log("[RingManager] 모든 링을 수집했습니다!");
    }
}
