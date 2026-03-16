using System.Collections;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    public GameObject tutorialPanel;
    public GameObject[] tutorialSteps;

    public bool isTutorialActive = false;
    private int currentStep = 0;

    [Header("Drag Obstacles")]
    public GameObject Hand2D;      
    public GameObject InstructionDrag; 
    private bool waitingFor2DClick = false;

    [Header("Move Player")]
    public GameObject InstructionMove;
    private bool waitingForMovementClick = false;

    [Header("Invincible Mode")]
    public GameObject HandFreeze;
    public GameObject InstructionFreeze;
    private bool waitingForInvincibleClick = false;

    [Header("Booster Tutorial")]
    public GameObject BoosterMode;
    private bool waitingForBoosterCollect = false;

    [Header("Reached Door")]
    public GameObject ReachedDoor;

    [Header("Gameplay Cnotrol")]
    public GameObject MobileControlBtn;
    public GameObject SnapshotBtn;
    public GameObject PowerUpBtn;
    public GameObject FreezeBtn;
    public GameObject PauseBtn;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartTutorial()
    {
        if (PlayerPrefs.GetInt("TutorialDone", 0) == 1)
        {
            tutorialPanel.SetActive(false);
            Time.timeScale = 1f;
            return;
        }
       
        ResetTutorialState();

        isTutorialActive = true;

        tutorialPanel.SetActive(true);
        currentStep = 0;

        DisableAllBtn();
        SnapshotBtn.SetActive(true);

        StartCoroutine(InstructionDelay());
    }

    IEnumerator InstructionDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        ShowStep(currentStep);
    }

    void ShowStep(int step)
    {
        if (tutorialSteps.Length == 0) return;

        // Disable all steps
        for (int i = 0; i < tutorialSteps.Length; i++)
        {
            tutorialSteps[i].SetActive(false);
        }

        // Enable current step
        if (step < tutorialSteps.Length)
        {
            tutorialSteps[step].SetActive(true);
        }
    }

    public void OnSnapshotFinished()
    {
        if (!isTutorialActive) return;
        currentStep = 1;
        ShowStep(currentStep);
        InstructionMove.SetActive(true);
        DisableAllBtn();
        MobileControlBtn.SetActive(true);
        waitingForMovementClick = true;
        Time.timeScale = 0f;
    }

    public void OnMovementButtonPressed()
    {
        if (!waitingForMovementClick) return;
        waitingForMovementClick = false;
        InstructionMove.SetActive(false);
        MobileControlBtn.SetActive(false);

        if (GameManagerCycle.Instance.player != null)
            GameManagerCycle.Instance.player.StopAllInput();

        Time.timeScale = 1f;
        NextStep();
    }

    public void StartDragObstacleStep()
    {
        if (!isTutorialActive) return;
        DisableAllBtn();
        PowerUpBtn.SetActive(true);
        Time.timeScale = 0f;
        // show hand pointing to 2D button
        Hand2D.SetActive(true);
        waitingFor2DClick = true;
    }

    public void On2DButtonClicked()
    {
        if (!waitingFor2DClick) return;
        waitingFor2DClick = false;
        Hand2D.SetActive(false);
        InstructionDrag.SetActive(true);
        PowerUpBtn.SetActive(false);
    }

    public void OnObstacleDragged()
    {
        if (!isTutorialActive)
            return;

        PowerUpController power = FindAnyObjectByType<PowerUpController>();
        if (power != null)
            power.ForceEndPowerUp();
        InstructionDrag.SetActive(false);
        Time.timeScale = 1f;
        // Next tutorial step
        StartCoroutine(StartNextStepAfterDelay());
    }

    IEnumerator StartNextStepAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        NextStep();
    }

    public void StartInvincibleStep()
    {

        if (!isTutorialActive) return;
        DisableAllBtn();
        FreezeBtn.SetActive(true);
        Time.timeScale = 0f;
        HandFreeze.SetActive(true);
        waitingForInvincibleClick = true;
    }

    public void OnInvincibleButtonClicked()
    {
        if (!waitingForInvincibleClick)
            return;
        waitingForInvincibleClick = false;
        HandFreeze.SetActive(false);
        MobileControlBtn.SetActive(true);
        InstructionFreeze.SetActive(true);

        if (GameManagerCycle.Instance.player != null)
            GameManagerCycle.Instance.player.StopAllInput();
    }

    public void OnObstacleCrossed()
    {
        if (!isTutorialActive) return;
        InstructionFreeze.SetActive(false);
        DisableAllBtn();
        StartCoroutine(EndInvincibleDelay());
    }

    IEnumerator EndInvincibleDelay()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        PowerUpController power = FindAnyObjectByType<PowerUpController>();
        if (power != null)
            power.EndFreezeFromTutorial();

        if (GameManagerCycle.Instance.player != null)
            GameManagerCycle.Instance.player.StopAllInput();

        Time.timeScale = 1f;

        StartCoroutine(SpawnBoosterAfterDelay());
    }

    IEnumerator SpawnBoosterAfterDelay()
    {
        yield return new WaitForSecondsRealtime(3f);

        LevelGenerator generator = FindAnyObjectByType<LevelGenerator>();

        if (generator != null)
            generator.SpawnBoosterNow();
    }
    public void OnBoosterAppeared()
    {
        if (!isTutorialActive) return;
        MobileControlBtn.SetActive(true);
        if (currentStep == 3)
            NextStep();
        waitingForBoosterCollect = true;
    }

    public void OnBoosterCollected()
    {
        if (!waitingForBoosterCollect) return;

        waitingForBoosterCollect = false;
        BoosterMode.SetActive(false);
        NextStep();
    }

    public void OnDoorReached()
    {
        if (!isTutorialActive) return;
        ReachedDoor.SetActive(false);
        EndTutorial();
    }
    public void NextStep()
    {
        currentStep++;

        if (currentStep < tutorialSteps.Length)
        {
            if(currentStep == 2)
            {
                StartCoroutine(DragObstacleDelay());
            }
            else if (currentStep == 3)
            {
                ShowStep(currentStep);
                StartInvincibleStep();
            }
            else
            {
                ShowStep(currentStep);
            }
        }
        else
        {
            EndTutorial();
        }
    }

    IEnumerator DragObstacleDelay()
    {
        yield return new WaitForSecondsRealtime(1f); 
        ShowStep(currentStep);
        StartDragObstacleStep();
    }

    void EndTutorial()
    {
        StopAllCoroutines();
        isTutorialActive = false;

        // Disable tutorial UI
        tutorialPanel.SetActive(false);

        waitingForBoosterCollect = false;
        waitingFor2DClick = false;
        waitingForMovementClick = false;
        waitingForInvincibleClick = false;

        Time.timeScale = 1f;
        // Save tutorial completion

        MobileControlBtn.SetActive(true);
        SnapshotBtn.SetActive(true);
        PowerUpBtn.SetActive(true);
        FreezeBtn.SetActive(true);

        PlayerPrefs.SetInt("TutorialDone", 1);
        PlayerPrefs.Save();
        GameManagerCycle.Instance.CompleteTutorial();
    }

    public void ForceEndTutorial()
    {
        ResetTutorialState();

        isTutorialActive = false;

        tutorialPanel.SetActive(false);

        waitingForBoosterCollect = false;
        waitingFor2DClick = false;
        waitingForMovementClick = false;
        waitingForInvincibleClick = false;

        Time.timeScale = 1f;

        MobileControlBtn.SetActive(true);
        SnapshotBtn.SetActive(true);
        PowerUpBtn.SetActive(true);
        FreezeBtn.SetActive(true);
    }

    void ResetTutorialState()
    {
        StopAllCoroutines();

        currentStep = 0;

        waitingForBoosterCollect = false;
        waitingFor2DClick = false;
        waitingForMovementClick = false;
        waitingForInvincibleClick = false;

        Hand2D.SetActive(false);
        HandFreeze.SetActive(false);
        InstructionDrag.SetActive(false);
        InstructionMove.SetActive(false);
        InstructionFreeze.SetActive(false);
        BoosterMode.SetActive(false);
        ReachedDoor.SetActive(false);

        for (int i = 0; i < tutorialSteps.Length; i++)
        {
            tutorialSteps[i].SetActive(false);
        }
    }
    void DisableAllBtn()
    {
        PowerUpBtn.SetActive(false);
        FreezeBtn.SetActive(false);
        MobileControlBtn.SetActive(false);
        SnapshotBtn.SetActive(false);
        PauseBtn.SetActive(false);
    }
}