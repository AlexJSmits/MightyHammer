using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    NavMeshAgent agent;


    public GameObject player;
    public GameObject damageZone;

    private Animator animator;
    private float distanceFromPlayer;

    private bool attacking;
    public bool inRange;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);


        //Walk Cycle
        if (distanceFromPlayer > agent.stoppingDistance && !attacking)
        {
            animator.SetFloat("Speed", Mathf.MoveTowards(animator.GetFloat("Speed"), 1, Time.deltaTime * 2));
            MoveToPlayer();
        }
        else
        {
            animator.SetFloat("Speed", Mathf.MoveTowards(animator.GetFloat("Speed"), 0, Time.deltaTime * 2));
        }   

        
        //Attacking Trigger
        if (distanceFromPlayer <= agent.stoppingDistance && !attacking)
        {
            Attack();
        }

        //Rotation Check
        if (distanceFromPlayer <= agent.stoppingDistance && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            RotateToPlayer();
        }

    }

    void MoveToPlayer()
    {
        agent.SetDestination(player.transform.position);
    }

    void RotateToPlayer()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    void Attack()
    {
        attacking = true;
        animator.SetTrigger("Attack");
        Invoke("DoDamage", 1f);
        Invoke("AttackCoolDown", 2);
    }

    void DoDamage()
    {

    }

    void AttackCoolDown()
    {
        attacking = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.gameObject.CompareTag("Player"))
        {
            inRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.gameObject.CompareTag("Player"))
        {
            inRange = false;
        }
    }

    private void OnDestroy()
    {

    }
}
