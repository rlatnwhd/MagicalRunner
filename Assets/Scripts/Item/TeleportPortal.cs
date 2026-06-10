using UnityEngine;

/// <summary>
/// 순간이동 포털 컴포넌트.
/// 두 포털 프리팹을 서로 연결해두면, 한 쪽에 닿으면 반대쪽으로 이동합니다.
///
/// ▶ 설정 방법
///   Portal_A, Portal_B 두 프리팹을 씬에 배치하고
///   각각 인스펙터의 "연결된 포털" 슬롯에 상대방을 드래그하세요.
///   (A의 linkedPortal = B, B의 linkedPortal = A)
/// </summary>
public class TeleportPortal : MonoBehaviour
{
    [Header("포털 연결")]
    [Tooltip("이 포털과 연결된 반대쪽 포털 Transform.\n" +
             "플레이어가 이 포털에 닿으면 linkedPortal 위치로 순간이동합니다.")]
    [SerializeField] private Transform linkedPortal;

    [Header("이동 오프셋")]
    [Tooltip("도착 포털에서 플레이어가 생성될 로컬 오프셋.\n" +
             "포털을 관통하지 않도록 앞쪽(Z+)으로 살짝 밀어줍니다.")]
    [SerializeField] private Vector3 exitOffset = new Vector3(0f, 0f, 1.5f);

    [Header("이펙트 (선택)")]
    [Tooltip("순간이동 시 입구/출구에 재생할 파티클 (없으면 생략)")]
    [SerializeField] private ParticleSystem teleportEffect;

    // 연속 텔레포트 방지용 쿨타임
    private float _cooldown = 0f;
    private const float CooldownDuration = 1f;

    private void Update()
    {
        if (_cooldown > 0f)
            _cooldown -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_cooldown > 0f) return;
        if (linkedPortal == null)
        {
            Debug.LogWarning($"[TeleportPortal] {name} 의 linkedPortal이 할당되지 않았습니다.");
            return;
        }

        // 도착 위치 = 연결된 포털 위치 + 로컬 오프셋
        Vector3 destination = linkedPortal.TransformPoint(exitOffset);

        // CharacterController가 있으면 비활성화 후 이동 (Move 없이 position 직접 변경)
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        other.transform.position = destination;
        other.transform.rotation = linkedPortal.rotation;

        if (cc != null) cc.enabled = true;

        // 파티클 재생
        if (teleportEffect != null)
        {
            teleportEffect.Play();
            // 도착 포털의 이펙트도 재생
            var destEffect = linkedPortal.GetComponent<TeleportPortal>()?.teleportEffect;
            destEffect?.Play();
        }

        // 도착 포털의 쿨타임도 설정해 연속 왕복 방지
        var destPortal = linkedPortal.GetComponent<TeleportPortal>();
        if (destPortal != null)
            destPortal._cooldown = CooldownDuration;

        _cooldown = CooldownDuration;
    }
}
