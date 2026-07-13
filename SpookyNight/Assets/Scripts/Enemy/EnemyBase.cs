using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("기본 스탯")]
    public float maxHp = 100f;
    public float moveSpeed = 3f;
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public int expDrop = 10;

    [Header("드롭")]
    public GameObject pumpkinPrefab;
    public int pumpkinDrop = 1;

    protected float currentHp;
    protected float nextAttackTime = 0f;
    protected bool isDead = false;

    protected virtual void Start()
    {
        currentHp = maxHp;
    }

    // ─── 피격 ────────────────────────────────────────────────
    public virtual void TakeDamage(float dmg)
    {
        if (isDead) return;

        currentHp -= dmg;
        StartCoroutine(HitFlash());

        if (currentHp <= 0)
            Die();
    }

    // ─── 사망 ────────────────────────────────────────────────
    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        PlayerStats player = FindObjectOfType<PlayerStats>();
        if (player != null)
            player.GainExp(expDrop);

        GameManager.Instance.OnEnemyKilled();
        DropPumpkins();
        WaveManager.Instance.OnEnemyDied();

        Destroy(gameObject);
    }

    // ─── 호박 드롭 ───────────────────────────────────────────
    void DropPumpkins()
    {
        if (pumpkinPrefab == null) return;

        for (int i = 0; i < pumpkinDrop; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                0f,
                Random.Range(-0.5f, 0.5f)
            );
            Instantiate(pumpkinPrefab, transform.position + offset, Quaternion.identity);
        }
    }

    // ─── 피격 이펙트 ─────────────────────────────────────────
    IEnumerator HitFlash()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend == null) yield break;

        Color original = rend.material.color;
        rend.material.color = Color.red;
        yield return new WaitForSeconds(0.08f);
        rend.material.color = original;
    }
}
