
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveStep = 1.2f;
    public float moveSpeed = 6f;
    public bool canMove = true;
    public bool freezeMode = false;

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
    private bool isPassingThroughDoor = false;

    public float holdMoveInterval = 0.25f;
    private float holdTimer = 0f;

    private Quaternion startRotation;

    private Vector3 lastSafePosition;

    [Header("Raycast Settings")]
    public float wallCheckDistance = 1.3f;
    public float doorCheckDistance = 1.2f;

    public LayerMask wallLayer;
    public LayerMask doorLayer;

    [Header("Mobile Buttons")]
    public Button upButton;
    public Button downButton;
    public Button leftButton;
    public Button rightButton;

    private bool tutorialOnlyUp = false;
    private bool isInsideObstacle = false;
    private Transform currentObstacle = null;

    Vector3 previousPos;
    void Start()
    {
        startPos = transform.position;
        lastSafePosition=startPos;
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
        float dt = freezeMode ? Time.unscaledDeltaTime : Time.deltaTime;
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * dt
        );

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            transform.position = targetPos;
            lastSafePosition = transform.position;

            isMoving = false;
            anim.SetBool("isWalking", false);
            CheckObstacleCrossed();
        }
    }
    public void StopMovementImmediately()
    {
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void SetControlInteraction(bool enable)
    {
        if (upButton) upButton.interactable = enable;
        if (downButton) downButton.interactable = enable;
        if (leftButton) leftButton.interactable = enable;
        if (rightButton) rightButton.interactable = enable;
    }

    public void SetTutorialOnlyUpControl()
    {
        tutorialOnlyUp = true;
        if (upButton) upButton.interactable = true;

        if (downButton) downButton.interactable = false;
        if (leftButton) leftButton.interactable = false;
        if (rightButton) rightButton.interactable = false;
    }

    public void SetAllControlsActive()
    {
        tutorialOnlyUp = false;
        if (upButton) upButton.interactable = true;
        if (downButton) downButton.interactable = true;
        if (leftButton) leftButton.interactable = true;
        if (rightButton) rightButton.interactable = true;
    }
    void HandleMobileHoldMovement()
    {
        if (!holdUp && !holdDown && !holdLeft && !holdRight) return;
        if (isMoving) return;

        holdTimer -= Time.unscaledDeltaTime;

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
        previousPos = transform.position;
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnMovementButtonPressed();
        }


        if (!canMove || isMoving) return;

        Vector3 nextPos = transform.position + dir;

        if (dir.z < 0 && nextPos.z < startPos.z)
            return;

        RaycastHit hit;

        // STEP 1: Check door FIRST with bigger distance
        if (Physics.Raycast(transform.position,
                            dir.normalized,
                            out hit,
                            doorCheckDistance,
                            doorLayer,
                            QueryTriggerInteraction.Collide))
        {
            if (hit.collider.CompareTag("Door"))
            {
                isPassingThroughDoor = true;

                targetPos = nextPos;
                isMoving = true;

                anim.SetBool("isWalking", true);
                transform.forward = dir;
                return;
            }
        }

        if (Physics.Raycast(transform.position,
                            dir.normalized,
                            wallCheckDistance,
                            wallLayer,
                            QueryTriggerInteraction.Ignore))
        {
            return; // blocked
        }


        targetPos = nextPos;
        isMoving = true;

        anim.SetBool("isWalking", true);
        transform.forward = dir;
        SoundManager.Instance.PlayWalk();
    }

    public void EnableUnscaledAnimation(bool enable)
    {
        anim.updateMode = enable
            ? AnimatorUpdateMode.UnscaledTime
            : AnimatorUpdateMode.Normal;
    }
 
    public void HoldUpStart() { holdUp = true; holdTimer = 0f; }
    public void HoldDownStart() { if (tutorialOnlyUp) return; holdDown = true; holdTimer = 0f; }
    public void HoldLeftStart() { if (tutorialOnlyUp) return; holdLeft = true; holdTimer = 0f; }
    public void HoldRightStart() { if (tutorialOnlyUp) return; holdRight = true; holdTimer = 0f; }

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

    public void ForceStopAnimation()
    {
        isMoving = false;
        holdUp = holdDown = holdLeft = holdRight = false;

        if (anim != null)
            anim.SetBool("isWalking", false);
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

    public void SnapToTargetTile()
    {
        if (isMoving)
        {
            transform.position = targetPos;
            lastSafePosition = targetPos;
            isMoving = false;

            if (anim != null)
                anim.SetBool("isWalking", false);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (GameManagerCycle.Instance == null) return;

        if (other.CompareTag("Door"))
        {
            DoorGlow.Instance?.ApplyWorldMaterial();

            UIFlowController.Instance.gameplayPanel.SetActive(false);
            isPassingThroughDoor = false;

            if (DiscoLightManager.Instance != null)
                DiscoLightManager.Instance.SetDiscoMode(true);
            
            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
            {
                TutorialManager.Instance.OnDoorReached();
            }

            GameManagerCycle.Instance.PlayerReachedDoor();
        }
        else if (other.CompareTag("Booster"))
        {
            GameManagerCycle.Instance.BoosterCollected();
            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
            {
                TutorialManager.Instance.OnBoosterCollected();
            }
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Obstacle"))
        {

            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
            {
                TutorialManager.Instance.ForceEndTutorial();
            }
            if (freezeMode)
                return;
            SoundManager.Instance.PlayDeath();
            GameManagerCycle.Instance.PlayerHitObstacle();
        }
    }

    void CheckObstacleCrossed()
    {
        if (!freezeMode) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, 0.3f);

        bool currentlyInside = false;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Obstacle"))
            {
                currentlyInside = true;

                if (!isInsideObstacle)
                {
                    isInsideObstacle = true;
                    currentObstacle = hit.transform;
                }
                break;
            }
        }

        if (isInsideObstacle && !currentlyInside)
        {
            isInsideObstacle = false;

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnObstacleCrossed();
            }
        }
    }
    public void ReviveToLastSafeTile()
    {
        transform.position = lastSafePosition;
        isMoving = false;
        canMove = false;

        anim.SetBool("isWalking", false);
    }

    public void StopAllInput()
    {
        holdUp = false;
        holdDown = false;
        holdLeft = false;
        holdRight = false;

        holdTimer = 0f;
    }

}
