using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameRecorder : MonoBehaviour
{
    [Header("Recording")]
    [SerializeField] private int frameRate = 30;
    [SerializeField] private int scaleDivider = 2;
    [SerializeField] private string outputFileName = "gameplay.mp4";

    private string outputDir;
    private string framesDir;
    private bool isRecording;
    private bool stopRequested;
    private bool stopped;
    private int frameIndex;
    private GameManager gameManager;

    private void Start()
    {
        outputDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "chat"));
        framesDir = Path.Combine(outputDir, "frames");

        ClearFramesDir();

        Directory.CreateDirectory(outputDir);
        Directory.CreateDirectory(framesDir);

        gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnGameOver += OnGameOverTriggered;
        }

        Application.targetFrameRate = 60;
        StartCoroutine(RecordFrames());
    }

    private void ClearFramesDir()
    {
        if (Directory.Exists(framesDir))
        {
            try
            {
                foreach (string f in Directory.GetFiles(framesDir))
                {
                    File.Delete(f);
                }
            }
            catch { }
        }
    }

    private void OnGameOverTriggered()
    {
        if (!stopped)
        {
            stopRequested = true;
        }
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnGameOver -= OnGameOverTriggered;
        }
    }

    private IEnumerator RecordFrames()
    {
        isRecording = true;
        frameIndex = 0;
        float frameInterval = 1f / frameRate;
        float nextFrameTime = Time.unscaledTime;

        while (isRecording)
        {
            yield return new WaitForEndOfFrame();

            float now = Time.unscaledTime;
            if (now < nextFrameTime) continue;
            nextFrameTime = now + frameInterval;

            CaptureFrame();

            if (stopRequested && !stopped)
            {
                stopped = true;
                StartCoroutine(CaptureGameOverAndStop());
            }
        }
    }

    private IEnumerator CaptureGameOverAndStop()
    {
        float duration = 2f;
        float endTime = Time.unscaledTime + duration;
        float nextFrameTime = Time.unscaledTime + 1f / frameRate;

        while (Time.unscaledTime < endTime)
        {
            yield return new WaitForEndOfFrame();

            float now = Time.unscaledTime;
            if (now < nextFrameTime) continue;
            nextFrameTime = now + 1f / frameRate;

            CaptureFrame();
        }

        isRecording = false;
        yield return new WaitForSeconds(0.3f);
        EncodeVideo();
    }

    private void CaptureFrame()
    {
        int w = Screen.width / scaleDivider;
        int h = Screen.height / scaleDivider;

        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        if (scaleDivider > 1)
        {
            RenderTexture rt = RenderTexture.GetTemporary(w, h);
            RenderTexture.active = rt;
            Graphics.Blit(screenshot, rt);
            Texture2D scaled = new Texture2D(w, h, TextureFormat.RGB24, false);
            scaled.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            scaled.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            Destroy(screenshot);
            screenshot = scaled;
        }

        byte[] bytes = screenshot.EncodeToPNG();
        Destroy(screenshot);

        string filePath = Path.Combine(framesDir, $"frame_{frameIndex:D06}.png");
        File.WriteAllBytes(filePath, bytes);
        frameIndex++;
    }

    private void EncodeVideo()
    {
        string outputPath = Path.Combine(outputDir, outputFileName);
        string ffmpegArgs = $"-y -framerate {frameRate} -i \"{framesDir}\\frame_%06d.png\" -c:v libx264 -pix_fmt yuv420p -preset fast \"{outputPath}\"";

        Debug.Log("Frames: " + framesDir);
        Debug.Log("Run: ffmpeg " + ffmpegArgs);

        try
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "ffmpeg";
            process.StartInfo.Arguments = ffmpegArgs;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.StartInfo.WorkingDirectory = outputDir;
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Debug.Log("Video saved: " + outputPath);
                try
                {
                    if (Directory.Exists(framesDir))
                        Directory.Delete(framesDir, true);
                }
                catch { }
            }
            else
            {
                Debug.LogWarning("FFmpeg: " + error + "\nFrames kept at: " + framesDir);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("FFmpeg not found: " + e.Message + "\nFrames at: " + framesDir);
        }
    }
}
