using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;
    private void Aawake()
    {
        animator = GetComponent<Animator>();
    }
}
