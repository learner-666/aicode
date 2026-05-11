using UnityEngine;
using System.Collections.Generic;

public class PhaseAbility : MonoBehaviour
{
    [Header("Phase Settings")]
    [SerializeField] private float phaseDuration = 3f;
    [SerializeField] private float scanRadius = 3f;
    [SerializeField] private float cooldown = 5f;

    [Header("Visual")]
    [SerializeField] private float fadeAlpha = 0.25f;
    [SerializeField] private Color phaseTint = new Color(0.4f, 0.6f, 1f);

    private CharacterController playerCC;
    private SkinnedMeshRenderer[] playerRenderers;
    private Material[][] originalMaterials;

    private bool isPhasing;
    private float phaseEndTime;
    private float lastPhaseTime = -999f;
    private float nextScanTime;

    private HashSet<Collider> ignoredColliders = new HashSet<Collider>();

    private void Start()
    {
        playerCC = GetComponent<CharacterController>();
        playerRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        CachePlayerMaterials();
    }

    private void CachePlayerMaterials()
    {
        if (playerRenderers == null || playerRenderers.Length == 0) return;

        originalMaterials = new Material[playerRenderers.Length][];
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            originalMaterials[i] = playerRenderers[i].materials;
        }
    }

    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        if (Input.GetMouseButtonDown(1) && !isPhasing)
        {
            if (Time.time - lastPhaseTime >= cooldown)
            {
                ActivatePhase();
            }
        }

        if (isPhasing)
        {
            if (Time.time > phaseEndTime)
            {
                DeactivatePhase();
            }
            else if (Time.time >= nextScanTime)
            {
                ScanAndIgnoreColliders();
                nextScanTime = Time.time + 0.15f;
            }
        }
    }

    private void ActivatePhase()
    {
        isPhasing = true;
        phaseEndTime = Time.time + phaseDuration;
        lastPhaseTime = Time.time;
        nextScanTime = 0f;

        MakePlayerTransparent(true);
        ScanAndIgnoreColliders();
    }

    private void DeactivatePhase()
    {
        isPhasing = false;

        RestoreAllCollisions();
        MakePlayerTransparent(false);
    }

    private void MakePlayerTransparent(bool transparent)
    {
        if (playerRenderers == null) return;

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] == null) continue;

            if (transparent)
            {
                playerRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                Material[] phaseMats = new Material[originalMaterials[i].Length];
                for (int j = 0; j < originalMaterials[i].Length; j++)
                {
                    phaseMats[j] = new Material(originalMaterials[i][j]);
                    Color c = phaseMats[j].color;
                    c.a = fadeAlpha;
                    phaseMats[j].color = c * phaseTint;

                    if (phaseMats[j].HasProperty("_Mode"))
                    {
                        phaseMats[j].SetFloat("_Mode", 3);
                    }
                    phaseMats[j].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    phaseMats[j].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    phaseMats[j].SetInt("_ZWrite", 0);
                    phaseMats[j].DisableKeyword("_ALPHATEST_ON");
                    phaseMats[j].EnableKeyword("_ALPHABLEND_ON");
                    phaseMats[j].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    phaseMats[j].renderQueue = 3000;
                }
                playerRenderers[i].materials = phaseMats;
            }
            else
            {
                playerRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                playerRenderers[i].materials = originalMaterials[i];
            }
        }
    }

    private void ScanAndIgnoreColliders()
    {
        Vector3 playerPos = transform.position;
        float halfHeight = playerCC != null ? playerCC.height * 0.5f : 0.9f;
        Vector3 sphereCenter = playerPos + Vector3.up * halfHeight;

        Collider[] nearby = Physics.OverlapSphere(sphereCenter, scanRadius);
        HashSet<Collider> groundSet = FindGroundColliders(playerPos);

        foreach (Collider col in nearby)
        {
            if (col == null || col.isTrigger) continue;
            if (col == playerCC) continue;
            if (ignoredColliders.Contains(col)) continue;

            bool isGround = groundSet.Contains(col);

            if (!isGround && !IsBelowFeet(col, playerPos))
            {
                ignoredColliders.Add(col);
                Physics.IgnoreCollision(playerCC, col, true);
            }
        }

        Collider[] groundColliders = Physics.OverlapSphere(playerPos + Vector3.down * 0.3f, 0.8f);
        foreach (Collider col in groundColliders)
        {
            if (col == null || col.isTrigger) continue;
            if (col == playerCC) continue;
            if (IsBelowFeet(col, playerPos) && ignoredColliders.Contains(col))
            {
                ignoredColliders.Remove(col);
                Physics.IgnoreCollision(playerCC, col, false);
            }
        }
    }

    private HashSet<Collider> FindGroundColliders(Vector3 playerPos)
    {
        HashSet<Collider> ground = new HashSet<Collider>();

        Vector3 rayOrigin = playerPos + Vector3.up * 0.5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 3f))
        {
            ground.Add(hit.collider);

            RaycastHit[] allHits = Physics.RaycastAll(rayOrigin, Vector3.down, 3f);
            foreach (RaycastHit h in allHits)
            {
                if (h.collider != null && !h.collider.isTrigger)
                {
                    ground.Add(h.collider);
                }
            }
        }

        return ground;
    }

    private bool IsBelowFeet(Collider col, Vector3 playerPos)
    {
        Bounds bounds = col.bounds;
        float playerFeetY = playerPos.y;
        return bounds.max.y <= playerFeetY + 0.05f;
    }

    private void RestoreAllCollisions()
    {
        foreach (Collider col in ignoredColliders)
        {
            if (col != null && playerCC != null)
            {
                Physics.IgnoreCollision(playerCC, col, false);
            }
        }
        ignoredColliders.Clear();
    }
}
