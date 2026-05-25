using Fusion;
using UnityEngine;

public class AnimacaoPersonagem : NetworkBehaviour
{
    private Animator animator;
    private NetworkCharacterController cc;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        cc = GetComponent<NetworkCharacterController>();
    }

    void Update()
    {
        if (animator == null) return;

        float speed = cc != null ? new Vector3(cc.Velocity.x, 0f, cc.Velocity.z).magnitude : 0f;
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }
}
