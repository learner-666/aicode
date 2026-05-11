using UnityEngine;

public class TargetEntity : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 30;

    private int currentHealth;
    private Transform healthBarFill;
    private TextMesh hpText;
    private GameManager gameManager;

    public void SetGameManager(GameManager gm)
    {
        gameManager = gm;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        CreateHealthBar();
    }

    private void CreateHealthBar()
    {
        GameObject barRoot = new GameObject("HealthBar");
        barRoot.transform.SetParent(transform);
        barRoot.transform.localPosition = new Vector3(0, 1.5f, 0);

        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bg.name = "BarBG";
        bg.transform.SetParent(barRoot.transform);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(0.8f, 0.08f, 0.08f);
        bg.transform.localRotation = Quaternion.identity;
        Destroy(bg.GetComponent<Collider>());
        Renderer bgRend = bg.GetComponent<Renderer>();
        bgRend.material.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);

        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fill.name = "BarFill";
        fill.transform.SetParent(barRoot.transform);
        fill.transform.localPosition = new Vector3(-0.4f, 0, 0);
        fill.transform.localScale = new Vector3(0.8f, 0.05f, 0.05f);
        fill.transform.localRotation = Quaternion.identity;
        fill.GetComponent<Renderer>().material.color = Color.red;
        Destroy(fill.GetComponent<Collider>());
        healthBarFill = fill.transform;

        GameObject textObj = new GameObject("HPText");
        textObj.transform.SetParent(barRoot.transform);
        textObj.transform.localPosition = new Vector3(0, -0.1f, 0);
        hpText = textObj.AddComponent<TextMesh>();
        hpText.text = currentHealth + "/" + maxHealth;
        hpText.fontSize = 24;
        hpText.characterSize = 0.04f;
        hpText.anchor = TextAnchor.MiddleCenter;
        hpText.color = Color.white;
    }

    private void Update()
    {
        BillboardHealthBar();
    }

    private void BillboardHealthBar()
    {
        Transform barRoot = transform.Find("HealthBar");
        if (barRoot == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 dir = cam.transform.position - barRoot.position;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            barRoot.rotation = Quaternion.LookRotation(-dir);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float percent = (float)currentHealth / maxHealth;
            healthBarFill.localScale = new Vector3(0.8f * percent, 0.05f, 0.05f);
            healthBarFill.localPosition = new Vector3(-0.4f + 0.8f * percent * 0.5f, 0, 0);
        }
        if (hpText != null)
        {
            hpText.text = currentHealth + "/" + maxHealth;
        }
    }

    private void Die()
    {
        if (gameManager != null)
        {
            gameManager.OnEntityDestroyed();
        }
        Destroy(gameObject);
    }
}
