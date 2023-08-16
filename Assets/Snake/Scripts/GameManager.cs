using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public Food[] foodPrefabs;
    public int foodSpawnClearance = 5;
    public int foodSpawnArea = 14;

    public AudioClip eatFoodClip;
    public AudioClip gameOverClip;
    public AudioClip playGameClip;

    UIManager uiManager => UIManager.instance;
    Player player => Player.instance;

    int score;

    new AudioSource audio;

    public static GameManager instance {
        get; private set;
    }

    void Awake() {
        instance = this;

        uiManager.PlayClicked += PlayClicked;

        audio = GetComponent<AudioSource>();
    }

    void PlayClicked() {
        uiManager.SetPlayButton(visible: false);
        uiManager.SetGameOverText(visible: false);
        uiManager.SetScore(score = 0);

        player.ResetPlayer();
        player.enabled = true;

        ClearFood();
        SpawnFood();

        audio.Play(playGameClip);
    }

    void OnEnable() {
        uiManager.SetPlayButton(visible: true);

        player.ResetPlayer();
        player.enabled = false;
    }

    public void GameOver() {
        // Prevent GameOver() being called multiple times
        if (player.enabled) {
            player.enabled = false;

            Time.timeScale = 1f;

            uiManager.SetPlayButton(visible: true);
            uiManager.SetGameOverText(visible: true);

            Camera.main.DOShakePosition(0.5f, strength:1f, vibrato:10);

            audio.Play(gameOverClip);
        }
    }

    public void EatFood(Food food) {
        if (food.enabled) {
            food.enabled = false;
            food.transform.DOScale(0, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(food.gameObject));
            player.length += 10;
            uiManager.SetScore(++score);
            SpawnFood();

            Time.timeScale += 0.05f;

            audio.Play(eatFoodClip);
        }
    }

    void ClearFood() {
        foreach (var food in GetComponentsInChildren<Food>()) {
            Destroy(food.gameObject);
        }
    }

    void SpawnFood() {
        for (var i = 0; i < 100; i++) {
            // Pick random position for food to spawn
            var foodPosition = new Vector3(Random.Range(-foodSpawnArea, +foodSpawnArea), 0, Random.Range(-foodSpawnArea, +foodSpawnArea));

            // Food too close to player
            if (Vector3.Distance(player.transform.position, foodPosition) < foodSpawnClearance) {
                continue;
            }

            // Food overlaps something else (wall, food or player segment)
            if (Physics.CheckSphere(foodPosition, 1)) {
                continue;
            }

            var foodPrefab = foodPrefabs.SelectRandom();
            Instantiate(foodPrefab, foodPosition, Quaternion.identity, transform);
            return;
        }

        Debug.LogWarning("Unable to create food");
    }
}
