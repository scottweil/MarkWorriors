using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ModeState = CharacterState.Mode.State;
using MoveState = CharacterState.Move.State;
using ActionState = CharacterState.Action.State;

public class Logger
{
    public enum Tag
    {
        State
    }

    public static void Log(Tag tag, object msg)
    {
        Logger.Log(tag.ToString(), msg);
    }
    public static void Log(string tag, object msg)
    {
        Debug.Log("[" + tag + "], " + msg.ToString());
    }
}

public class SswPlayerMediator : MonoBehaviour,
                                    CharacterMediator,
                                    Interaction.Attack,
                                    // CharacterState.Mode.Receiver,
                                    // CharacterState.Move.Receiver, 
                                    // CharacterState.Action.Receiver, 
                                    OnAttackEvent
{
    private CharacterStatus PlayerStatus { get; set; }

    private SswPlayerCameraMove PlayerCameraMove { get; set; }
    private SswPlayerInputMove PlayerInputMove { get; set; }
    private SswPlayerShortcutKeys PlayerShortcutKeys { get; set; }
    private SswPlayerEffector PlayerEffector { get; set; }
    private SswPlayerAnim PlayerAnim { get; set; }

    private WeaponWearableHand rightWeaponHand;

    public static SswPlayerMediator GetFindPlayerMediator(SswPlayerMediator mediator, Animator animator)
    {
        if (mediator == null)
        {
            return animator.GetComponent<SswPlayerMediator>();
        }
        return mediator;
    }

    void Start()
    {
        PlayerStatus = GetComponent<CharacterStatus>();
        PlayerCameraMove = GetComponent<SswPlayerCameraMove>();
        PlayerInputMove = GetComponent<SswPlayerInputMove>();
        PlayerEffector = GetComponent<SswPlayerEffector>();
        PlayerAnim = GetComponent<SswPlayerAnim>();

        rightWeaponHand = GetComponentInChildren<WeaponWearableHand>();
    }

    void Update()
    {

    }

    /**************************************************
     * ?????? ?????? ??????
     */
    private void SetState(ModeState state) { PlayerStatus.SetState(state); }
    private void SetState(MoveState state) { PlayerStatus.SetState(state); }
    private void SetState(ActionState state) { PlayerStatus.SetState(state); }

    public bool IsCheckState(ModeState state) { return PlayerStatus.IsCheckState(state); }
    public bool IsCheckState(MoveState state) { return PlayerStatus.IsCheckState(state); }
    public bool IsCheckState(ActionState state) { return PlayerStatus.IsCheckState(state); }

    public ModeState GetModeState() { return PlayerStatus.ModeState.CurrentState; }
    public MoveState GetMoveState() { return PlayerStatus.MoveState.CurrentState; }
    public ActionState GetActionState() { return PlayerStatus.ActionState.CurrentState; }

    public bool IsAttacking()
    {
        return IsCheckState(ActionState.NoramlAttack) ||
                IsCheckState(ActionState.HeavyAttack) ||
                IsCheckState(ActionState.FinishAttack);
    }

    // ?????? ??????????????? ?????? (????????? ??? ?????? ??????)
    public void UpdateMove(Vector3 dir, Vector3 velocity)
    {
        PlayerAnim.SetMove(dir);
    }

    /**************************************************
     * ????????? ??????
     */
    public void Idle()
    {
        SetState(ActionState.Idle);
    }

    public void Stay()
    {
        SetState(MoveState.Stay);
        if (IsAttacking())
        {
            return;
        }
        if (IsCheckState(MoveState.Stay))
        {
            PlayerAnim.SetStay();
        }
    }

    public void Walk()
    {
        SetState(MoveState.Walk);
        PlayerInputMove.SetWalk();

        bool isPlayAnim = (IsAttacking() || IsCheckState(ActionState.Jump)) == false;
        PlayerAnim.SetWalk(isPlayAnim);
    }

    public void Run()
    {
        SetState(MoveState.Run);
        PlayerInputMove.SetRun();

        bool isPlayAnim = (IsAttacking() || IsCheckState(ActionState.Jump)) == false;
        PlayerAnim.SetRun(isPlayAnim);
    }

    public void Jump()
    {
        SetState(ActionState.Jump);
        PlayerAnim.Jump();
        PlayerInputMove.SetJump();
    }

    public void Landing()
    {
        SetState(ActionState.Landing);
        PlayerInputMove.SetLanding();
        PlayerAnim.Landing();
    }

    public void Dash(Vector3 dir, float dashHoldingTime)
    {
        if (IsAttacking())
        {
            return;
        }

        SetState(MoveState.Dash);
        StopCoroutine(CRDashEnd(dashHoldingTime));
        StartCoroutine(CRDashEnd(dashHoldingTime));
    }

    private IEnumerator CRDashEnd(float dashHoldingTime)
    {
        PlayerAnim.Dash();
        yield return new WaitForSeconds(dashHoldingTime);
        PlayerAnim.DashEnd();
        Stay();
    }

    public void RecoveryHp(int amount)
    {
        Logger.Log(Logger.Tag.State, "?????? ??? ?????? : " + PlayerStatus.HP);

        int recoveryHp = (PlayerStatus.MaxHP <= (PlayerStatus.HP + amount)) ? PlayerStatus.MaxHP : PlayerStatus.HP + amount;

        PlayerStatus.SetRecoveryHP(amount);
        PlayerEffector.RecoveryHpEffect();

        Logger.Log(Logger.Tag.State, "?????? ??? ?????? : " + PlayerStatus.HP);
    }

    public void Death()
    {
        Logger.Log("??????", "????");
    }

    public void ExpIncrease(int amount)
    {
        PlayerStatus.ExpIncrease(amount);
    }

    public void LevelUp()
    {
        PlayerEffector.LevelUp();
    }

    public void NormalAttack()
    {
        SetState(ActionState.NoramlAttack);
        rightWeaponHand.Attack(true);
        PlayerAnim.NoramlAttack();
        //     StopCoroutine("CRResetComboCount");
    }

    public void HeavyAttack()
    {
        SetState(ActionState.HeavyAttack);
        rightWeaponHand.Attack(true);
        PlayerAnim.HeavyAttack();
        //   StopCoroutine("CRResetComboCount");
    }

    public void EquipmentChange(WeaponStatus weapon)
    {
        rightWeaponHand.SetChangeWeapon(weapon);
    }

    // ????????? ????????? ?????? ????????? ??????
    public bool PickUpItem(GameItem item)
    {
        //    if(typeof(WeaponStatus).IsInstanceOfType(item)){
        if (item is WeaponStatus)
        {
            item.gameObject.SetActive(false);
            EquipmentChange((WeaponStatus)item);
            return true;
        }
        return false;
    }

    public void AttackEnd()
    {
        StopCoroutine("CRResetComboCount");
        StartCoroutine("CRResetComboCount");
        rightWeaponHand.Attack(false);
    }

    private IEnumerator CRResetComboCount()
    {
        if (PlayerAnim.IsMove())
        {
            Idle();
            Stay();
        }
        yield return new WaitForSeconds(1f);
        PlayerAnim.ResetComboCount();
    }

    public GameObject hudDamageText;
    public void OnAttackDetect(GameObject hitTarget, WeaponStatus weaponStatus)
    {
        Interaction.Attack target = hitTarget.GetComponent<Interaction.Attack>();
        if (target == null)
        {
            return;
        }
        target.OnAttackHit(weaponStatus.power * PlayerStatus.OffensePower);
        //????????? ?????? UI ???????????? kdy
        GameObject hudText = Instantiate(hudDamageText); // ????????? ????????? ????????????
        hudText.GetComponentInChildren<DamageUp>().DAMAGE = weaponStatus.power * PlayerStatus.OffensePower;
        hudText.transform.position = hitTarget.transform.position; // ????????? ??????
    }

    public void OnAttackHit(int damage)
    {
        damage -= PlayerStatus.OffensePower;
        if (damage < 1)
        {
            damage = 1;
        }
        PlayerStatus.SetDamage(damage);
    }





    // // ?????? ????????? ??????????????? ?????? ???????????? ?????? ??????

    // /*****************************
    //  * ?????? ?????? ?????????
    //  */

    // public void OnDailyStartEvent()
    // {
    //     throw new NotImplementedException();
    // }

    // public void OnBattleStartEvent()
    // {
    //     throw new NotImplementedException();
    // }

    // public void OnDailyEndEvent()
    // {
    //     throw new NotImplementedException();
    // }

    // public void OnBattleEndEvent()
    // {
    //     throw new NotImplementedException();
    // }

    // public void OnClimbStartEvent()
    // {
    //     throw new NotImplementedException();
    // }

    // public void OnFallDownStartEvent()
    // {
    //     throw new NotImplementedException();
    // }

    // public void OnClimbEndEvent()
    // {
    //     throw new NotImplementedException();
    // }

    // public void OnFallDownEndEvent()
    // {
    //     throw new NotImplementedException();
    // }



    // /*****************************
    //  * ??????, ?????? ?????? ?????????
    //  */

    // public void OnStayStartEvent(){
    //     Logger.Log(Logger.Tag.State, "stay start");
    // }

    // public void OnIdleStartEvent(){
    //     Logger.Log(Logger.Tag.State, "idle start");
    // }

    // public void OnWalkStartEvent()
    // {
    //     Logger.Log(Logger.Tag.State, "walk start");
    // }

    // public void OnRunStartEvent()
    // {
    //     Logger.Log(Logger.Tag.State, "run start");
    // }

    // public void OnDashStartEvent()
    // {
    //     Logger.Log(Logger.Tag.State, "dash start");
    // }

    // public void OnJumpStartEvent()
    // {
    //     Logger.Log(Logger.Tag.State, "jump start");
    // }

    //  public void OnLandingStartEvent()
    // {
    //     Logger.Log(Logger.Tag.State, "landing start");
    // }

    // public void OnAttackStartEvent()
    // {
    //     Logger.Log(Logger.Tag.State, "attack start");
    //     rightWeaponHand.Attack(true);
    // }



    // /*****************************
    //  * ??????, ?????? ?????? ?????????
    //  */

    // public void OnStayEndEvent(){
    //     Logger.Log(Logger.Tag.State, "stay end");
    // }

    // public void OnIdleEndEvent(){
    //     Logger.Log(Logger.Tag.State, "idle end");
    // }

    // public void OnWalkEndEvent()
    // {
    //     Logger.Log(Logger.Tag.State, "walk end");
    //     PlayerAnim.SetWalk(false);
    // }

    // public void OnRunEndEvent()
    // {
    //     Logger.Log(Logger.Tag.State, "run end");
    //     PlayerAnim.SetRun(false);
    // }

    // public void OnDashEndEvent()
    // {
    //     Logger.Log(Logger.Tag.State, "dash end");
    // }

    // public void OnJumpEndEvent()
    // {
    //     Logger.Log(Logger.Tag.State, "jump end");
    // }

    // public void OnLandingEndEvent()
    // {
    //     Logger.Log(Logger.Tag.State, "landing end");
    // }

    // public void OnAttackEndEvent()
    // {   

    // }
}
