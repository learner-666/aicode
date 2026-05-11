using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float eyeHeight = 1.55f;

    [Header("Crosshair")]
    [SerializeField] private float crosshairSize = 16f;
    [SerializeField] private float crosshairThickness = 2f;
    [SerializeField] private Color crosshairColor = new Color(1, 1, 1, 0.8f);

    private CharacterController characterController;
    private Animator animator;
    private Camera playerCamera;
    private float verticalRotation = 0f;
    private Vector3 moveDirection = Vector3.zero;

    private void Start()
    {
        if (gameObject.tag != "Player")
        {
            try { gameObject.tag = "Player"; }
            catch { Debug.LogWarning("Please add 'Player' tag in Project Settings > Tags and Layers"); }
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider c in colliders)
        {
            if (c is CharacterController) continue;
            c.enabled = false;
        }

        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.center = new Vector3(0, 1f, 0);
            characterController.radius = 0.3f;
            characterController.height = 1.8f;
            characterController.stepOffset = 0.3f;
        }

        animator = GetComponent<Animator>();

        GameObject camObj = new GameObject("FirstPersonCamera");
        camObj.transform.SetParent(transform);
        camObj.transform.localPosition = new Vector3(0, eyeHeight, 0);
        camObj.transform.localRotation = Quaternion.identity;
        playerCamera = camObj.AddComponent<Camera>();
        playerCamera.nearClipPlane = 0.01f;
        playerCamera.tag = "MainCamera";

        Camera[] existingCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in existingCameras)
        {
            if (cam != playerCamera)
            {
                cam.enabled = false;
            }
        }

        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer r in renderers)
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        HandleMouseLook();
        HandleMovement();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        transform.Rotate(0, mouseX, 0);
    }

    private void HandleMovement()
    {
        bool isGrounded = characterController.isGrounded;

        if (isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector3 move = transform.right * h + transform.forward * v;
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        moveDirection.x = move.x * currentSpeed;
        moveDirection.z = move.z * currentSpeed;

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            moveDirection.y = jumpForce;
        }

        moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);

        if (animator != null)
        {
            float horizontalSpeed = new Vector3(moveDirection.x, 0, moveDirection.z).magnitude;
            float normalizedSpeed = isRunning ? horizontalSpeed / runSpeed : horizontalSpeed / walkSpeed;
            animator.SetFloat("MoveSpeed", normalizedSpeed);
        }
    }

    private void OnGUI()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        float s = crosshairSize;
        float t = crosshairThickness;

        GUI.color = crosshairColor;
        GUI.DrawTexture(new Rect(cx - s, cy - t * 0.5f, s * 2, t), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - t * 0.5f, cy - s, t, s * 2), Texture2D.whiteTexture);
    }
}
