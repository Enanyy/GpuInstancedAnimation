using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamplePlay : MonoBehaviour
{
    GpuInstancedAnimation mAnimation;

    public int fadeFrame = 5;
    // Start is called before the first frame update
    void Start()
    {
        mAnimation = GetComponent<GpuInstancedAnimation>();

    }

    public void PlayIdle()
    {
        if (mAnimation == null)
        {
            return;
        }
        mAnimation.Play("idle", 0, fadeFrame);
    }
    public void PlayRun()
    {
        if (mAnimation == null)
        {
            return;
        }
        mAnimation.Play("run", 0, fadeFrame);
    }
    public void PlayAttack()
    {
        if (mAnimation == null)
        {
            return;
        }
        mAnimation.Play("attack01", 0, fadeFrame);
    }
    public void PlayDie()
    {
        if (mAnimation == null)
        {
            return;
        }
        mAnimation.Play("die", 0, fadeFrame);
    }
    public void PlayBlend()
    {
        if (mAnimation == null)
        {
            return;
        }
        if (mAnimation.isBlending == false)
        {
            if (mAnimation.playingAnimationClip == "attack01")
            {
                mAnimation.PlayBlend("run", BlendDirection.Down, fadeFrame);
            }
            else if (mAnimation.playingAnimationClip == "run")
            {
                mAnimation.PlayBlend("attack01", BlendDirection.Top, fadeFrame);
            }

        }
        else
        {
            mAnimation.PlayBlend(null);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mAnimation == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayIdle();
            //UnityEditor.EditorApplication.isPaused = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayRun();
            //UnityEditor.EditorApplication.isPaused = true;

        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayAttack();
            //UnityEditor.EditorApplication.isPaused = true;

        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlayDie();
            //UnityEditor.EditorApplication.isPaused = true;

        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            PlayBlend();
            //UnityEditor.EditorApplication.isPaused = true;
        }
    }
}
