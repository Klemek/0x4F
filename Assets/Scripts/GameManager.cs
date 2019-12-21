using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameObjects;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (!_instance)
                _instance = FindObjectOfType<GameManager>();
            return _instance;
        }
    }

    private Settings _settings;

    public Settings Settings
    {
        get
        {
            if (_settings == null)
                _settings = GetComponent<Settings>();
            return _settings;
        }
    }

    [Header("Global")]
    public float speed;
    public float size;
    public bool paused;
    public List<int> randoms;
    private int _life;
    private int _score;
    private int _toAdd;
    public int maxLife;
    public int levelHeight;
    
    private Player _player;
    private GameUIManager _uiManager;

    [Header("Room")] 
    public GameObject[] walls;
    public GameObject groundParent;
    public GameObject groundTilePrefab;
    private float _wallHeight;
    private Vector3 _wallScale;
    private float _wallScaleFactor;
    private float _groundTileSize;
    private float _groundSize;

    [Header("Orbz")] public GameObject orbParent;
    public GameObject orbPrefab;
    public int[] orbPoints;
    public Color[] orbColors;
    public int[] orbChances;
    public float orbGrowTime;
    private const float OrbHeight = 1f;

    private void Start()
    {
        _uiManager = FindObjectOfType<GameUIManager>();
        _player = FindObjectOfType<Player>();
        _player.Teleport(new Vector3(0,levelHeight,0));

        size = IsZOriented(walls[0]) ? walls[0].transform.localPosition.z : walls[0].transform.localPosition.x;
        _wallHeight = walls[0].transform.localPosition.y;
        _wallScale = walls[0].transform.localScale;
        _wallScaleFactor = _wallScale.z / size;

        _groundTileSize = groundTilePrefab.GetComponent<Renderer>().bounds.size.x;

        _groundSize = _groundTileSize;
        SpawnGround();

        randoms = new List<int>();

        _life = maxLife;
        _uiManager.UpdateHealth(_life, maxLife);

        InvokeRepeating(nameof(UpdateGameState), 0f, 3f);
        
        Resume();
        _uiManager.GameUI();
    }

    private void SpawnGround()
    {
        for (; _groundSize < size + _groundTileSize; _groundSize += _groundTileSize)
        {
            for (var j = 0f; j < _groundSize; j += _groundTileSize)
            {
                SpawnGroundTile(j + _groundTileSize / 2, _groundSize - _groundTileSize / 2);
                SpawnGroundTile(-j - _groundTileSize / 2, -_groundSize + _groundTileSize / 2);
                SpawnGroundTile(-_groundSize + _groundTileSize / 2, j + _groundTileSize / 2);
                SpawnGroundTile(_groundSize - _groundTileSize / 2, -j - _groundTileSize / 2);
                if (!(j >= _groundTileSize)) continue;
                SpawnGroundTile(-j + _groundTileSize / 2, _groundSize - _groundTileSize / 2);
                SpawnGroundTile(j - _groundTileSize / 2, -_groundSize + _groundTileSize / 2);
                SpawnGroundTile(-_groundSize + _groundTileSize / 2, -j + _groundTileSize / 2);
                SpawnGroundTile(_groundSize - _groundTileSize / 2, j - _groundTileSize / 2);
            }
        }
    }

    private void SpawnGroundTile(float x, float z)
    {
        var tile = Instantiate(groundTilePrefab, new Vector3(x, groundTilePrefab.transform.position.y, z), Quaternion.identity);
        tile.transform.parent = groundParent.transform;
        tile.transform.name = "Ground Tile (" + x + "; " + z + ")";
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }

    private void FixedUpdate()
    {
        size += speed * Time.deltaTime;

        if (size > _groundSize - _groundTileSize)
        {
            SpawnGround();
        }
        
        _wallScale.z = size * _wallScaleFactor;

        foreach (var wall in walls)
        {
            var wallPosition = wall.transform.localPosition;
            wallPosition = IsZOriented(wall)
                ? new Vector3(0, _wallHeight, Mathf.Sign(wallPosition.z) * size)
                : new Vector3(Mathf.Sign(wallPosition.x) * size, _wallHeight, 0);

            wall.transform.localPosition = wallPosition;
            wall.transform.localScale = _wallScale;
        }
    }

    private void UpdateGameState()
    {
        DestroyRandomGround();
        SpawnRandomOrb();
    }

    private void SpawnRandomOrb()
    {
        var position = new Vector3(Random.Range(-size, size), OrbHeight, Random.Range(-size, size));
        var orbObject = Instantiate(orbPrefab, position, Quaternion.identity);
        orbObject.transform.parent = orbParent.transform;
        var orbType = GetRandomOrb();
        var orb = orbObject.GetComponent<Orb>();
        orb.color = orbColors[orbType];
        orb.points = orbPoints[orbType];
        orb.UpdateColor();
        StartCoroutine(GrowOrb(orb.transform));
    }

    private IEnumerator GrowOrb(Transform orbTransform)
    {
        var targetScale = orbPrefab.transform.localScale.x;
        orbTransform.localScale = Vector3.zero;
        var currentScale = 0f;
        while (currentScale < targetScale && orbTransform)
        {
            currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime / orbGrowTime);
            orbTransform.localScale = new Vector3(currentScale, currentScale, currentScale);
            yield return null;
        }
    }

    private int GetRandomOrb()
    {
        var r = Random.Range(0, orbChances.Sum());
        int i;
        for (i = 0; i < orbChances.Length - 1; i++)
        {
            if (r < orbChances[i])
                return i;
            r -= orbChances[i];
        }
        return i;
    }

    private void DestroyRandomGround()
    {
        if (randoms.Count <= 5)
        {
            randoms.Add(-1);
            return;
        }
        int r;
        do
        {
            r = Random.Range(0, 100);
        } while (randoms.Contains(r));

        randoms.Add(r);
    }

    public void RespawnPlayer()
    {
        paused = true;
        _life--;
        _uiManager.UpdateHealth(_life);

        if (_life > 0)
        {
            var respawnPos = new Vector3(0,levelHeight,0);
            var respawnTile = FindObjectsOfType<GroundTile>().OrderBy(t => t.transform.position.magnitude).FirstOrDefault();
            if (respawnTile)
            {
                // ReSharper disable once PossibleNullReferenceException
                respawnPos += respawnTile.transform.position;
                respawnTile.locked = true;
            }
            _player.Teleport(respawnPos);
            paused = false;
            RebootOrbTrails();
        }
        else
        {
            Pause();
            _uiManager.GameOverUI(_score);
        }
        
    }

    private static void RebootOrbTrails()
    {
        foreach (var trailRenderer in FindObjectsOfType<Orb>().Select(o => o.GetComponentInChildren<TrailRenderer>()))
        {
            trailRenderer.Clear();
        }
    }

    public void AddPoints(int value)
    {
        _score += value;
        _uiManager.AddScore(value);
    }

    private static bool IsZOriented(GameObject wall)
    {
        return Mathf.Abs(wall.transform.position.x) <= Mathf.Epsilon;
    }
}