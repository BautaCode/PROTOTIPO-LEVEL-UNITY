// Environment.cs
using System.Collections.Generic;
using UnityEngine;

public class DestroyWhenPastEnd : MonoBehaviour
{
    public Transform end;
    public float margin = 1.5f;

    void Update()
    {
        if (!end) return;
        if (transform.position.x <= end.position.x - margin)
            Destroy(gameObject);
    }
}

public class Environment : MonoBehaviour
{
    public GameObject front;
    public GameObject center;
    public GameObject back;

    [Header("Velocidad fondo")]
    public float environmentVelocity = 10f;
    public float centerFactor = 0.5f;
    public float backFactor   = 0.1f;

    [Header("Ramp de dificultad")]
    public bool  useSpeedRamp        = true;
    public float speedRampPerSecond  = 0.6f;
    public float maxEnvironmentVelocity = 20f;

    [Header("Bordes loop")]
    public Transform start;   // borde derecho
    public Transform end;     // borde izquierdo

    public List<GameObject> props = new List<GameObject>();

    [Header("Spawns (anchor)")]
    public Transform spawnAnchor; // hijo de "front", X > Player.X, misma Y/Z que la pista

    // ======================= COLECCIONABLES =======================
    [Header("Collectibles")]
    public GameObject collectiblePrefab;
    public float[] collectibleLanesX = new float[] { -1.6f, 0f, 1.6f };
    public Vector2 collectibleYRange = new Vector2(0.9f, 1.6f);
    public float collectibleZ = 0f;
    public float collectibleAheadOffsetX = 2.5f;

    public Vector2 collectibleIntervalRange = new Vector2(1.0f, 1.6f); // tiempo entre intentos
    public float   collectibleMinDistance   = 8f;                       // metros de fondo entre spawns
    public float   collectibleStartDelay    = 0.6f;                     // retraso inicial
    public float   collectibleForceAfter    = 3.0f;                     // si pasa este tiempo sin spawnear, forzar

    float collectibleNextTime;
    float collectibleDistanceAcc = 0f;
    float collectibleLastSpawnTime = -999f;

    // ======================= OBSTACULOS =======================
    [Header("Obstaculos")]
    public GameObject obstaclePrefab;
    public float[] obstacleLanesX = new float[] { -1.6f, 0f, 1.6f };
    public float obstacleY = 0f;
    public float obstacleZ = 0f;
    public float obstacleAheadOffsetX = 6.0f;

    public Vector2 obstacleIntervalRange = new Vector2(1.6f, 2.6f); // tiempo entre intentos
    public float   obstacleMinDistance   = 12f;                      // metros de fondo entre spawns
    public float   obstacleMinGapSeconds = 1.25f;                    // gap temporal mínimo
    public float   obstacleStartDelay    = 1.6f;                     // retraso inicial
    public float   obstacleForceAfter    = 4.0f;                     // si pasa este tiempo sin spawnear, forzar

    [Header("Obstaculos – velocidad relativa")]
    public float obstacleSpeedMultiplier = 1.25f;
    public float obstacleExtraSpeed      = 0f;
    public bool  obstaclesMoveRight      = false; // false = hacia la izquierda (–X), true = hacia la derecha (+X)

    float obstacleNextTime;
    float obstacleDistanceAcc = 0f;
    float obstacleLastSpawnTime = -999f;

    // Lanes (evitar repetir carril)
    int _lastCollectibleLane = -1;
    int _lastObstacleLane    = -1;

    void Start()
    {
        collectibleNextTime   = Time.time + collectibleStartDelay;
        obstacleNextTime      = Time.time + obstacleStartDelay;
        collectibleLastSpawnTime = Time.time - collectibleForceAfter;
        obstacleLastSpawnTime    = Time.time - obstacleForceAfter;
    }

    void Update()
    {
        if (useSpeedRamp)
            environmentVelocity = Mathf.Min(maxEnvironmentVelocity, environmentVelocity + speedRampPerSecond * Time.deltaTime);

        MoveLayers();
        LoopProps();

        float distThisFrame = environmentVelocity * Time.deltaTime;
        collectibleDistanceAcc += distThisFrame;
        obstacleDistanceAcc    += distThisFrame;

        ManageCollectibles();
        ManageObstacles();
    }

    void MoveLayers()
    {
        float dx = -environmentVelocity * Time.deltaTime;
        if (front)  front.transform.position  += new Vector3(dx, 0f, 0f);
        if (center) center.transform.position += new Vector3(dx * centerFactor, 0f, 0f);
        if (back)   back.transform.position   += new Vector3(dx * backFactor,   0f, 0f);
    }

    void LoopProps()
    {
        if (!start || !end || props == null) return;
        for (int i = 0; i < props.Count; i++)
        {
            var p = props[i];
            if (!p) continue;
            if (p.transform.position.x <= end.position.x)
                p.transform.position = new Vector3(start.position.x, p.transform.position.y, p.transform.position.z);
        }
    }

    // -------------------- COLECCIONABLES --------------------
    void ManageCollectibles()
    {
        if (!collectiblePrefab || (collectibleLanesX == null || collectibleLanesX.Length == 0)) return;
        if (Time.time < collectibleNextTime) return;

        bool distOk   = collectibleDistanceAcc >= collectibleMinDistance;
        bool timeout  = (Time.time - collectibleLastSpawnTime) >= collectibleForceAfter;

        if (!distOk && !timeout) return;

        int laneIdx = NextLane(ref _lastCollectibleLane, collectibleLanesX.Length);
        float laneX = collectibleLanesX[laneIdx];

        float anchorX = spawnAnchor ? spawnAnchor.position.x : (start ? start.position.x : 0f);
        float spawnX  = anchorX + laneX + collectibleAheadOffsetX;
        float y       = Random.Range(collectibleYRange.x, collectibleYRange.y);
        Vector3 pos   = new Vector3(spawnX, y, collectibleZ);

        var go = Instantiate(collectiblePrefab, pos, Quaternion.identity);
        if (front) go.transform.SetParent(front.transform, true);

        var killer = go.GetComponent<DestroyWhenPastEnd>() ?? go.AddComponent<DestroyWhenPastEnd>();
        killer.end = end; killer.margin = 1.5f;

        collectibleLastSpawnTime = Time.time;
        collectibleDistanceAcc = 0f;

        float gap = Random.Range(collectibleIntervalRange.x, collectibleIntervalRange.y);
        collectibleNextTime = Time.time + gap;
    }

    // ---------------------- OBSTACULOS ----------------------
    void ManageObstacles()
    {
        if (!obstaclePrefab || (obstacleLanesX == null || obstacleLanesX.Length == 0)) return;
        if (Time.time < obstacleNextTime) return;

        bool distOk   = obstacleDistanceAcc >= obstacleMinDistance;
        bool minGapOk = (Time.time - obstacleLastSpawnTime) >= obstacleMinGapSeconds;
        bool timeout  = (Time.time - obstacleLastSpawnTime) >= obstacleForceAfter;

        if ((!distOk || !minGapOk) && !timeout) return;

        int laneIdx = NextLane(ref _lastObstacleLane, obstacleLanesX.Length);
        float laneX = obstacleLanesX[laneIdx];

        float anchorX = spawnAnchor ? spawnAnchor.position.x : (start ? start.position.x : 0f);
        float spawnX  = anchorX + laneX + obstacleAheadOffsetX;
        Vector3 pos   = new Vector3(spawnX, obstacleY, obstacleZ);

        var go = Instantiate(obstaclePrefab, pos, Quaternion.identity);

        var mover = go.GetComponent<ObstacleMover>() ?? go.AddComponent<ObstacleMover>();
        mover.environment     = this;
        mover.start           = start;     // borde derecho
        mover.end             = end;       // borde izquierdo
        mover.speedMultiplier = obstacleSpeedMultiplier;
        mover.extraSpeed      = obstacleExtraSpeed;
        mover.moveRight       = obstaclesMoveRight; // false = izquierda, true = derecha

        obstacleLastSpawnTime = Time.time;
        obstacleDistanceAcc = 0f;

        float randomGap = Random.Range(obstacleIntervalRange.x, obstacleIntervalRange.y);
        obstacleNextTime = Time.time + randomGap;
    }

    int NextLane(ref int last, int count)
    {
        if (count <= 1) { last = 0; return 0; }
        int i;
        do { i = Random.Range(0, count); } while (i == last);
        last = i;
        return i;
    }
}




