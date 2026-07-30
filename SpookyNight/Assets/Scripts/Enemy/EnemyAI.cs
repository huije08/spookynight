using UnityEngine;

public class EnemyAI : EnemyBase
{
    private Transform player;

    private enum State { Chase, Attack }
    private State currentState = State.Chase;

    protected override void Start()
    {
        base.Start();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        currentState = distToPlayer <= attackRange ? State.Attack : State.Chase;

        switch (currentState)
        {
            case State.Chase:
                ChasePlayer();
                break;
            case State.Attack:
                TryAttack();
                break;
        }
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // 플레이어 바라보기 (Y축만)
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }

    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats != null)
            playerStats.TakeDamage(attackDamage);
    }
}
