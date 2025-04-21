/*using System;
using UnityEngine;

public class Ball : Node
{
    public BallColor ballColor;
    public float speed;
    [SerializeField]bool canChangeColor = false;

    private Rigidbody _rb;

    private float _forceScale = 0.3f;

    
    private Transform _cameraTransform;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (Camera.main != null) _cameraTransform = Camera.main.transform;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (Camera.main != null) _cameraTransform = Camera.main.transform;
    }

    public void Init(float speed)
    {
        this.speed = speed;s
    }

    public void ActiveForce(float speed)
    {
        
        _rb.linearVelocity = transform.forward * (speed * _forceScale);
    }

    public Quaternion GetLookAtPlayerRotation()
    {
        if (_cameraTransform)
        {
            return Quaternion.LookRotation(transform.position + _cameraTransform.rotation * Vector3.forward,
                Vector3.up);
        }
        return Quaternion.identity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("CheckArea"))
        {
            if (ballColor is BallColor.Red or BallColor.Yellow)
            {
                GameManager.Instance.OnMissedHit?.Invoke();
                SoundManager.Instance.PlaySound(Sound.FakeBallExplosion, transform.position);
                GameManager.Instance.AddScore(-20);
            }

            Deactivate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SpawnSphere"))
        {
            Transform spawnVfx = ObjectPutter.Instance.PutObject(SpawnerType.VFXSpawnBall);
            spawnVfx.position = transform.position;
            spawnVfx.rotation = GetLookAtPlayerRotation();
        }
    }
}*/
using System;
using UnityEngine;

public class Ball : Node
{
    [Header("Mesh Settings")]
    public Mesh[] meshes; // Thứ tự khớp với enum BallColor

    public BallColor ballColor;
    public float speed;

    [SerializeField] bool canChangeColor = false;

    private Rigidbody _rb;
    private MeshFilter _meshFilter;

    private float _forceScale = 0.3f;
    private Transform _cameraTransform;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _meshFilter = GetComponent<MeshFilter>();

        if (Camera.main != null)
            _cameraTransform = Camera.main.transform;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (Camera.main != null)
            _cameraTransform = Camera.main.transform;
    }

    public void Init(float speed)
    {
        this.speed = speed;
    }

    public void ActiveForce(float speed)
    {
        _rb.linearVelocity = transform.forward * (speed * _forceScale);
    }

    public Quaternion GetLookAtPlayerRotation()
    {
        if (_cameraTransform)
        {
            return Quaternion.LookRotation(transform.position + _cameraTransform.rotation * Vector3.forward, Vector3.up);
        }
        return Quaternion.identity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ChangeColor"))
        {
            if (canChangeColor)
            {
                BallColor randomColor = GetRandomBallColor();
                if (randomColor != ballColor)
                {
                    ChangeShape(randomColor);
                }
                else
                {
                    Debug.Log("Không đổi hình vì trùng màu cũ");
                }
            }
        }

        if (other.CompareTag("CheckArea"))
        {
            if (ballColor is BallColor.Red or BallColor.Yellow)
            {
                GameManager.Instance.OnMissedHit?.Invoke();
                SoundManager.Instance.PlaySound(Sound.FakeBallExplosion, transform.position);
                GameManager.Instance.AddScore(-20);
            }

            Deactivate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SpawnSphere"))
        {
            Transform spawnVfx = ObjectPutter.Instance.PutObject(SpawnerType.VFXSpawnBall);
            spawnVfx.position = transform.position;
            spawnVfx.rotation = GetLookAtPlayerRotation();
        }
    }

    public void ChangeShape(BallColor newColor)
    {
        ballColor = newColor;

        int index = (int)newColor;
        if (_meshFilter != null && index >= 0 && index < meshes.Length)
        {
            _meshFilter.mesh = meshes[index];
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy mesh tương ứng với BallColor {newColor}");
        }
    }

    private BallColor GetRandomBallColor()
    {
        Array values = Enum.GetValues(typeof(BallColor));
        return (BallColor)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }
}
