using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityStandardAssets.ImageEffects;
using System.Collections.Generic;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    enum GameState
    {
        Initial,
        Shop,
        Gameplay,
        GameOver
    }

    // Menus
    public RectTransform MainMenu;
    public Shop ShopMenu;

    public Camera BackgroundCamera;
    public BlackHole BlackHole;
    public PlayerController Player;
    public GameObject Curtain;
    public GameObject StarPrefab;
    public GameObject AsteroidPrefab;
    public Diamond Diamond;
    public Text PointsText;
    public Text RestartText;
    public Text HiScoreText;
    public Text DiamondsText;

    public float CameraMinRadius;
    public float CameraMaxRadius;
    public float VortexRadius;
    public float CameraChangeScreenXPercentage;

    public float MaxStars;

    public Color[] BackgroundColors;
    int currentColor;

    GameState gameState;

    float lastPlayerAngle;
    float startAngle;
    float screenRatio;
    Vortex vortexEffect;
    float vortexSizeFactor;
    float timePassed;
    int points;
    int hiScore;
    int diamonds;
    
    Vector3 cameraPos;
    List<GameObject> spawnedAsteroids;
    
    void Start ()
    {
        cameraPos = Camera.main.transform.position;
        spawnedAsteroids = new List<GameObject>();
        gameState = GameState.Initial;
        hiScore = PlayerPrefs.GetInt("HI_SCORE", 0);
        diamonds = PlayerPrefs.GetInt("DIAMONDS", 0);

        // camera initials
        screenRatio = Screen.width / (float)Screen.height;
        vortexEffect = Camera.main.GetComponent<Vortex>();
        vortexEffect.radius = new Vector2(VortexRadius, VortexRadius * screenRatio);
        vortexSizeFactor = VortexRadius * Camera.main.orthographicSize;
        
        var curtainScale = GetLocalScale();
        Curtain.transform.localScale = new Vector2(curtainScale, curtainScale);

        ShopMenu.Init();

        SetInitialState();
        Curtain.transform.DOScale(0f, 1f).SetEase(Ease.InCubic);

        Player.OnCollisionWithDiamond += AddDiamond;

        BlackHole.range = CameraMaxRadius * screenRatio;

        PointsText.transform.DOShakePosition(20f, 5f, 1, 90).SetEase(Ease.Linear).SetLoops(-1);
        UpdateDiamondsText();

        // background already baked to prefab, so commenting this out
        /*
        Random.seed = (int)System.DateTime.Now.Ticks;
        var starsContainer = new GameObject("StarsBackground");
        for (int i = 0; i < MaxStars; i++)
        {
            var scale = Mathf.Pow(Random.value, 8f) * (1f - 0.2f) + 0.2f;
            var newStar = Instantiate(StarPrefab);
            newStar.transform.position = new Vector2(Random.Range(-8f, 8f), Random.Range(-10f, 10f));
            newStar.transform.localScale = new Vector2(scale, scale);
            newStar.transform.parent = starsContainer.transform;
        }
        */

        Random.seed = (int)System.DateTime.Now.Ticks;
        var AsteroidBeltContainer = new GameObject("AsteroidBelt");
        for (int i = 0; i < 200; i++)
        {
            var angle = Random.Range(0f, 360f);
            var scale = Random.Range(0.5f, 1f);

            var q = Quaternion.AngleAxis(angle, Vector3.forward);
            var distance = BlackHole.range * Random.Range(0.9f, 1.1f);
            var pos = q * Vector3.right * distance;

            var newAsteroid = Instantiate(AsteroidPrefab);
            newAsteroid.transform.position = pos;
            newAsteroid.transform.localScale = new Vector2(scale, scale);
            newAsteroid.transform.parent = AsteroidBeltContainer.transform;
            newAsteroid.transform.DORotate(new Vector3(0f, 0f, 360f), Random.Range(2f, 5f), RotateMode.WorldAxisAdd).SetLoops(-1);
        }

        AsteroidBeltContainer.transform.DORotate(new Vector3(0f, 0f, 360f), 30f, RotateMode.WorldAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }

    public void SetInitialState(bool resetPlayer = true)
    {
        Debug.Log("Initial state entered.");
        MainMenu.gameObject.SetActive(true);
        ShopMenu.gameObject.SetActive(false);

        gameState = GameState.Initial;
        Camera.main.transform.position = cameraPos;
        currentColor = 0;
        SetNewBackgroundColor();

        if (resetPlayer)
            Player.Reset();

        BlackHole.Reset();
        Diamond.Disappear();
        PointsText.text = "";
        RestartText.gameObject.SetActive(false);
        HiScoreText.DOFade(1f, 0f);
        HiScoreText.text = "Highscore: " + hiScore;

        foreach (var asteroid in spawnedAsteroids)
        {
            Destroy(asteroid);
        }
    }

    void SetDiamonds(int diamondNum)
    {
        diamonds = diamondNum;
        PlayerPrefs.SetInt("DIAMONDS", diamonds);

        UpdateDiamondsText();
    }

    public void AddDiamond()
    {
        SetDiamonds(diamonds + 1);
    }

    public void ReduceDiamonds(int quantity)
    {
        SetDiamonds(diamonds - quantity);
    }

    public int GetDiamonds()
    {
        return diamonds;
    }

    void UpdateDiamondsText()
    {
        DiamondsText.text = diamonds.ToString();
        DiamondsText.rectTransform.DOScale(1.2f, 0.3f).OnComplete(() =>
        DiamondsText.rectTransform.DOScale(1f, 0.3f));
    }

    void Update ()
    {
        switch (gameState)
        {
            case GameState.Initial:
                break;

            case GameState.Gameplay:
                timePassed += Time.deltaTime;
                if (timePassed > 1f)
                {
                    // TODO: object pooling zamiast instantiate :/
                    SpawnAsteroid(Random.Range(3f, BlackHole.range));
                    timePassed -= 1f;
                }

                if (Player.isDead)
                {
                    GameOver();
                }
                else if(!Player.isDying)
                {
                    var playerAngle = Player.transform.rotation.z;
                    if (lastPlayerAngle > startAngle && playerAngle <= startAngle)
                    {
                        points++;
                        PointsText.text = points.ToString();
                        SpawnDiamond();

                        if (points > 0 && points % 5 == 0)
                            SetNewBackgroundColor();
                    }

                    lastPlayerAngle = playerAngle;
                }
                break;

            case GameState.GameOver:
                break;

            case GameState.Shop:
                if (Input.GetKeyDown(KeyCode.Escape))
                    OnScreenTap();
                break;

            default:
                break;
        }
        
        UpdateCameraSize();
    }

    void SetNewBackgroundColor()
    {
        if (currentColor >= BackgroundColors.Length)
            currentColor = 0;

        BackgroundCamera.DOColor(BackgroundColors[currentColor], 0.5f);

        currentColor++;
    }

    public void GoToShop()
    {
        Debug.Log("Shop button clicked!");
        gameState = GameState.Shop;

        ShopMenu.Enter();
        MainMenu.gameObject.SetActive(false);
    }

    public void OnScreenTap()
    {
        Debug.Log("Screen tapped!");
        switch (gameState)
        {
            case GameState.Initial:
                StartGame();
                break;

            case GameState.Shop:
                SetInitialState(false);
                break;
                
            case GameState.Gameplay:
                if (!Player.isDying)
                    Player.SpeedUp();
                break;

            case GameState.GameOver:
                ResetScene();
                break;

            default:
                break;
        }
    }

    private void SpawnDiamond()
    {
        Diamond.AppearAtPosition(RandomOnCircle(Vector2.zero, Random.Range(3f, BlackHole.range - 2f)));
    }

    void StartGame()
    {
        Debug.Log("Game state entered.");
        MainMenu.gameObject.SetActive(false);

        timePassed = 0f;
        gameState = GameState.Gameplay;
        BlackHole.StartGrowing();
        Player.StartGame();
        startAngle = Player.transform.rotation.z;
        lastPlayerAngle = startAngle;
        points = 0;
        PointsText.text = points.ToString();
        HiScoreText.DOFade(0f, 0.4f);

        for (int i = 1; i < 6; i++)
        {
            SpawnAsteroid(i * 3);
        }

        SpawnDiamond();
    }

    void GameOver()
    {
        gameState = GameState.GameOver;

        if (points > hiScore)
        {
            hiScore = points;
            PlayerPrefs.SetInt("HI_SCORE", hiScore);
        }
        
        RestartText.gameObject.SetActive(true);
    }

    void ResetScene()
    {
        var curtainTween = Curtain.transform.DOScale(GetLocalScale(), 1f).SetEase(Ease.OutCubic);
        curtainTween.OnComplete(() =>
        {
            SetInitialState();
            UpdateCameraSize();
            var newScale = GetLocalScale() * 1.1f;
            Curtain.transform.localScale = new Vector2(newScale, newScale);
            Curtain.transform.DOScale(0f, 1f).SetEase(Ease.InCubic);
        });
    }

    float GetLocalScale()
    {
        Vector2 ss = Camera.main.ViewportToWorldPoint(Vector2.one);

        return ss.magnitude * 2f;
    }

    void SpawnAsteroid(float radius)
    {
        //return;

        var asteroid = Instantiate(AsteroidPrefab);
        var rb = asteroid.AddComponent<Rigidbody2D>();
        var sr = asteroid.GetComponent<SpriteRenderer>();
        var coll = asteroid.GetComponent<Collider2D>();
        rb.gravityScale = 0f;
        rb.drag = 0.5f;
        var initialVelocity = 2.4f;

        coll.enabled = false;

        Vector3 dir = -transform.position.normalized;
        Vector3 cross = Vector3.Cross(Vector3.right, dir).normalized;

        rb.velocity = cross * initialVelocity;
        
        asteroid.transform.position = RandomOnCircle(Vector3.zero, radius);
        spawnedAsteroids.Add(asteroid);

        sr.DOFade(0f, 1f).SetEase(Ease.Flash).OnComplete(() =>
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
            rb.velocity = cross * initialVelocity;
            coll.enabled = true;
        });
    }

    void UpdateCameraSize()
    {
        if (Player.isDead)
            return;

        float border = Camera.main.orthographicSize * 2 * screenRatio * CameraChangeScreenXPercentage;
        float playerDistance = Player.transform.position.magnitude + border;

        Camera.main.orthographicSize = Mathf.Clamp(Mathf.Max(playerDistance / screenRatio, playerDistance), CameraMinRadius, CameraMaxRadius);

        // adjust vortex effect
        VortexRadius = vortexSizeFactor / Camera.main.orthographicSize;
        vortexEffect.radius = new Vector2(VortexRadius, VortexRadius * screenRatio);
    }

    static Vector3 RandomOnCircle(Vector3 center, float radius)
    {
        // create random angle between 0 to 360 degrees
        var ang = Random.value * 360;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;
    }

    public void SetPlayerSkin(Color color)
    {
        Player.SetSkin(color);
    }
}
