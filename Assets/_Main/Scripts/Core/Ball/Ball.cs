using System;
using UnityEngine;

public class Ball : Node
{
    public BallColor ballColor;
    public float speed;

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
}