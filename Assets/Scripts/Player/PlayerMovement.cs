using UnityEngine;

/// <summary>
/// WASD 이동을 처리하는 플레이어 이동 컴포넌트.
/// CharacterController가 필요합니다.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("중력 설정")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _isGrounded;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
        ApplyGravity();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A / D
        float vertical   = Input.GetAxis("Vertical");   // W / S

        // 플레이어의 로컬 방향 기준으로 이동 벡터 구성
        Vector3 moveDir = transform.right * horizontal + transform.forward * vertical;

        // 대각선 이동 시 속도 정규화
        if (moveDir.magnitude > 1f)
            moveDir.Normalize();

        _controller.Move(moveDir * moveSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        // 지면 체크: CharacterController 하단 구 위치
        Vector3 groundCheckPos = transform.position + _controller.center
                                 + Vector3.down * (_controller.height * 0.5f - _controller.radius);
        _isGrounded = Physics.CheckSphere(groundCheckPos, groundCheckRadius, groundLayer,
                                          QueryTriggerInteraction.Ignore);

        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f; // 지면에 붙어있도록 소량 하방 속도 유지

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(new Vector3(0f, _velocity.y * Time.deltaTime, 0f));
    }
}
