using UnityEngine;

/// <summary>
/// Ring 프리팹에 부착하는 컴포넌트.
/// 플레이어가 트리거에 진입하면 RingManager에 수집 사실을 알리고 자신을 파괴합니다.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class RingCollectible : MonoBehaviour
{
    [Header("회전")]
    [Tooltip("초당 회전 각도 (도)")]
    [SerializeField] private float rotateSpeed = 90f;
    [Tooltip("회전 축")]
    [SerializeField] private Vector3 rotateAxis = Vector3.up;

    [Header("오디오")]
    [Tooltip("링 수집 시 재생될 AudioClip")]
    [SerializeField] private AudioClip collectSound;
    [Tooltip("수집 사운드 볼륨 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 1f;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1f; // 3D 사운드
    }

    private void Update()
    {
        transform.Rotate(rotateAxis.normalized, rotateSpeed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // RingManager에 수집 알림
        RingManager.Instance?.OnRingCollected();

        // 미니맵에서 링 도트 제거
        MinimapUI.Instance?.RemoveRingDot(transform);

        // 오브젝트의 Renderer·Collider를 즉시 비활성화해 중복 트리거 방지
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>())  c.enabled = false;

        // 소리 재생 후 파괴 (AudioSource.PlayClipAtPoint 방식으로 소리가 잘리지 않게 처리)
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position, soundVolume);

        Destroy(gameObject);
    }
}
