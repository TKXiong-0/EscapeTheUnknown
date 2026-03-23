using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleKeyInputJump : MonoBehaviour
{
    public Animator characterAnimator;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       if(Input.GetKeyDown(KeyCode.Space))
        {
            characterAnimator.SetTrigger("JumpTrigger");
        }
    }
}
