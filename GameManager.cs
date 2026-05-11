using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public event Action OnGameOver;

    [Header("Game Over")]
    [SerializeField] private float gameOverDuration = 2f;

    private int remainingEntities;
    private bool gameOver;
    private float gameOverShowTime;

    public bool IsGameOver => gameOver;

    private void Start()
    {
        TargetEntity[] entities = FindObjectsOfType<TargetEntity>();
        remainingEntities = entities.Length;

        foreach (TargetEntity e in entities)
        {
            e.SetGameManager(this);
        }
    }

    public void OnEntityDestroyed()
    {
        remainingEntities--;
        if (remainingEntities <= 0 && !gameOver)
        {
            gameOver = true;
            gameOverShowTime = Time.time;
            OnGameOver?.Invoke();
        }
    }

    private void Update()
    {
        if (gameOver && Time.time - gameOverShowTime > gameOverDuration)
        {
            gameOver = false;
        }
    }

    private void OnGUI()
    {
        if (!gameOver) return;

        float elapsed = Time.time - gameOverShowTime;
        float alpha = Mathf.Clamp01(1f - elapsed / gameOverDuration);

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 48;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;

        Color c = Color.white;
        c.a = alpha;
        style.normal.textColor = c;

        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        GUI.Label(new Rect(cx - 150, cy - 30, 300, 60), "GAME OVER", style);
    }
}
