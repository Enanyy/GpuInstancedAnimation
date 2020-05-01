using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;

[ConverterVersion("joe", 2)]
public class SampleMeshRenderingAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{

    public GpuInstancedAnimation GpuAnimation;

    public int InstanceCount = 1;
    public int Group = 1;
#if UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP_RUNTIME_WORLD
    static World world;
#endif
    Dictionary<string, AnimationClipType> NameToType = new Dictionary<string, AnimationClipType>
    {
        { "idle", AnimationClipType.idle},
        { "run", AnimationClipType.run},
        { "attack", AnimationClipType.attack},
        { "attack01", AnimationClipType.attack},
        { "attack02", AnimationClipType.attack},
        { "die", AnimationClipType.die},
    };

    private void Awake()
    {
       
    }

    private void Start()
    {
#if UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP_RUNTIME_WORLD

        //DefaultWorldInitialization.Initialize("Default World", false);
        //world = World.DefaultGameObjectInjectionWorld;
        world = new World("AAA");

        World.DefaultGameObjectInjectionWorld = world;

        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default, false);
        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);
        ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);

        var types = world.EntityManager.CreateArchetype(typeof(Translation), typeof(Rotation), typeof(Scale), typeof(LocalToWorld));

        var entity =  world.EntityManager.CreateEntity(types);
        var scale = world.EntityManager.GetComponentData<Scale>(entity);
        scale.Value = 1;
        world.EntityManager.SetComponentData(entity, scale);


        world.CreateSystem<ECSInputSystem>();
        world.CreateSystem<ECSGpuInstancedAnimationSystem>();

        Init(entity, world.EntityManager);


#endif
    }


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Init(entity, dstManager);
    }

    private void Init(Entity entity, EntityManager dstManager)
    {
        World world = dstManager.World;
      
        var systems = world.Systems;
        foreach(var v in systems)
        {
            Debug.Log(v.GetType().Name);
        }
        var components = dstManager.GetComponentTypes(entity);
        foreach (var v in components)
        {
            Debug.Log(v.GetManagedType().Name);
        }

        ECSGpuInstancedMesh mesh = new ECSGpuInstancedMesh
        {
            Mesh = GpuAnimation.mesh,
            Material = GpuAnimation.material,
        };

        dstManager.AddSharedComponentData(entity, mesh);
        dstManager.AddSharedComponentData(entity, new ECSGpuInstancedAnimationGroup { Group = Group });

        int Count = GpuAnimation.animationClips.Count;
        var Clips = dstManager.AddBuffer<ECSGpuInstancedAnimationClip>(entity);
        int defaultIndex = 0;
        for (int i = 0; i < Count; ++i)
        {
            var type = NameToType[GpuAnimation.animationClips[i].Name];
            if (type == AnimationClipType.idle)
            {
                defaultIndex = i;
            }

            Clips.Add(new ECSGpuInstancedAnimationClip(
                  type,
                  GpuAnimation.animationClips[i].StartFrame,
                  GpuAnimation.animationClips[i].EndFrame,
                  GpuAnimation.animationClips[i].FrameCount,
                  InstanceCount == 1 ? GpuAnimation.animationClips[i].wrapMode : GpuInstancedAnimationClip.WrapMode.Loop
             ));
        }
        ECSGpuInstancedAnimation animation = new ECSGpuInstancedAnimation
        {
            DefaultIndex = defaultIndex,
            CurrentIndex = defaultIndex,
            PreviousIndex = ECSGpuInstancedAnimation.INVALID_INDEX,
            BlendIndex = ECSGpuInstancedAnimation.INVALID_INDEX,

        };
        dstManager.AddComponentData(entity, animation);
        var translation = dstManager.GetComponentData<Translation>(entity);
        translation.Value = new float3(0, 5, -10);
        dstManager.SetComponentData(entity, translation);

        InstanceCount--;
        if (InstanceCount > 0)
        {
            var entities = dstManager.Instantiate(entity, InstanceCount, Allocator.Temp);

            foreach (var e in entities)
            {
                var position = dstManager.GetComponentData<Translation>(e);
                Vector2 v = UnityEngine.Random.insideUnitCircle * 50;
                position.Value = Count == 1 ? new float3(0, 5, -10) : new float3(v.x, 0, v.y);
                dstManager.SetComponentData(e, position);

                ECSGpuInstancedAnimation instancedAnimation = dstManager.GetComponentData<ECSGpuInstancedAnimation>(e);
                instancedAnimation.CurrentIndex = UnityEngine.Random.Range(0, Count);
                dstManager.SetComponentData(e, instancedAnimation);

            }
        }
    }
}
public enum AnimationClipType
{
    none = -1,
    idle,
    run,
    attack,
    die,
}

public struct ECSGpuInstancedMesh : ISharedComponentData,IEquatable<ECSGpuInstancedMesh>
{
    public Mesh Mesh;
    public Material Material;

    public bool Equals(ECSGpuInstancedMesh other)
    {
        return Mesh == other.Mesh && Material == other.Material;
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public struct ECSGpuInstancedAnimationGroup : ISharedComponentData,IEquatable<ECSGpuInstancedAnimationGroup>
{
    public int Group;

    public bool Equals(ECSGpuInstancedAnimationGroup other)
    {
        return other.Group == Group;
    }
    public override int GetHashCode()
    {
        return Group << 2;
    }
}

public struct ECSGpuInstancedAnimationClip : IBufferElementData
{
    public readonly AnimationClipType Type;
    public readonly int StartFrame;
    public readonly int EndFrame;
    public readonly int FrameCount;

    public readonly GpuInstancedAnimationClip.WrapMode WrapMode;

    public int OffsetFrame;
    public float CurrentTime;
    public int CurrentFrame;


    public ECSGpuInstancedAnimationClip(AnimationClipType type, int startFrame, int endFrame, int frameCount, GpuInstancedAnimationClip.WrapMode wrapMode)
    {
        Type = type;
        StartFrame = startFrame;
        EndFrame = endFrame;
        FrameCount = frameCount;
        WrapMode = wrapMode;
        OffsetFrame = 0;
        CurrentTime = 0;
        CurrentFrame = 0;
    }

    public int AnimationFrame { get { return StartFrame + CurrentFrame; } }
}

public static class ECSGpuInstancedAnimationClipExtensions
{
    public static void OnUpdate(this ref ECSGpuInstancedAnimationClip clip, float deltaTime, float speed)
    {
        clip.CurrentTime += deltaTime * speed;

        if (clip.WrapMode == GpuInstancedAnimationClip.WrapMode.Once)
        {
            clip.CurrentFrame = ((int)(clip.CurrentTime * GpuInstancedAnimation.TargetFrameRate) + clip.OffsetFrame);

            if (clip.CurrentFrame >= clip.FrameCount)
            {
                clip.CurrentFrame = clip.FrameCount;//固定在最后一帧
            }
        }
        else if (clip.WrapMode == GpuInstancedAnimationClip.WrapMode.ClampForever)
        {
            clip.CurrentFrame = ((int)(clip.CurrentTime * GpuInstancedAnimation.TargetFrameRate) + clip.OffsetFrame);

            if (clip.CurrentFrame >= clip.FrameCount - 1)
            {
                clip.CurrentFrame = clip.FrameCount - 1;//固定在最后一帧
            }
        }
        else
        {
            clip.CurrentFrame = ((int)(clip.CurrentTime * GpuInstancedAnimation.TargetFrameRate) + clip.OffsetFrame) % clip.FrameCount;
        }
    }
}

public struct ECSGpuInstancedAnimation:IComponentData
{
    public const int INVALID_INDEX = -1;
    public int DefaultIndex;

    public int CurrentIndex;
    public int PreviousIndex;
    public int BlendIndex;

    public int CurrentFrame;
    public int PreviousFrame;
    public int BlendFrame;

    public float Speed;

    public int FadeFrame;
    public bool FadeBegin;
    public bool FadeEnd;
    public int FadeBeginAt;
    public float FadeStrength;

    public int BlendFadeFrame;
    public bool BlendBegin;
    public bool BlendEnd;
    public int BlendBeginAt;
    public float BlendFadeStrength;

    public BlendDirection BlendDirection;

}

public static class ECSGpuInstancedAnimationExtensions
{
    public static void OnUpdate(this ref ECSGpuInstancedAnimation animation, ref DynamicBuffer<ECSGpuInstancedAnimationClip> clips, float deltaTime)
    {
        if(animation.CurrentIndex == ECSGpuInstancedAnimation.INVALID_INDEX)
        {
            animation.CurrentIndex = animation.DefaultIndex;
        }

        if (animation.CurrentFrame >= 0)
        {
            PlayDefault(ref animation, ref clips);

            OnUpdate(ref animation.CurrentFrame, ref clips, animation.CurrentIndex, deltaTime, animation.Speed);

            if (animation.PreviousIndex != ECSGpuInstancedAnimation.INVALID_INDEX)
            {
                if (animation.PreviousIndex == animation.CurrentIndex)
                {
                    animation.PreviousIndex = ECSGpuInstancedAnimation.INVALID_INDEX;
                    animation.PreviousFrame = 0;
                }
                else
                {
                    OnUpdate(ref animation.PreviousFrame, ref clips, animation.PreviousIndex, deltaTime, animation.Speed);
                }
            }
            else
            {
                animation.PreviousFrame = 0;
            }

            CrossFade(ref animation, ref clips);

            if (animation.BlendIndex != ECSGpuInstancedAnimation.INVALID_INDEX)
            {
                if (animation.BlendIndex != animation.CurrentIndex
                && animation.BlendIndex != animation.PreviousIndex)
                {
                    OnUpdate(ref animation.BlendFrame, ref clips, animation.BlendIndex, deltaTime, animation.Speed);
                }
            }
            else
            {
                animation.BlendFrame = 0;
            }
           

            Blend(ref animation, ref clips);
        }
    }
    private static void OnUpdate(ref int animationFrame, ref DynamicBuffer<ECSGpuInstancedAnimationClip> clips, int index, float deltaTime, float speed)
    {
        animationFrame = 0;

        if (index >= 0 && index < clips.Length)
        {
            var clip = clips[index];

            clip.OnUpdate(deltaTime, speed);
            clips[index] = clip;
            animationFrame = clip.AnimationFrame;
        }
    }
    private static void PlayDefault(ref ECSGpuInstancedAnimation animation, ref DynamicBuffer<ECSGpuInstancedAnimationClip> clips)
    {
        int index = animation.CurrentIndex;

        if (index >= 0 && index < clips.Length)
        {
            var clip = clips[index];
            if (clip.WrapMode == GpuInstancedAnimationClip.WrapMode.Once)
            {
                if (clip.CurrentFrame >= clip.FrameCount && animation.BlendIndex == ECSGpuInstancedAnimation.INVALID_INDEX)
                {
                    Play(ref animation, AnimationClipType.idle, ref clips, 0, animation.FadeFrame);
                    return;
                }
            }
        }
    }

    private static void CrossFade(ref ECSGpuInstancedAnimation animation, ref DynamicBuffer<ECSGpuInstancedAnimationClip> clips)
    {
        animation.FadeStrength = 1;

        if (animation.CurrentIndex != ECSGpuInstancedAnimation.INVALID_INDEX && animation.FadeFrame > 0)
        {
            var currentClip = clips[animation.CurrentIndex];
            if (currentClip.WrapMode == GpuInstancedAnimationClip.WrapMode.Once)
            {
                if (animation.BlendIndex != ECSGpuInstancedAnimation.INVALID_INDEX && animation.FadeEnd ==false && currentClip.CurrentFrame + animation.FadeFrame >= currentClip.FrameCount - 1)
                {
                    animation.FadeEnd = true;
                    animation.FadeBeginAt = currentClip.CurrentFrame;
                    animation.PreviousIndex = animation.BlendIndex;
                }
            }
            int offsetFrame = currentClip.CurrentFrame - animation.FadeBeginAt;
            if (animation.FadeFrame > 0 && (animation.FadeBegin || animation.FadeEnd) && offsetFrame <= animation.FadeFrame)
            {
                animation.FadeStrength = offsetFrame * 1f / animation.FadeFrame;
                if (animation.FadeBegin)
                {
                    if (offsetFrame >= animation.FadeFrame)
                    {
                        animation.FadeBegin = false;
                        animation.PreviousIndex = ECSGpuInstancedAnimation.INVALID_INDEX;
                    }
                }
                if (animation.FadeEnd)
                {
                    animation.FadeStrength = 1 - animation.FadeStrength;

                    if (offsetFrame >= animation.FadeFrame)
                    {
                        animation.FadeEnd = false;
                        animation.PreviousIndex = ECSGpuInstancedAnimation.INVALID_INDEX;
                    }
                }
            }

            if (currentClip.WrapMode == GpuInstancedAnimationClip.WrapMode.Once)
            {
                if (currentClip.CurrentFrame >= currentClip.FrameCount)
                {
                    if (animation.BlendIndex != ECSGpuInstancedAnimation.INVALID_INDEX)
                    {
                        animation.CurrentIndex = animation.BlendIndex;
                        animation.BlendIndex = ECSGpuInstancedAnimation.INVALID_INDEX;
                    }
                }
            }
        }
        else
        {
            animation.FadeEnd = false;
            animation.FadeBeginAt = 0;
        }
    }

    private static void Blend(ref ECSGpuInstancedAnimation animation, ref DynamicBuffer<ECSGpuInstancedAnimationClip> clips)
    {
        animation.BlendFadeStrength = 1;
        if (animation.BlendIndex!= ECSGpuInstancedAnimation.INVALID_INDEX)
        {
            var blendClip = clips[animation.BlendIndex];

            if (blendClip.WrapMode == GpuInstancedAnimationClip.WrapMode.Once)
            {
                if (animation.BlendEnd == false && blendClip.CurrentFrame + animation.BlendFadeFrame >= blendClip.FrameCount - 1)
                {
                    animation.BlendEnd = true;
                    animation.BlendBeginAt = blendClip.CurrentFrame;
                }
            }

            int offsetFrame = blendClip.CurrentFrame - animation.BlendBeginAt;
            if (animation.BlendFadeFrame > 0 && (animation.BlendBegin || animation.BlendEnd) && offsetFrame <= animation.BlendFadeFrame)
            {
                animation.BlendFadeStrength = offsetFrame * 1f / animation.BlendFadeFrame;
                if (animation.BlendBegin)
                {
                    if (offsetFrame >= animation.BlendFadeFrame)
                    {
                        animation.BlendBegin = false;
                    }
                }
                if (animation.BlendEnd)
                {
                    animation.BlendFadeStrength = 1 - animation.BlendFadeStrength;
                }
            }

            if (blendClip.WrapMode == GpuInstancedAnimationClip.WrapMode.Once)
            {
                if (blendClip.CurrentFrame >= blendClip.FrameCount - 1)
                { 
                    animation.BlendIndex = ECSGpuInstancedAnimation.INVALID_INDEX;
                    animation.BlendEnd = false;
                }
            }
        }

    }



    public static void Play(this ref ECSGpuInstancedAnimation animation, AnimationClipType type, ref DynamicBuffer<ECSGpuInstancedAnimationClip> clips, int offsetFrame = 0, int fadeFrame = 0)
    {

        for(int i = 0,max = clips.Length; i<max;++i)
        {
            var clip = clips[i];
            if(clip.Type == type)
            {
                if (animation.CurrentIndex != i)
                {
                    clip.CurrentTime = 0;
                    clip.CurrentFrame = 0;
                    clip.OffsetFrame = offsetFrame;

                    animation.BlendIndex = ECSGpuInstancedAnimation.INVALID_INDEX;
                    animation.PreviousIndex = animation.CurrentIndex;
                    animation.CurrentIndex = i;
                    animation.FadeFrame = fadeFrame;
                    animation.FadeBeginAt = clip.CurrentFrame;
                    animation.FadeBegin = true;
                    animation.FadeEnd = false;
                    animation.CurrentFrame = clip.AnimationFrame;

                  
                    clips[i] = clip;
                }
                return;
            }
        }

    }

    public static void PlayBlend(this ref ECSGpuInstancedAnimation animation, AnimationClipType type, ref DynamicBuffer<ECSGpuInstancedAnimationClip> clips, BlendDirection direction = BlendDirection.Down, int fadeFrame = 0)
    {
        animation.BlendDirection = direction;
        for (int i = 0, max = clips.Length; i < max; ++i)
        {
            var clip = clips[i];
            if (clip.Type == type)
            {
                if (animation.BlendIndex != i)
                {
                    clip.CurrentTime = 0;
                    clip.CurrentFrame = 0;
                    clip.OffsetFrame = 0;

                    animation.BlendIndex = i;
                    animation.BlendFadeFrame = fadeFrame;
                    animation.BlendBeginAt = clip.CurrentFrame;
                    animation.BlendBegin = true;
                    animation.BlendEnd = false;
                    animation.BlendFrame = clip.AnimationFrame;

                    clips[i] = clip;
                }
                return;
            }
        }

        animation.BlendIndex = ECSGpuInstancedAnimation.INVALID_INDEX;
    }

    
}

[ExecuteAlways]
[AlwaysUpdateSystem]
[UpdateInGroup(typeof(PresentationSystemGroup))]

public class ECSGpuInstancedAnimationSystem : ComponentSystem
{
    static  MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();

    protected override void OnUpdate()
    {
        Entities.WithAll<ECSGpuInstancedMesh,ECSGpuInstancedAnimation,LocalToWorld,ECSGpuInstancedAnimationGroup>().ForEach((Entity entity, ECSGpuInstancedMesh renderer, ref LocalToWorld localToWorld, ref ECSGpuInstancedAnimation animation) =>
        {
            var clips = EntityManager.GetBuffer<ECSGpuInstancedAnimationClip>(entity);

            animation.OnUpdate(ref clips, Time.DeltaTime);

            materialPropertyBlock.SetInt("_CurrentFrame", animation.CurrentFrame);
            materialPropertyBlock.SetInt("_PreviousFrame", animation.PreviousFrame);
            materialPropertyBlock.SetFloat("_FadeStrength", animation.FadeStrength);
            materialPropertyBlock.SetInt("_BlendFrame", animation.BlendFrame);
            materialPropertyBlock.SetFloat("_BlendFadeStrength", animation.BlendFadeStrength);
            materialPropertyBlock.SetFloat("_BlendDirection", (int)animation.BlendDirection);

            Graphics.DrawMesh(renderer.Mesh, localToWorld.Value, renderer.Material, 0, null,0, materialPropertyBlock);
        });

    }

}

[ExecuteAlways]
[AlwaysUpdateSystem]
[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateBefore(typeof(ECSGpuInstancedAnimationSystem))]

public class ECSInputSystem : ComponentSystem
{
    static Dictionary<int, AnimationClipType> KeyCodeToType = new Dictionary<int, AnimationClipType>
    {
        {(int)KeyCode.Alpha1, AnimationClipType.idle } ,
        {(int)KeyCode.Alpha2, AnimationClipType.run  },
        {(int)KeyCode.Alpha3, AnimationClipType.attack },
        {(int)KeyCode.Alpha4, AnimationClipType.die },

    };

    public static int FadeFrame= 5;

    private EntityQuery mEntityQuery;

    protected override void OnCreate()
    {
        base.OnCreate();
        mEntityQuery = GetEntityQuery(typeof(ECSGpuInstancedAnimation), typeof(ECSGpuInstancedMesh));

    }

    protected override void OnUpdate()
    {

        AnimationClipType type = AnimationClipType.none;
        var it = KeyCodeToType.GetEnumerator();
        while(it.MoveNext())
        {
            if(Input.GetKeyDown((KeyCode)it.Current.Key))
            {
                type = it.Current.Value;
                break;
            }
        }

        bool blend = Input.GetKeyDown(KeyCode.B);

       

        int index = 0;
        Entities.WithAll<ECSGpuInstancedMesh, ECSGpuInstancedAnimation, LocalToWorld, ECSGpuInstancedAnimationGroup>()
                .ForEach((Entity entity, ref ECSGpuInstancedAnimation animation) =>
        {
            var clips = EntityManager.GetBuffer<ECSGpuInstancedAnimationClip>(entity);

            animation.Speed = 1;
            animation.FadeFrame = FadeFrame;
            if (index == 0)
            {
                if (type != AnimationClipType.none)
                {
                    animation.Play(type, ref clips, 0, FadeFrame);
                }

                if (animation.BlendIndex == ECSGpuInstancedAnimation.INVALID_INDEX && blend)
                {
                    if (animation.CurrentIndex != ECSGpuInstancedAnimation.INVALID_INDEX && clips[animation.CurrentIndex].Type == AnimationClipType.attack)
                    {
                        animation.PlayBlend(AnimationClipType.run, ref clips, BlendDirection.Down, FadeFrame);
                    }
                    else if (animation.CurrentIndex != ECSGpuInstancedAnimation.INVALID_INDEX && clips[animation.CurrentIndex].Type == AnimationClipType.run)
                    {
                        animation.PlayBlend(AnimationClipType.attack, ref clips, BlendDirection.Top, FadeFrame);
                    }
                }
            }

            ++index;
        });
    }
}