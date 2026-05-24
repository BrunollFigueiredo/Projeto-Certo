using Fusion;
using UnityEngine;
public class Player : NetworkBehaviour
{
    public static bool LocalSpawnou { get; private set; } = false;
    public static Transform LocalTransform { get; private set; }
    public static Transform LocalPontoMao { get; private set; }
    public static Camera LocalCamera { get; private set; }

    private NetworkCharacterController _cc;

    [SerializeField] private float speed = 15f;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Transform pontoMao;

    [Networked] private NetworkButtons PreviousButtons { get; set; }

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
    }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            LocalTransform = transform;
            LocalPontoMao = pontoMao;
            LocalCamera = cameraHolder != null ? cameraHolder.GetComponentInChildren<Camera>() : Camera.main;

            if (cameraHolder != null)
            {
                Camera sceneCam = Camera.main;
                if (sceneCam != null && !sceneCam.transform.IsChildOf(transform))
                {
                    AudioListener al = sceneCam.GetComponent<AudioListener>();
                    if (al != null) al.enabled = false;
                    sceneCam.gameObject.SetActive(false);
                }

                cameraHolder.gameObject.SetActive(true);
            }

            LocalSpawnou = true;
        }
        else if (cameraHolder != null)
        {
            cameraHolder.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (HasInputAuthority)
        {
            LocalSpawnou = false;
            LocalTransform = null;
            LocalPontoMao = null;
            LocalCamera = null;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (_cc == null) return;

        if (GetInput(out NetworkInputData data))
        {
            transform.rotation = Quaternion.Euler(0, data.lookYaw, 0);

            if (cameraHolder != null)
            {
                cameraHolder.localRotation = Quaternion.Euler(data.lookPitch, 0, 0);
            }

            Vector3 moveDirection = transform.forward * data.direction.z + transform.right * data.direction.x;
            _cc.Move(speed * moveDirection * Runner.DeltaTime);

            if (data.buttons.WasPressed(PreviousButtons, InputButtons.Jump))
            {
                _cc.Jump();
            }

            PreviousButtons = data.buttons;
        }
    }
    private void Update()
    {
        
    }
}