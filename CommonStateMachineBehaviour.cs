using System;
using System.Collections.Generic;
using UnityEngine;

public class CommonStateMachineBehaviour : StateMachineBehaviour
{
    public delegate void StateEvent(AnimatorControl animator, AnimatorStateInfo stateInfo, int layerIndex);

    public AnimatorControl Control;
    public class EventStruct
    {
        public StateEvent ExitEvents;
        public StateEvent EnterEvents;
        public StateEvent RunningEvents;
    }

    public Dictionary<int, EventStruct> Events = new Dictionary<int, EventStruct>();

    public void AddEvent(AnimatorEventType type, StateEvent action, string stateName)
    {
        int hash = Animator.StringToHash(stateName);
        EventStruct eventValue;
        if (!Events.TryGetValue(hash,out eventValue))
        {
            Events.Add(hash,eventValue = new EventStruct());
        }
        switch (type)
        {
            case AnimatorEventType.Enter:
                eventValue.EnterEvents += action;
                break;
            case AnimatorEventType.Exit:
                eventValue.ExitEvents += action;
                break;
            case AnimatorEventType.Running:
                eventValue.RunningEvents += action;
                break;
            default:
                throw new ArgumentOutOfRangeException("type", type, null);
        }
    }

    public void RemoveEvent(AnimatorEventType type, StateEvent action, string stateName)
    {
        int hash = Animator.StringToHash(stateName);
        EventStruct eventValue;
        if (!Events.TryGetValue(hash, out eventValue))
        {
            return;
        }
        switch (type)
        {
            case AnimatorEventType.Enter:
                eventValue.EnterEvents -= action;
                break;
            case AnimatorEventType.Exit:
                eventValue.ExitEvents -= action;
                break;
            case AnimatorEventType.Running:
                eventValue.RunningEvents -= action;
                break;
            default:
                throw new ArgumentOutOfRangeException("type", type, null);
        }
    }
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EventStruct @event;
        if (Events.TryGetValue(stateInfo.fullPathHash,out @event))
        {
            if (@event.EnterEvents != null)
                @event.EnterEvents(Control, stateInfo, layerIndex);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EventStruct @event;
        if (Events.TryGetValue(stateInfo.fullPathHash, out @event))
        {
            if (@event.RunningEvents != null)
                @event.RunningEvents(Control, stateInfo, layerIndex);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        EventStruct @event;
        if (Events.TryGetValue(stateInfo.fullPathHash, out @event))
        {
            if (@event.ExitEvents != null)
                @event.ExitEvents(Control, stateInfo, layerIndex);
        }
    }

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateMachineEnter is called when entering a statemachine via its Entry Node
    //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
    //
    //}

    // OnStateMachineExit is called when exiting a statemachine via its Exit Node
    //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
    //
    //}
}
