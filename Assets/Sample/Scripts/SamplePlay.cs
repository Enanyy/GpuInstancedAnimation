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

    // Update is called once per frame
    void Update()
    {
        if(mAnimation== null)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            mAnimation.Play("idle", 0, fadeFrame);
            //UnityEditor.EditorApplication.isPaused = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            mAnimation.Play("run", 0, fadeFrame);
            //UnityEditor.EditorApplication.isPaused = true;

        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            mAnimation.Play("attack01", 0, fadeFrame);
            //UnityEditor.EditorApplication.isPaused = true;

        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (mAnimation.isBlending == false)
            {
                if (mAnimation.playingAnimationClip == "attack01")
                {
                    mAnimation.SetBlend("run", BlendDirection.Down, fadeFrame);
                }
                else if(mAnimation.playingAnimationClip == "run")
                {
                    mAnimation.SetBlend("attack01", BlendDirection.Top, fadeFrame);
                }

            }
            else
            {
                mAnimation.SetBlend(null);
            }
            //UnityEditor.EditorApplication.isPaused = true;
        }
    }
}
