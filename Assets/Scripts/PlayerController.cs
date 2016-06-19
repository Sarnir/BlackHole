using UnityEngine;
using System.Collections;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer sr;
    Collider2D coll2d;
    public Transform blackHole;
    public float startVelocity;
    public float speedFactor;
    public bool isDead;
    public bool isDying;

    public GameObject EngineParticlePrefab;
    public int EngineParticlesMax;
    public int BurstParticlesMax;
    public int ShipParticlesMax;

    public AudioClip BurstClip;
    public AudioClip BoomClip;
    public AudioClip HoleClip;

    Vector2 initialPos;
    Vector2 initialRight;

    float initialDrag;
    
    GameObject[] EngineParticles;
    GameObject[] BurstParticles;
    GameObject[] ShipParticles;

    public float BurstXRange;
    public float BurstYRange;

    public bool IsAlive { get { return !isDead && !isDying; } }

    public event System.Action OnCollisionWithDiamond;

    void Awake()
    {
        Random.seed = 1;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        coll2d = GetComponent<Collider2D>();
        initialDrag = rb.drag;
        initialPos = transform.position;
        initialRight = transform.right;

        EngineParticles = new GameObject[EngineParticlesMax];
        BurstParticles = new GameObject[BurstParticlesMax];
        ShipParticles = new GameObject[ShipParticlesMax];

        for (int i = 0; i < EngineParticlesMax; i++)
        {
            EngineParticles[i] = Instantiate(EngineParticlePrefab);
            EngineParticles[i].transform.parent = transform;
        }

        for (int i = 0; i < BurstParticlesMax; i++)
        {
            BurstParticles[i] = Instantiate(EngineParticlePrefab);
            BurstParticles[i].transform.parent = transform;
            BurstParticles[i].transform.localScale = Vector3.zero;
        }

        var shipParticlesObject = new GameObject("ShipParticles");
        for (int i = 0; i < ShipParticlesMax; i++)
        {
            ShipParticles[i] = Instantiate(EngineParticlePrefab);
            ShipParticles[i].name = "ShipParticle";
            ShipParticles[i].transform.localScale = Vector3.zero;
            ShipParticles[i].transform.parent = shipParticlesObject.transform;
        }

        StartEngineParticles();
    }

    public void Reset()
    {
        transform.right = initialRight;
        rb.velocity = transform.up * startVelocity;
        rb.angularVelocity = 0f;
        isDead = false;
        transform.position = initialPos;
        transform.localScale = Vector3.one;
        rb.drag = 0f;
        coll2d.enabled = true;
    }

    void StartEngineParticles()
    {
        float delta = 0f;
        foreach (var particle in EngineParticles)
        {
            particle.transform.localScale = Vector3.one;
            AnimateParticle(particle, delta);
            delta += Random.Range(0.1f, 0.5f);
            if (delta > 1f)
                delta -= 1f;
        }
    }

    void StartParticleBurst()
    {
        int activated = 0;
        int maxStarted = BurstParticlesMax / 2;
        foreach (var particle in BurstParticles)
        {
            if (!DOTween.IsTweening(particle.transform))
            {
                activated++;
                AnimateBurstParticle(particle);
                particle.transform.localScale = Vector3.one;

                if (activated == maxStarted)
                    break;
            }
        }
    }

    void StopParticles()
    {
        for (int i = 0; i < EngineParticlesMax; i++)
        {
            EngineParticles[i].transform.DOKill(true);
        }
    }

    void AnimateBurstParticle(GameObject particle)
    {
        if (!IsAlive)
            return;

        particle.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f), -0.25f, 0f);
        var particlePos = particle.transform.localPosition;
        Vector3 endPosition = new Vector3(particlePos.x * Random.Range(1f, BurstXRange), particlePos.y - Random.Range(0.5f, 0.5f + BurstYRange), particlePos.z);
        float duration = Random.Range(0.4f, 0.6f);
        float spin = Random.Range(90, 360);

        particle.transform.localScale = Vector3.one;
        particle.transform.DOScale(0f, duration)
            .OnPlay(() => particle.transform.localScale = Vector3.one)
            .SetEase(Ease.InSine);
        particle.transform.DOLocalMove(endPosition, duration)
            .OnPlay(() => particle.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f), -0.25f, 0f));
        particle.transform.DORotate(new Vector3(0f, 0f, spin), duration, RotateMode.FastBeyond360)
            .OnPlay(() => particle.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
    }

    void AnimateParticle(GameObject particle, float delta = 0f)
    {
        if (!IsAlive)
            return;

        particle.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f), -0.25f, 0f);
        var particlePos = particle.transform.localPosition;
        Vector3 endPosition = new Vector3(particlePos.x, particlePos.y - Random.Range(0.5f, 0.75f), particlePos.z);
        float duration = 1.5f;//Random.Range(0.5f, 2f);
        float spin = Random.Range(90, 360);

        if (delta > 0)
        {
            particle.transform.localPosition = Vector3.Lerp(particle.transform.localPosition, endPosition, delta);
            duration -= duration * delta;
            spin -= spin * delta;
        }

        particle.transform.localScale = Vector3.one;
        particle.transform.DOScale(0f, duration)
            .OnPlay(() => particle.transform.localScale = Vector3.one)
            .SetLoops(-1);
        particle.transform.DOLocalMove(endPosition, duration)
            .OnPlay(() => particle.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f), -0.25f, 0f))
            .SetLoops(-1);
        particle.transform.DORotate(new Vector3(0f, 0f, spin), duration, RotateMode.FastBeyond360)
            .OnPlay(() => particle.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)))
            .SetLoops(-1);
    }
    
    public void SpeedUp()
    {
        if (IsAlive)
        {
            if (PlayerPrefs.GetInt("SFX", 1) == 1)
                AudioSource.PlayClipAtPoint(BurstClip, Camera.main.transform.position);
            rb.velocity += (Vector2)transform.up * speedFactor;
            StartParticleBurst();
        }
    }

    void FixedUpdate()
    {
        if (IsAlive)
        {
            var direction = (blackHole.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        HandleCollision(collider.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    void HandleCollision(GameObject other)
    {
        if (!IsAlive)
            return;

        if (other.tag == "BlackHole")
            CollisionWithBlackHole();
        else if (other.tag == "Asteroid")
            CollisionWithAsteroid();
        else if (other.tag == "Diamond")
            OnCollisionWithDiamond();
    }

    public void StartGame()
    {
        rb.drag = initialDrag;
    }

    public void SetDead()
    {
        coll2d.enabled = false;
        isDying = false;
        isDead = true;
    }

    public void SetDying()
    {
        isDying = true;
        rb.drag = 1f;
    }

    void CollisionWithBlackHole()
    {
        if (PlayerPrefs.GetInt("SFX", 1) == 1)
            AudioSource.PlayClipAtPoint(HoleClip, Camera.main.transform.position);
        float dyingDuration = 2f;
        SetDying();
        transform.DORotate(transform.rotation.eulerAngles - new Vector3(0f, 0f, 720f), dyingDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear);
        transform.DOScale(Vector3.zero, dyingDuration).OnComplete(() =>
        {
            SetDead();
        });
    }

    void CollisionWithAsteroid()
    {
        if (PlayerPrefs.GetInt("SFX", 1) == 1)
            AudioSource.PlayClipAtPoint(BoomClip, Camera.main.transform.position);
        transform.localScale = Vector3.zero;

        Camera.main.DOShakePosition(1f);

        float dyingDuration = 2f;
        SetDying();
        for (int i = 0; i < ShipParticles.Length; i++)
        {
            var particle = ShipParticles[i];
            var particlePos = transform.position;
            particle.transform.localPosition = particlePos;
            Vector3 endPosition = new Vector3(particlePos.x + Random.Range(-5f, 5f), particlePos.y + Random.Range(-5f, 5f), particlePos.z);

            particle.transform.localScale = Vector3.one * Random.Range(1f, 2f);
            particle.transform.DOLocalMove(endPosition, dyingDuration)
                .OnPlay(() => particle.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f), -0.25f, 0f));
            particle.transform.DORotate(transform.rotation.eulerAngles - new Vector3(0f, 0f, Random.Range(360f, 720f)), dyingDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear);

            if (i == 0)
            {
                particle.transform.DOScale(Vector3.zero, dyingDuration).OnComplete(() =>
                {
                    SetDead();
                });
            }
            else
                particle.transform.DOScale(Vector3.zero, dyingDuration);
        }
    }

    public void SetSkin(Color skin)
    {
        sr.color = skin;
    }
}
