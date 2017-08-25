using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif
public enum AnimatorEventType
{
    Enter,Exit,Running
}

public struct AnimatorParameters
{
    public string Value;

    public AnimatorParameters(string str)
    {
        Value = str;
    }
    public static AnimatorParameters Moving = new AnimatorParameters("Moving");
    public static AnimatorParameters Attack = new AnimatorParameters("Attack");
    public static AnimatorParameters Cast = new AnimatorParameters("Cast");
    public static AnimatorParameters Jumping = new AnimatorParameters("Jumping");
    public static AnimatorParameters IsJump = new AnimatorParameters("IsJump");
    public static AnimatorParameters GetHit = new AnimatorParameters("GetHit");
}

[RequireComponent(typeof(Animator))]
public partial class AnimatorControl : MonoBehaviour
{
    private Animator mAnimator;
    public Animator Animator
    {
        get { return !mAnimator.IsNull() ? mAnimator : (mAnimator = GetComponent<Animator>()); }
        private set { mAnimator = value; }
    }
    private const string MatchSign = "*";
    public string[] FullStateName;
    public int[] FullStateNameHash;
    private CommonStateMachineBehaviour mBehaviour;
    public CommonStateMachineBehaviour Behaviour
    {
        get
        {
            if (mBehaviour == null)
            {
                mBehaviour = Animator.GetBehaviour<CommonStateMachineBehaviour>();
                mBehaviour.Control = this;
            }
            return mBehaviour;
        }
        private set { mBehaviour = value; }
    }

    public void Reset()
    {
        if(Animator == null)
            Animator = GetComponent<Animator>();
        mBehaviour = Animator.GetBehaviour<CommonStateMachineBehaviour>();
        mBehaviour.Control = this;
    }

    void Awake()
    {
        Reset();
    }

    /// <summary>
    /// 使用*表示将Action应用到所有的状态当中
    /// 使用如Attack_*表示所有带有Attack_前缀的添加Action
    /// 当不带*时表示全字符匹配
    /// </summary>
    public void SetEvent(AnimatorEventType type,CommonStateMachineBehaviour.StateEvent action,params string[] stateName)
    {
        int length = stateName.Length;
        for (int i = 0; i < length; i++)
        {
            if (stateName[i].Contains(MatchSign))
            {
                if (stateName[i].Length == 1)
                {
                    for (int j = 0; j < FullStateName.Length; j++)
                    {
                        Behaviour.AddEvent(type, action, FullStateName[j]);
                    }
                }
                else
                {
                    stateName[i] = stateName[i].Replace(MatchSign, "");
                    for (int j = 0; j < FullStateName.Length; j++)
                    {
                        if (FullStateName[j].Contains(stateName[i]))
                            Behaviour.AddEvent(type, action, FullStateName[j]);
                    }
                }
            }
            else
            {
                Behaviour.AddEvent(type, action, stateName[i]);
            }
        }
    }
    /// <summary>
    /// 使用*表示将Action应用到所有的状态当中
    /// 使用如Attack_*表示所有带有Attack_前缀的添加Action
    /// 当不带*时表示全字符匹配
    /// </summary>
    public void RemoveEvent(AnimatorEventType type, CommonStateMachineBehaviour.StateEvent action, params string[] stateName)
    {
        int length = stateName.Length;
        for (int i = 0; i < length; i++)
        {
            if (stateName[i].Contains(MatchSign))
            {
                if (stateName[i].Length == 1)
                {
                    for (int j = 0; j < FullStateName.Length; j++)
                    {
                        Behaviour.RemoveEvent(type, action, FullStateName[j]);
                    }
                }
                else
                {
                    stateName[i] = stateName[i].Replace(MatchSign, "");
                    for (int j = 0; j < FullStateName.Length; j++)
                    {
                        if (FullStateName[j].Contains(stateName[i]))
                            Behaviour.RemoveEvent(type, action, FullStateName[j]);
                    }
                }
            }
            else
            {
                Behaviour.RemoveEvent(type, action, stateName[i]);
            }
        }
    }

    public string HashToName(int hash)
    {
        for (int i = FullStateNameHash.Length - 1; i >= 0; i--)
        {
            if (FullStateNameHash[i] == hash)
                return FullStateName[i];
        }
        throw new Exception("不存在这个Hash");
    }

    #region 重写方法
    public void Play(string anim)
    {
        Animator.Play(anim);
    }
    public void Play(string anim,int layer)
    {
        Animator.Play(anim,layer);
    }
    public void Play(string anim, int layer, float time)
    {
        Animator.Play(anim,layer,time);
    }
    public void PlayImmediately(string anim)
    {
        Animator.Play(anim, 0, 0);
    }

    public event Action<AnimatorControl, AnimatorParameters, bool> ParametersBoolChange;
    public void SetBool(AnimatorParameters varName, bool value)
    {
        Animator.SetBool(varName.Value, value);
        if (ParametersBoolChange != null) ParametersBoolChange(this, varName, value);
    }

    public event Action<AnimatorControl, AnimatorParameters, int> ParametersIntegerChange;
    public void SetInteger(AnimatorParameters varName, int value)
    {
        Animator.SetInteger(varName.Value, value);
        if (ParametersIntegerChange != null) ParametersIntegerChange(this, varName, value);
    }

    public event Action<AnimatorControl, AnimatorParameters, float> ParametersFloatChange;
    public void SetFloat(AnimatorParameters varName, float value)
    {
        Animator.SetFloat(varName.Value, value);
        if (ParametersFloatChange != null) ParametersFloatChange(this, varName, value);
    }

    public event Action<AnimatorControl, AnimatorParameters> ParametersTriggerChange;
    public void SetTrigger(AnimatorParameters varName)
    {
        Animator.SetTrigger(varName.Value);
        if (ParametersTriggerChange != null) ParametersTriggerChange(this, varName);
    }
    #endregion
}

#region 编辑器方法
#if UNITY_EDITOR
[CustomEditor(typeof(AnimatorControl))]
public class AnimatorControlEditor : Editor
{
    private static AnimatorControl mControl;
    public void OnEnable()
    {
        Run((AnimatorControl)target);
    }

    public static void Run(AnimatorControl control)
    {
        mControl = control;
        if (mControl.IsNull())
            return;
        if (mControl.Animator == null)
        {
            mControl.Reset();
        }
        AnimatorController animatorController = mControl.Animator.runtimeAnimatorController as AnimatorController;
        if (animatorController != null)
        {
            AnimatorStateMachine stateMachine = animatorController.layers[0].stateMachine;
            List<string> nameList = new List<string>();
            List<int> hashList = new List<int>();
            GetValue(stateMachine, nameList, hashList, stateMachine.name);
            CheckHaveBehaviour(stateMachine);
            mControl.FullStateName = nameList.ToArray();
            mControl.FullStateNameHash = hashList.ToArray();
        }
    }

    private static void GetValue(AnimatorStateMachine stateMachine,List<string> name, List<int> hash,string layerName)
    {
        for (int i = 0; i < stateMachine.stateMachines.Length; i++)
        {
            var state = stateMachine.stateMachines[i].stateMachine;
            GetValue(state, name,hash, layerName + "." + state.name);
        }
        for (int j = 0; j < stateMachine.states.Length; j++)
        {
            var state = stateMachine.states[j].state;
            name.Add(layerName  + "." + state.name);
            hash.Add(state.nameHash);
        }
    }

    private static void CheckHaveBehaviour(AnimatorStateMachine sm)
    {
        bool needAddBehaviours = true;
        foreach (StateMachineBehaviour node in sm.behaviours.Where(node => node.GetType() == typeof(CommonStateMachineBehaviour)))
            needAddBehaviours = false;
        if (needAddBehaviours)
            sm.AddStateMachineBehaviour<CommonStateMachineBehaviour>();
    }
}

public partial class AnimatorControl : ISerializationCallbackReceiver
{
    public void OnBeforeSerialize()
    {
        if (Animator == null)
            return;
        var runtime = Animator.runtimeAnimatorController;
        if (!AnimatorControlIntelligent.mAnimator.ContainsKey(runtime))
        {
            AnimatorControlIntelligent.mAnimator.Add(runtime, new List<AnimatorControl>());
        }
        else
            AnimatorControlIntelligent.mAnimator[runtime].Add(this);
    }

    public void OnAfterDeserialize()
    {
    }
}

[InitializeOnLoad]
public class AnimatorControlIntelligent : AssetPostprocessor
{
    public static Dictionary<RuntimeAnimatorController, List<AnimatorControl>> mAnimator = new Dictionary<RuntimeAnimatorController, List<AnimatorControl>>();

    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        for (int i = importedAssets.Length - 1; i >= 0; i--)
        {
            if (importedAssets[i].EndsWith(".controller"))
            {
                var runtime = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(importedAssets[i]);
                if (mAnimator.ContainsKey(runtime))
                {
                    var list = mAnimator[runtime];
                    int count = list.Count;
                    for (int j = 0; j < count; j++)
                    {
                        AnimatorControlEditor.Run(list[j]);
                    }
                }
            }
        }
    }
}
#endif
#endregion