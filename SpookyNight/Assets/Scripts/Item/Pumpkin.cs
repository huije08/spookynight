using UnityEngine;

public class Pumpkin : MonoBehaviour
{
    public int value = 5;
    public float pickupRange = 1.5f;
    public float moveSpeed = 8f;

    private Transform player;
    private bool isMovingToPlayer = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= pickupRange)
            isMovingToPlayer = true;

        if (isMovingToPlayer)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );

            if (dist <= 0.3f)
                Collect();
        }
    }

    void Collect()
    {
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.pumpkins += value;
            UIManager.Instance.UpdatePumpkin(playerStats.pumpkins);
            Debug.Log($"호박 +{value}개! 총 호박: {playerStats.pumpkins}개");
        }

        Destroy(gameObject);
    }
}
