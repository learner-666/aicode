using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private string attackTrigger = "Pickup";
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 10;

    private Animator animator;
    private float lastAttackTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (animator == null) return;

        lastAttackTime = Time.time;
        animator.SetTrigger(attackTrigger);

        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 origin = cam.transform.position;
        Vector3 direction = cam.transform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, attackRange))
        {
            Debug.Log("Hit: " + hit.collider.name);
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(attackDamage);
            }
        }
    }
}
