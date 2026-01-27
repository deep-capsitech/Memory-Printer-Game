//using UnityEngine;
//using System.Collections;

//public class PowerUpManager : MonoBehaviour
//{
//    public static PowerUpManager Instance;

//    [Header("Cameras")]
//    public Camera mainCamera;
//    public Camera topDownCamera;

//    public float powerUpDuration = 10f;
//    public bool powerUpActive = false;

//    private DraggableObstacle selectedObstacle;

//    void Awake()
//    {
//        Instance = this;
//    }

//    public void ActivatePowerUp()
//    {
//        if (powerUpActive) return;

//        powerUpActive = true;

//        // Camera switch
//        mainCamera.gameObject.SetActive(false);
//        topDownCamera.gameObject.SetActive(true);

//        // Stop player
//        GameManagerCycle.Instance.player.canMove = false;

//        StartCoroutine(PowerUpTimer());
//    }

//    IEnumerator PowerUpTimer()
//    {
//        yield return new WaitForSeconds(powerUpDuration);
//        DeactivatePowerUp();
//    }

//    void DeactivatePowerUp()
//    {
//        powerUpActive = false;

//        if (selectedObstacle != null)
//            selectedObstacle.Lock();

//        // Camera restore
//        topDownCamera.gameObject.SetActive(false);
//        mainCamera.gameObject.SetActive(true);

//        GameManagerCycle.Instance.player.canMove = true;
//    }

//    public void SelectObstacle(DraggableObstacle obstacle)
//    {
//        if (!powerUpActive) return;

//        if (selectedObstacle != null)
//            selectedObstacle.Lock();

//        selectedObstacle = obstacle;
//        selectedObstacle.Unlock();
//    }
//}
