using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 스테이지 이동 포털 컴포넌트.
/// 플레이어가 접촉하면 링을 모두 수집했는지 확인하고 지정 씬으로 이동합니다.
///
/// ▶ 씬 이름
///   - 메인 씬 : MainScene
///   - 게임 씬 : GameScene
/// </summary>
public class StagePortal : MonoBehaviour
{
    [Header("이동할 씬")]
    [Tooltip("링을 모두 수집했을 때 이동할 씬 이름.\n" +
             "기본값: MainScene (빌드 세팅에 등록되어 있어야 함)")]
    [SerializeField] private string targetSceneName = "MainScene";

    [Header("링 확인")]
    [Tooltip("true: 링을 모두 수집해야 씬 이동 허용.\n" +
             "false: 링 수집 여부 무관하게 항상 이동.")]
    [SerializeField] private bool requireAllRings = true;

    [Header("이펙트 (선택)")]
    [Tooltip("포털 진입 시 재생할 파티클 (없으면 생략)")]
    [SerializeField] private ParticleSystem enterEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (requireAllRings)
        {
            RingManager rm = RingManager.Instance;

            // RingManager가 없거나 링이 남아 있으면 이동 불가
            if (rm == null || rm.RemainingRings > 0)
            {
                // TODO: 말풍선 / 안내 UI 표시 (추후 구현)
                Debug.Log("[StagePortal] 아직 링이 남아 있습니다! 모든 링을 수집하세요.");
                return;
            }
        }

        enterEffect?.Play();
        SceneManager.LoadScene(targetSceneName);
    }
}
