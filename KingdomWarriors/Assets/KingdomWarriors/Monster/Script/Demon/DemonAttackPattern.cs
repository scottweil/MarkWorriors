using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DemonAttackPattern : MonsterAttackPatternCommon
{
    public GameObject DemonDeathSound;
    AudioSource DemonDeathSource;
    AudioClip DemonDeathClip;
    public GameObject DemonRushSound;
    AudioSource DemonRushSource;
    AudioClip DemonRushClip;
    public GameObject monsterAttackSound;
    AudioSource monsterAttackSource;
    AudioClip monsterAttackClip;
    public GameObject monsterClankSound;
    AudioSource monsterClankSource;
    AudioClip monsterClankClip;
    public ParticleSystem dust;
    Rigidbody rb;
    private void Awake()
    {
        gameObject.SetActive(false);
        DemonDeathSource = DemonDeathSound.GetComponent<AudioSource>();
        DemonDeathClip = DemonDeathSource.clip;
        monsterAttackSource = monsterAttackSound.GetComponent<AudioSource>();
        monsterAttackClip = monsterAttackSource.clip;
        monsterClankSource = monsterClankSound.GetComponent<AudioSource>();
        monsterClankClip = monsterClankSource.clip;
        DemonRushSource = DemonRushSound.GetComponent<AudioSource>();
        DemonRushClip = DemonRushSource.clip;

    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        nvAgent = GetComponent<NavMeshAgent>();
        traceRadius = traceZone.transform.localScale.x * 0.5f;
        setState(State.Idle, "Idle");
        //        patrolIndex = UnityEngine.Random.Range(0, PatrolLocation.instance.patrolPoints.Length);
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        switch (state)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Chase:
                UpdateChase();
                break;
            case State.Attack:
                UpdateAttack();
                break;
        }
    }
    // internal override void MonsterAttackActivation()
    // {
    //     base.MonsterAttackActivation();
    //     DemonGrowlSource.PlayOneShot(DemonGrowlClip);
    // }

    private void UpdateAttack()
    {
        //?????? ?????? ??? ??????????????? ?????? ????????? ????????? ??????????????? ???????????? ??????.(????????????)
        //???????????? ?????? ????????? ????????? ??????.
        nvAgent.isStopped = true;

        setState(State.Attack, "Attack");

        //???????????? ??????????????? ???????????? ????????? ??????.
        Vector3 monsterLookForward = target.transform.position;
        monsterLookForward.y = transform.position.y;
        transform.LookAt(monsterLookForward);
        Vector3 dir = transform.position - target.transform.position;

        // transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 2);
    }
    public float rushPower = 30;
    IEnumerator IERush()
    {
        dust.Play();
        float distToPlayer = Vector3.Distance(transform.position, target.transform.position); //target?????? ??????
        isAttack = true;
        state = State.Rush;
        anim.SetTrigger("Rush");
        yield return new WaitForSeconds(1f);
        nvAgent.enabled = false;
        rb.isKinematic = false;
        rb.AddForce(transform.forward * rushPower, ForceMode.Impulse);
        yield return new WaitForSeconds(1f);
        dust.Stop();
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        // if (distToPlayer > nvAgent.stoppingDistance)
        // {
        //     print("Idle");
        //     nvAgent.enabled = true;
        //     isAttack = false;
        //     setState(State.Idle, "Idle");
        //     yield break;
        // }
        // // rb.isKinematic = true;
        nvAgent.enabled = true;
        isAttack = false;
        setState(State.Chase, "Chase");
    }

    private void UpdateChase()
    {
        //print("Chase");
        //????????? ?????? ???????????? ??????.
        nvAgent.isStopped = false;

        //???????????? ??????????????? 4??? ?????? ??????.
        nvAgent.speed = 4;

        // setState(State.Chase, "Chase");
        float distToPlayer = Vector3.Distance(transform.position, target.transform.position); //target?????? ??????

        //target ????????? ????????? ?????? ??????.
        nvAgent.destination = target.transform.position;



        //???????????? ????????? ??????????????? ????????? ????????? ?????? ??????.
        RaycastHit[] attackTarget = Physics.SphereCastAll(transform.position, 3f, transform.forward, traceRadius - 5, 1 << LayerMask.NameToLayer("Player"));

        if (distToPlayer >= traceRadius - 5 && attackTarget.Length > 0 && isAttack == false)
        {
            //???????????? ??????.
            StartCoroutine(IERush());
        }
        else if (distToPlayer <= nvAgent.stoppingDistance)
        {
            //StopCoroutine(IERush());
            setState(State.Attack, "Attack");
        }
    }
    private void UpdateIdle()
    {

        nvAgent.isStopped = true;

        int layerMask = 1 << LayerMask.NameToLayer("Player"); //Player??? ????????? ?????? Layer ?????????
        //???????????? traceZone ?????? Player??? ????????? ?????? ???????????? ??????.
        Collider[] cols = Physics.OverlapSphere(transform.position, traceRadius, layerMask);

        //???????????? ?????? Player??? ?????????
        if (cols.Length > 0)
        {
            target = cols[0].gameObject; //target??? Player??? ??????.  
            //????????? Chase??? ????????? ??????.
            setState(State.Chase, "Chase");
        }
    }
    internal override void OnMonsterDeathAnimFinished()
    {
        DemonDeathSource.PlayOneShot(DemonDeathClip);
        base.OnMonsterDeathAnimFinished();
        GeneratorManager.instance.DEMONKILLCOUNT--;
    }
    internal override void MonsterGrowlSoundActivation()
    {
        monsterAttackSource.PlayOneShot(monsterAttackClip);
    }
    internal override void MonsterReactSoundActivation()
    {
        monsterClankSource.PlayOneShot(monsterClankClip);
    }
    internal override void DemonRushSoundActivation()
    {
        DemonRushSource.PlayOneShot(DemonRushClip);
    }
}