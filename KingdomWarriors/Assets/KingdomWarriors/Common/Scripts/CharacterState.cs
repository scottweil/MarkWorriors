using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ModeState = CharacterState.Mode.State;
using MoveState = CharacterState.Move.State;
using ActionState = CharacterState.Action.State;

using ModeReceiver = CharacterState.Mode.Receiver;
using MoveReceiver = CharacterState.Move.Receiver;
using ActionReceiver = CharacterState.Action.Receiver;

public interface CharacterState{
    public interface Sender<T>{
        public void Notify(T state);
    }

    public interface Mode : Sender<ModeState>{
        public enum State{
            Daily, Battle
        }
        public interface Receiver{
            public void OnDailyStartEvent();
            public void OnBattleStartEvent();

            public void OnDailyEndEvent();
            public void OnBattleEndEvent();
        }
    }

    public interface Move : Sender<MoveState>{
        public enum State{
            Stay, Walk, Run, Dash, Climb, FallDown
        }
        public interface Receiver{
            public void OnStayStartEvent();
            public void OnWalkStartEvent();
            public void OnRunStartEvent();
            public void OnDashStartEvent();
            public void OnClimbStartEvent();
            public void OnFallDownStartEvent();

            public void OnStayEndEvent();
            public void OnWalkEndEvent();
            public void OnRunEndEvent();
            public void OnDashEndEvent();
            public void OnClimbEndEvent();
            public void OnFallDownEndEvent();
        }
    }
    
    public interface Action : Sender<ActionState>{
        public enum State{
            Idle, Jump, Landing, GetDamage, Death, NoramlAttack, HeavyAttack, FinishAttack
        }
        public interface Receiver{
            public void OnIdleStartEvent();
            public void OnJumpStartEvent();
            public void OnLandingStartEvent();
            public void OnAttackStartEvent();

            public void OnIdleEndEvent();
            public void OnJumpEndEvent();
            public void OnLandingEndEvent();
            public void OnAttackEndEvent();
        }
    }
}

// T  : ?????? enum ??????
// T2 : ????????? ????????? ??????????????? ??????
public abstract class CharacterState<T, T2>{
    protected delegate void StateChangeEvent();

    private List<T2> eventReceiverList = new List<T2>();

   // private T prevState;
    public T CurrentState {get; private set;}

    public void AddReceiver(T2 receiver){
        this.eventReceiverList.Add(receiver);
    }

    public void RemoveReceiver(T2 receiver){
        this.eventReceiverList.Remove(receiver);
    }

    public void ClearReceiverList(){
        this.eventReceiverList.Clear();
    }

    public bool IsCheckState(T state){
        return state.Equals(CurrentState);
    }

    public void SetState(T state){
        if(state == null || eventReceiverList == null || CurrentState.Equals(state)){
            
            return;
        }
        StateChangeEvent stateChangeEvent = null;
        stateChangeEvent += GetStateEndEvents(CurrentState);
        stateChangeEvent += GetStateStartEvents(state);
        CurrentState = state;

        if(stateChangeEvent == null){
            return;
        }
        stateChangeEvent();
    }

    private StateChangeEvent GetStateEndEvents(T prevState){
        StateChangeEvent stateChangeEvent = null;
        foreach(T2 receiver in eventReceiverList){
            if(receiver == null){
                continue;
            }
            stateChangeEvent += GetStateEndEvent(prevState, receiver);
        }
        return stateChangeEvent;
    }

    private StateChangeEvent GetStateStartEvents(T currnetState){
        StateChangeEvent stateChangeEvent = null;
        foreach(T2 receiver in eventReceiverList){
            if(receiver == null){
                continue;
            }
            stateChangeEvent += GetStateStartEvent(currnetState, receiver);
        }
        return stateChangeEvent;
    }

    protected abstract StateChangeEvent GetStateStartEvent(T currentState, T2 receiver);
    protected abstract StateChangeEvent GetStateEndEvent(T prevState, T2 receiver);
}

/**************************************************
 * ????????? ?????? (??????, ??????)
 */
public class CharacterModeState : CharacterState<ModeState, ModeReceiver>
{
    protected override StateChangeEvent GetStateEndEvent(ModeState prevState, ModeReceiver receiver)
    {
        if(receiver == null){
            return null;
        }
        switch (prevState){
            case ModeState.Daily : return receiver.OnDailyEndEvent;
            case ModeState.Battle : return receiver.OnBattleEndEvent;
            default: return null;
        }
    }

    protected override StateChangeEvent GetStateStartEvent(ModeState currentState, ModeReceiver receiver)
    {
        if(receiver == null){
            return null;
        }
        switch (currentState){
            case ModeState.Daily : return receiver.OnDailyStartEvent;
            case ModeState.Battle : return receiver.OnBattleStartEvent;
            default: return null;
        }
    }
}

/**************************************************
 * ?????? ??????
 */
public class CharacterMoveState : CharacterState<MoveState, MoveReceiver>
{
    protected override StateChangeEvent GetStateEndEvent(MoveState prevState, MoveReceiver receiver)
    {
        if(receiver == null){
            return null;
        }
        switch (prevState){
            case MoveState.Stay : return receiver.OnStayEndEvent;
            case MoveState.Walk : return receiver.OnWalkEndEvent;
            case MoveState.Run : return receiver.OnRunEndEvent;
            case MoveState.Dash : return receiver.OnDashEndEvent;
            case MoveState.Climb : return receiver.OnClimbEndEvent;
            case MoveState.FallDown : return receiver.OnFallDownEndEvent;
            default: return null;
        }
    }

    protected override StateChangeEvent GetStateStartEvent(MoveState currentState, MoveReceiver receiver)
    {
        if(receiver == null){
            return null;
        }
        switch (currentState){
            case MoveState.Stay : return receiver.OnStayStartEvent;
            case MoveState.Walk : return receiver.OnWalkStartEvent;
            case MoveState.Run : return receiver.OnRunStartEvent;
            case MoveState.Dash : return receiver.OnDashStartEvent;
            case MoveState.Climb : return receiver.OnClimbStartEvent;
            case MoveState.FallDown : return receiver.OnFallDownStartEvent;
            default: return null;
        }
    }
}

/**************************************************
 * ?????? ??????
 */
public class CharacterActionState : CharacterState<ActionState, ActionReceiver>
{
    protected override StateChangeEvent GetStateEndEvent(ActionState prevState, ActionReceiver receiver)
    {
        if(receiver == null){
            return null;
        }

        switch (prevState){
            case ActionState.Idle : return receiver.OnIdleEndEvent;
            case ActionState.Jump : return receiver.OnJumpEndEvent;
            case ActionState.Landing : return receiver.OnLandingEndEvent;
            case ActionState.NoramlAttack : 
            case ActionState.HeavyAttack : 
            case ActionState.FinishAttack : return receiver.OnAttackEndEvent;
            default: return null;
        }
    }

    protected override StateChangeEvent GetStateStartEvent(ActionState currentState, ActionReceiver receiver)
    {
        if(receiver == null){
            return null;
        }
        switch (currentState){
            case ActionState.Idle : return receiver.OnIdleStartEvent;
            case ActionState.Jump : return receiver.OnJumpStartEvent;
            case ActionState.Landing : return receiver.OnLandingStartEvent;
            case ActionState.NoramlAttack : 
            case ActionState.HeavyAttack : 
            case ActionState.FinishAttack : return receiver.OnAttackStartEvent;
            default: return null;
        }
    }
}