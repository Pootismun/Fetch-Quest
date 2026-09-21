using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

//Editor tool to build eand control every animation clip.
public static class AnimationBuilder
{
    private const string SpriteRoot = "Assets/Sprites";
    private const string ClipRoot = "Assets/Animations/Clips";
    private const string ControllerRoot = "Assets/Animations/Controllers";

    // Setting sprite animation timings.
    private const float FrameTime = 0.25f;
    private const float LoopExitTime = 4f;
    private const float OneShotExitTime = 1f;

    // Lets states cycle on their own so every animation can be previewed.
    private const string PreviewParameter = "PreviewCycle";

    private static readonly string[] Directions = { "Up", "Right", "Down", "Left" };
    private static readonly string[] SquirrelColours = { "Grey", "Red", "Black", "Brown" };

    private class StateInfo
    {
        public readonly string Name;
        public readonly AnimationClip Clip;
        public readonly bool Loops;

        public StateInfo(string name, AnimationClip clip, bool loops)
        {
            Name = name;
            Clip = clip;
            Loops = loops;
        }
    }

    [MenuItem("Tools/Fetch Quest/Build Animations")]
    public static void BuildAll()
    {
        try
        {
            EnsureFolders();
            BuildDog();
            BuildPowerPellet();
            BuildSquirrels();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("AnimationBuilder completed.");
        }
        catch (Exception e)
        {
            Debug.LogError("AnimationBuilder failed: " + e.Message);
        }
    }

    // Dog
    private static void BuildDog()
    {
        var states = new List<StateInfo>();

        foreach (string dir in Directions)
        {
            AnimationClip clip = MakeClip("Dog", "Dog_Walk_" + dir, Frames("Dog", "Dog_" + dir, 2), true);
            states.Add(new StateInfo("Walk_" + dir, clip, true));
        }

        AnimationClip dead = MakeClip("Dog", "Dog_Dead", Frames("Dog", "Dog_Dead", 4), false);
        states.Add(new StateInfo("Dead", dead, false));

        BuildController("DogAnimator", states);
    }

    // Power Pellet
    private static void BuildPowerPellet()
    {
        AnimationClip flash = MakeClip("Pickups", "PowerPellet_Flash", Frames("Pickups", "Pickup_PowerPellet", 2), true);
        BuildController("PowerPelletAnimator", new List<StateInfo> { new StateInfo("Flash", flash, true) });
    }

    //Squirrels
    private static void BuildSquirrels()
    {
        var greyWalk = new Dictionary<string, AnimationClip>();
        foreach (string colour in SquirrelColours)
        {
            foreach (string dir in Directions)
            {
                AnimationClip clip = MakeClip("Squirrels", "Squirrel_" + colour + "_Walk_" + dir, Frames("Squirrels", "Squirrel_" + colour + "_" + dir, 2), true);
                if (colour == "Grey")
                {
                    greyWalk[dir] = clip;
                }
            }
        }

        var states = new List<StateInfo>();
        foreach (string dir in Directions)
        {
            states.Add(new StateInfo("Walk_" + dir, greyWalk[dir], true));
        }

        foreach (string dir in Directions)
        {
            AnimationClip scared = MakeClip("Squirrels", "Squirrel_Scared_" + dir, Frames("Squirrels", "Squirrel_Scared_" + dir, 2), true);
            states.Add(new StateInfo("Scared_" + dir, scared, true));
        }

        AnimationClip recovering = MakeClip("Squirrels", "Squirrel_Recovering", Frames("Squirrels", "Squirrel_Recovering", 2), true);
        states.Add(new StateInfo("Recovering", recovering, true));

        AnimationClip dead = MakeClip("Squirrels", "Squirrel_Dead", Frames("Squirrels", "Squirrel_Dead", 2), true);
        states.Add(new StateInfo("Dead", dead, true));

        AnimatorController baseController = BuildController("GhostAnimator_Grey", states);

        foreach (string colour in SquirrelColours)
        {
            if (colour != "Grey")
            {
                BuildOverride(baseController, colour);
            }
        }
    }

    private static void BuildOverride(AnimatorController baseController, string colour)
    {
        string path = ControllerRoot + "/GhostAnimator_" + colour + ".overrideController";
        if (AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path) != null)
        {
            Debug.LogWarning("AnimationBuilder: " + path + " already exists, skipped. Delete it first to rebuild it.");
            return;
        }

        var overrideController = new AnimatorOverrideController(baseController);
        var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(pairs);

        for (int i = 0; i < pairs.Count; i++)
        {
            AnimationClip original = pairs[i].Key;
            if (original == null || !original.name.StartsWith("Squirrel_Grey_Walk_"))
            {
                continue;
            }

            string newName = original.name.Replace("Squirrel_Grey_", "Squirrel_" + colour + "_");
            var replacement = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipRoot + "/Squirrels/" + newName + ".anim");
            if (replacement == null)
            {
                throw new Exception("Clip not found: " + newName);
            }

            pairs[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, replacement);
        }

        overrideController.ApplyOverrides(pairs);
        AssetDatabase.CreateAsset(overrideController, path);
    }

    // Shared Helpers
    private static AnimationClip MakeClip(string subfolder, string clipName, string[] spritePaths, bool loop)
    {
        var keys = new ObjectReferenceKeyframe[spritePaths.Length + 1];

        for (int i = 0; i < spritePaths.Length; i++)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[i]);
            if (sprite == null)
            {
                throw new Exception("Sprite not found: " + spritePaths[i]);
            }
            keys[i] = new ObjectReferenceKeyframe { time = i * FrameTime, value = sprite };
        }
        keys[spritePaths.Length] = new ObjectReferenceKeyframe
        {
            time = spritePaths.Length * FrameTime,
            value = loop ? keys[0].value : keys[spritePaths.Length - 1].value
        };

        var clip = new AnimationClip { name = clipName, frameRate = 4f };
        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        string path = ClipRoot + "/" + subfolder + "/" + clipName + ".anim";
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(clip, existing);
            return existing;
        }

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimatorController BuildController(string controllerName, List<StateInfo> states)
    {
        string path = ControllerRoot + "/" + controllerName + ".controller";
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existing != null)
        {
            Debug.LogWarning("AnimationBuilder: " + path + " already exists, skipped. Delete it first to rebuild it.");
            return existing;
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter(new AnimatorControllerParameter
        {
            name = PreviewParameter,
            type = AnimatorControllerParameterType.Bool,
            defaultBool = true
        });

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        var created = new List<AnimatorState>();

        for (int i = 0; i < states.Count; i++)
        {
            var position = new Vector3(300 + 220 * (i % 4), 100 + 80 * (i / 4), 0f);
            AnimatorState state = machine.AddState(states[i].Name, position);
            state.motion = states[i].Clip;
            created.Add(state);
        }

        machine.defaultState = created[0];

        if (created.Count > 1)
        {
            for (int i = 0; i < created.Count; i++)
            {
                AnimatorState next = created[(i + 1) % created.Count];
                AnimatorStateTransition transition = created[i].AddTransition(next);
                transition.hasExitTime = true;
                transition.exitTime = states[i].Loops ? LoopExitTime : OneShotExitTime;
                transition.hasFixedDuration = true;
                transition.duration = 0f;
                transition.AddCondition(AnimatorConditionMode.If, 0f, PreviewParameter);
            }
        }

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static string[] Frames(string folder, string prefix, int count)
    {
        var paths = new string[count];
        for (int i = 0; i < count; i++)
        {
            paths[i] = SpriteRoot + "/" + folder + "/" + prefix + "_" + (i + 1) + ".png";
        }

        return paths;
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "Animations");
        EnsureFolder("Assets/Animations", "Clips");
        EnsureFolder("Assets/Animations", "Controllers");
        EnsureFolder(ClipRoot, "Dog");
        EnsureFolder(ClipRoot, "Pickups");
        EnsureFolder(ClipRoot, "Squirrels");
    }

    private static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}







