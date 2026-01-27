//using UnityEngine;

//public class PlayerController : MonoBehaviour
//{
//    [Header("Movement Settings")]
//    public float moveStep = 1.2f;
//    public float moveSpeed = 6f;
//    public bool canMove = true;

//    [Header("UI References")]
//    public GameObject mobileControls;

//    private Vector3 startPos;
//    private bool isMoving = false;
//    private Vector3 targetPos;

//    private Animator anim;

//    // -------- MOBILE HOLD MOVEMENT --------
//    private bool holdUp = false;
//    private bool holdDown = false;
//    private bool holdLeft = false;
//    private bool holdRight = false;

//    public float holdMoveInterval = 0.25f;   // Time between auto-moves
//    private float holdTimer = 0f;


//    void Start()
//    {
//        startPos = transform.position;
//        anim = GetComponent<Animator>();

//#if UNITY_ANDROID || UNITY_IOS
//        if (mobileControls) mobileControls.SetActive(true);
//#else
//        if (mobileControls) mobileControls.SetActive(false);
//#endif
//    }

//    void Update()
//    {
//        if (!canMove) return;

//#if UNITY_EDITOR || UNITY_STANDALONE
//        HandlePCMovement();
//#endif

//        HandleMobileHoldMovement();   // NEW
//        HandleSmoothMovement();
//    }

//    // ---------------- PC KEYBOARD MOVEMENT ------------------

//    void HandlePCMovement()
//    {
//        if (isMoving) return;

//        float h = Input.GetAxisRaw("Horizontal");
//        float v = Input.GetAxisRaw("Vertical");

//        if (Mathf.Abs(h) > 0.1f)
//            MovePlayer(new Vector3(h > 0 ? moveStep : -moveStep, 0, 0));

//        if (Mathf.Abs(v) > 0.1f)
//            MovePlayer(new Vector3(0, 0, v > 0 ? moveStep : -moveStep));
//    }

//    // ---------------- SMOOTH MOVEMENT ------------------

//    void HandleSmoothMovement()
//    {
//        if (!isMoving) return;

//        transform.position = Vector3.MoveTowards(
//            transform.position,
//            targetPos,
//            moveSpeed * Time.deltaTime
//        );

//        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
//        {
//            isMoving = false;
//            anim.SetBool("isWalking", false);
//        }
//    }

//    // ---------------- AUTO MOVEMENT FOR HOLD BUTTONS ------------------

//    void HandleMobileHoldMovement()
//    {
//        if (!holdUp && !holdDown && !holdLeft && !holdRight) return;
//        if (isMoving) return;

//        holdTimer -= Time.deltaTime;

//        if (holdTimer <= 0f)
//        {
//            holdTimer = holdMoveInterval;

//            if (holdUp) MovePlayer(new Vector3(0, 0, moveStep));
//            if (holdDown) MovePlayer(new Vector3(0, 0, -moveStep));
//            if (holdLeft) MovePlayer(new Vector3(-moveStep, 0, 0));
//            if (holdRight) MovePlayer(new Vector3(moveStep, 0, 0));
//        }
//    }

//    // ---------------- ACTUAL MOVEMENT FUNCTION ------------------

//    void MovePlayer(Vector3 dir)
//    {
//        if (!canMove || isMoving) return;

//        // Avoid walking into walls
//        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, moveStep, ~0, QueryTriggerInteraction.Ignore))
//        {
//            if (hit.collider.CompareTag("Wall"))
//                return;
//        }

//        targetPos = transform.position + dir;
//        isMoving = true;

//        anim.SetBool("isWalking", true);
//        transform.forward = dir;
//    }

//    // ------------------- MOBILE HOLD BUTTON EVENTS -------------------

//    // On Pointer Down
//    public void HoldUpStart() { holdUp = true; holdTimer = 0f; }
//    public void HoldDownStart() { holdDown = true; holdTimer = 0f; }
//    public void HoldLeftStart() { holdLeft = true; holdTimer = 0f; }
//    public void HoldRightStart() { holdRight = true; holdTimer = 0f; }

//    // On Pointer Up
//    public void HoldUpStop() { holdUp = false; }
//    public void HoldDownStop() { holdDown = false; }
//    public void HoldLeftStop() { holdLeft = false; }
//    public void HoldRightStop() { holdRight = false; }

//    // ------------------- ANIMATION HELPERS -------------------

//    public void PlayHitAnimation()
//    {
//        canMove = false;
//        isMoving = false;
//        anim.SetBool("isWalking", false);
//        anim.SetTrigger("Hit");
//    }

//    public void PlayWinJumpAnimation()
//    {
//        canMove = false;
//        isMoving = false;
//        anim.SetBool("isWalking", false);
//        anim.SetTrigger("WinJump");
//    }

//    // ------------------- RESET POSITION -------------------

//    public void ResetPosition()
//    {
//        transform.position = startPos;
//        isMoving = false;

//        if (anim != null)
//            anim.SetBool("isWalking", false);
//    }

//    // ------------------- COLLISION HANDLING -------------------

//    void OnTriggerEnter(Collider other)
//    {
//        GameManagerCycle gm = FindAnyObjectByType<GameManagerCycle>();
//        if (gm == null) return;

//        if (other.CompareTag("Obstacle"))
//            gm.PlayerHitObstacle();

//        if (other.CompareTag("Door"))
//            gm.PlayerReachedDoor();
//    }
//}

//using UnityEngine;

//public class PlayerController : MonoBehaviour
//{
//    [Header("Movement Settings")]
//    public float moveStep = 1.2f;
//    public float moveSpeed = 6f;
//    public bool canMove = true;

//    [Header("UI References")]
//    public GameObject mobileControls;

//    private Vector3 startPos;
//    private Vector3 targetPos;
//    private Animator anim;

//    private bool isMoving = false;

//    // MOBILE HOLD FLAGS
//    private bool holdUp, holdDown, holdLeft, holdRight;
//    public float holdMoveInterval = 0.25f;
//    private float holdTimer = 0f;


//    void Start()
//    {
//        startPos = transform.position;
//        anim = GetComponent<Animator>();

//#if UNITY_ANDROID || UNITY_IOS
//        if (mobileControls) mobileControls.SetActive(true);
//#else
//        if (mobileControls) mobileControls.SetActive(false);
//#endif
//    }


//    void Update()
//    {
//        if (!canMove) return;

//#if UNITY_EDITOR || UNITY_STANDALONE
//        HandlePCMovement();
//#endif

//        HandleHoldMovement();
//        HandleSmoothMovement();
//    }


//    // ------------ PC Keyboard Movement -------------

//    void HandlePCMovement()
//    {
//        if (isMoving) return;

//        float h = Input.GetAxisRaw("Horizontal");
//        float v = Input.GetAxisRaw("Vertical");

//        if (h > 0.1f) MovePlayer(Vector3.right * moveStep);
//        else if (h < -0.1f) MovePlayer(Vector3.left * moveStep);

//        if (v > 0.1f) MovePlayer(Vector3.forward * moveStep);
//        else if (v < -0.1f) MovePlayer(Vector3.back * moveStep);
//    }


//    // ------------ Smooth Step Movement -------------

//    void HandleSmoothMovement()
//    {
//        if (!isMoving) return;

//        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

//        if ((transform.position - targetPos).sqrMagnitude < 0.001f)
//        {
//            isMoving = false;
//            anim.SetBool("isWalking", false);
//        }
//    }


//    // ------------ Mobile Hold Movement -------------

//    void HandleHoldMovement()
//    {
//        if (isMoving) return;
//        if (!holdUp && !holdDown && !holdLeft && !holdRight) return;

//        holdTimer -= Time.deltaTime;
//        if (holdTimer > 0f) return;

//        holdTimer = holdMoveInterval;

//        if (holdUp) MovePlayer(Vector3.forward * moveStep);
//        else if (holdDown) MovePlayer(Vector3.back * moveStep);
//        else if (holdLeft) MovePlayer(Vector3.left * moveStep);
//        else if (holdRight) MovePlayer(Vector3.right * moveStep);
//    }


//    // ------------ Actual Movement -------------

//    void MovePlayer(Vector3 dir)
//    {
//        if (isMoving || !canMove) return;

//        // Single raycast check
//        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, moveStep, ~0, QueryTriggerInteraction.Ignore))
//            if (hit.collider.CompareTag("Wall")) return;

//        targetPos = transform.position + dir;
//        isMoving = true;

//        transform.forward = dir;
//        anim.SetBool("isWalking", true);
//    }


//    // ------------ Mobile Button Events -------------

//    public void HoldUpStart() { holdUp = true; holdTimer = 0f; }
//    public void HoldUpStop() { holdUp = false; }

//    public void HoldDownStart() { holdDown = true; holdTimer = 0f; }
//    public void HoldDownStop() { holdDown = false; }

//    public void HoldLeftStart() { holdLeft = true; holdTimer = 0f; }
//    public void HoldLeftStop() { holdLeft = false; }

//    public void HoldRightStart() { holdRight = true; holdTimer = 0f; }
//    public void HoldRightStop() { holdRight = false; }


//    // ------------ Hit / Win Animations -------------

//    public void PlayHitAnimation()
//    {
//        isMoving = false;
//        canMove = false;
//        anim.SetBool("isWalking", false);
//        anim.SetTrigger("Hit");
//    }

//    public void PlayWinJumpAnimation()
//    {
//        isMoving = false;
//        canMove = false;
//        anim.SetBool("isWalking", false);
//        anim.SetTrigger("WinJump");
//    }


//    // ------------ Reset -------------

//    public void ResetPosition()
//    {
//        transform.position = startPos;
//        isMoving = false;
//        anim.SetBool("isWalking", false);
//    }


//    // ------------ Trigger Events -------------

//    void OnTriggerEnter(Collider other)
//    {
//        GameManagerCycle gm = FindAnyObjectByType<GameManagerCycle>();
//        if (!gm) return;

//        if (other.CompareTag("Obstacle")) gm.PlayerHitObstacle();
//        else if (other.CompareTag("Door")) gm.PlayerReachedDoor();
//    }
//}
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveStep = 1.2f;
    public float moveSpeed = 6f;
    public bool canMove = true;

    [Header("UI References")]
    public GameObject mobileControls;

    private Vector3 startPos;
    private bool isMoving = false;
    private Vector3 targetPos;

    private Animator anim;

    private bool holdUp = false;
    private bool holdDown = false;
    private bool holdLeft = false;
    private bool holdRight = false;

    public float holdMoveInterval = 0.25f;
    private float holdTimer = 0f;

    private Quaternion startRotation;

    void Start()
    {
        startPos = transform.position;
        startRotation = transform.rotation;
        anim = GetComponent<Animator>();

#if UNITY_ANDROID || UNITY_IOS
        if (mobileControls) mobileControls.SetActive(true);
#else
        if (mobileControls) mobileControls.SetActive(false);
#endif
    }

    void Update()
    {
        if (!canMove) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandlePCMovement();
#endif

        HandleMobileHoldMovement();
        HandleSmoothMovement();
    }

    void HandlePCMovement()
    {
        if (isMoving) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(h) > 0.1f)
            MovePlayer(new Vector3(h > 0 ? moveStep : -moveStep, 0, 0));

        if (Mathf.Abs(v) > 0.1f)
            MovePlayer(new Vector3(0, 0, v > 0 ? moveStep : -moveStep));
    }

    void HandleSmoothMovement()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            isMoving = false;
            anim.SetBool("isWalking", false);
        }
    }

    void HandleMobileHoldMovement()
    {
        if (!holdUp && !holdDown && !holdLeft && !holdRight) return;
        if (isMoving) return;

        holdTimer -= Time.deltaTime;

        if (holdTimer <= 0f)
        {
            holdTimer = holdMoveInterval;

            if (holdUp) MovePlayer(new Vector3(0, 0, moveStep));
            if (holdDown) MovePlayer(new Vector3(0, 0, -moveStep));
            if (holdLeft) MovePlayer(new Vector3(-moveStep, 0, 0));
            if (holdRight) MovePlayer(new Vector3(moveStep, 0, 0));
        }
    }

    void MovePlayer(Vector3 dir)
    {
        if (!canMove || isMoving) return;

        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, moveStep, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Wall"))
                return;

            if (hit.collider.CompareTag("Door")) { }
        }

        targetPos = transform.position + dir;
        isMoving = true;

        anim.SetBool("isWalking", true);
        transform.forward = dir;
    }

    public void HoldUpStart() { holdUp = true; holdTimer = 0f; }
    public void HoldDownStart() { holdDown = true; holdTimer = 0f; }
    public void HoldLeftStart() { holdLeft = true; holdTimer = 0f; }
    public void HoldRightStart() { holdRight = true; holdTimer = 0f; }

    public void HoldUpStop() { holdUp = false; }
    public void HoldDownStop() { holdDown = false; }
    public void HoldLeftStop() { holdLeft = false; }
    public void HoldRightStop() { holdRight = false; }

    public void PlayHitAnimation()
    {
        canMove = false;
        isMoving = false;
        anim.SetBool("isWalking", false);
        anim.SetTrigger("Hit");
    }

    public void PlayWinJumpAnimation()
    {
        canMove = false;
        isMoving = false;
        anim.SetBool("isWalking", false);
        anim.SetTrigger("WinJump");
    }

    public void ResetPosition()
    {
        transform.position = startPos;
        transform.rotation = startRotation;
        isMoving = false;
        canMove = false;

        holdUp = false;
        holdDown = false;
        holdLeft = false;
        holdRight = false;

        holdTimer = 0f;

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.ResetTrigger("Hit");
            anim.ResetTrigger("WinJump");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //GameManagerCycle gm = FindAnyObjectByType<GameManagerCycle>();
        if (GameManagerCycle.Instance == null) return;

        //if (other.CompareTag("Obstacle"))
        //    GameManagerCycle.Instance.PlayerHitObstacle();

        if (other.CompareTag("Door"))
            GameManagerCycle.Instance.PlayerReachedDoor();
        else if (other.CompareTag("Booster"))
        {
            GameManagerCycle.Instance.BoosterCollected();
            Destroy(other.gameObject); // remove booster after use
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameManagerCycle.Instance == null) return;

        if (collision.gameObject.CompareTag("Obstacle"))
            GameManagerCycle.Instance.PlayerHitObstacle();
    }
}
