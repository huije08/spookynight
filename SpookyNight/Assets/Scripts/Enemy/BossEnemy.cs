using System.Collections;
using UnityEngine;

public class BossEnemy : EnemyBase
{
    [Header("돌진 설정")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 4f;

    [Header("삼지창 설정")]
    public GameObject tridentPrefab;
    public int tridentCount = 3;
    public float tridentSpread = 30f;
    public float tridentSpeed = 10f;
    public float tridentCooldown = 5f;
    public Transform throwPoint;

    [Header("경고 사운드")]
    public AudioClip dashWarnSound;         // 돌진 경고음
    public AudioClip tridentWarnSound;      // 삼지창 경고음
    public AudioClip dashSound;             // 돌진 사운드
    public AudioClip tridentThrowSound;     // 삼지창 투척 사운드
    private AudioSource audioSource;

    [Header("애니메이션")]
    public Animator animator;               // 보스 Animator

    // Animator 파라미터 이름
    private readonly string ANIM_DASH_READY = "DashReady";
    private readonly string ANIM_DASH = "Dash";
    private readonly string ANIM_THROW_READY = "ThrowReady";
    private readonly string ANIM_THROW = "Throw";
    private readonly string ANIM_WALK = "Walk";

    [Header("경고 이펙트")]
    public GameObject dashWarningEffect;    // 돌진 경고 이펙트 (빨간 원 등)
    public GameObject throwWarningEffect;   // 삼지창 경고 이펙트

    [Header("타이밍")]
    public float dashWarnDuration = 1.0f; // 돌진 경고 시간
    public float throwWarnDuration = 1.2f; // 삼지창 경고 시간

    private Transform player;
    private bool isDashing = false;
    private float nextDashTime = 0f;
    private float nextTridentTime = 3f;

    protected override void Start()
    {
        base.Start();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        audioSource = gameObject.AddComponent<AudioSource>();

        // 경고 이펙트 시작할 때 끄기
        if (dashWarningEffect != null)
            dashWarningEffect.SetActive(false);
        if (throwWarningEffect != null)
            throwWarningEffect.SetActive(false);
    }

    void Update()
    {
        if (isDead || player == null) return;

        if (!isDashing)
        {
            if (Time.time >= nextTridentTime)
            {
                StartCoroutine(ThrowTrident());
                nextTridentTime = Time.time + tridentCooldown;
                return;
            }

            if (Time.time >= nextDashTime)
            {
                StartCoroutine(Dash());
                nextDashTime = Time.time + dashCooldown;
                return;
            }

            ChasePlayer();
        }
    }

    void ChasePlayer()
    {
        // 걷기 애니메이션
        if (animator != null)
            animator.SetBool(ANIM_WALK, true);

        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.LookAt(new Vector3(
            player.position.x,
            transform.position.y,
            player.position.z));
    }

    // ─── 돌진 ────────────────────────────────────────────────
    IEnumerator Dash()
    {
        isDashing = true;

        // 1. 플레이어 바라보기
        transform.LookAt(new Vector3(
            player.position.x,
            transform.position.y,
            player.position.z));

        // 2. 걷기 애니메이션 끄기
        if (animator != null)
            animator.SetBool(ANIM_WALK, false);

        // 3. 경고 사운드 재생
        PlaySound(dashWarnSound);

        // 4. 돌진 준비 애니메이션 + 경고 이펙트
        if (animator != null)
            animator.SetTrigger(ANIM_DASH_READY);

        if (dashWarningEffect != null)
            dashWarningEffect.SetActive(true);

        // 5. 경고 시간 대기 (이 시간 동안 준비 자세)
        yield return new WaitForSeconds(dashWarnDuration);

        // 6. 경고 이펙트 끄기
        if (dashWarningEffect != null)
            dashWarningEffect.SetActive(false);

        // 7. 돌진 사운드 + 애니메이션
        PlaySound(dashSound);
        if (animator != null)
            animator.SetTrigger(ANIM_DASH);

        // 8. 실제 돌진
        Vector3 dashDirection = (player.position - transform.position).normalized;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            transform.position += dashDirection * dashSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;

            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < attackRange)
            {
                PlayerStats ps = player.GetComponent<PlayerStats>();
                if (ps != null)
                    ps.TakeDamage(attackDamage * 2f);
                break;
            }

            yield return null;
        }

        // 9. 돌진 끝 → 걷기 복귀
        if (animator != null)
            animator.SetBool(ANIM_WALK, true);

        isDashing = false;
    }

    // ─── 삼지창 투척 ─────────────────────────────────────────
    IEnumerator ThrowTrident()
    {
        isDashing = true;   // 투척 중 이동 막기

        // 1. 플레이어 바라보기
        transform.LookAt(new Vector3(
            player.position.x,
            transform.position.y,
            player.position.z));

        // 2. 걷기 애니메이션 끄기
        if (animator != null)
            animator.SetBool(ANIM_WALK, false);

        // 3. 경고 사운드
        PlaySound(tridentWarnSound);

        // 4. 투척 준비 애니메이션 + 경고 이펙트
        if (animator != null)
            animator.SetTrigger(ANIM_THROW_READY);

        if (throwWarningEffect != null)
            throwWarningEffect.SetActive(true);

        // 5. 경고 시간 대기
        yield return new WaitForSeconds(throwWarnDuration);

        // 6. 경고 이펙트 끄기
        if (throwWarningEffect != null)
            throwWarningEffect.SetActive(false);

        // 7. 투척 사운드 + 애니메이션
        PlaySound(tridentThrowSound);
        if (animator != null)
            animator.SetTrigger(ANIM_THROW);

        if (tridentPrefab == null)
        {
            isDashing = false;
            yield break;
        }

        // 8. 삼지창 발사
        Vector3 origin = throwPoint != null
            ? throwPoint.position
            : transform.position + Vector3.up;

        for (int i = 0; i < tridentCount; i++)
        {
            float offset = 0f;
            if (tridentCount > 1)
                offset = Mathf.Lerp(
                    -tridentSpread / 2f,
                     tridentSpread / 2f,
                    (float)i / (tridentCount - 1));

            Vector3 direction = Quaternion.Euler(0f, offset, 0f)
                * (player.position - origin).normalized;

            GameObject trident = Instantiate(
                tridentPrefab, origin, Quaternion.identity);
            trident.transform.forward = direction;

            Rigidbody rb = trident.GetComponent<Rigidbody>();
            if (rb != null)
                rb.velocity = direction * tridentSpeed;
        }

        // 9. 투척 끝 → 걷기 복귀
        yield return new WaitForSeconds(0.3f);
        if (animator != null)
            animator.SetBool(ANIM_WALK, true);

        isDashing = false;
    }

    // ─── 사운드 재생 ─────────────────────────────────────────
    void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

    // ─── 사망 ────────────────────────────────────────────────
    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();

        if (dashWarningEffect != null)
            dashWarningEffect.SetActive(false);
        if (throwWarningEffect != null)
            throwWarningEffect.SetActive(false);

        DropPumpkins();
        WaveManager.Instance.OnBossDefeated();
        Destroy(gameObject);
    }
}