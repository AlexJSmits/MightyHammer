using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.UIElements;

public class Hammer : MonoBehaviour
{
    [SerializeField] private InputActionReference leftSummonReference;
    [SerializeField] private InputActionReference rightSummonReference;
    [SerializeField] private InputActionReference summonLightningReference;

    public GameObject leftHand;
    public GameObject rightHand;

    public GameObject lightning;
    public GameObject impactEffect;

    public float returnSpeed;
    public float returnSpin;

    public AudioClip[] clips; 
    public float velocityCap;

    public Material[] materials;
    public LayerMask layerIgnoringCollision;

    public int _ignoreCollisionLayer;
    public int _hammerLayer;
    public int _playerLayer;

    private bool velocityCapOn;
    private AudioSource source;

    private bool leftSummon;
    private bool rightSummon;

    private Rigidbody rB;
    private MeshRenderer hammerGlow;

    private float distanceToLeft;
    private float distanceToRight;

    private bool isHolding;
    private bool holdingTrigger;



    void Start()
    {
        rB = GetComponent<Rigidbody>();
        source = GetComponent<AudioSource>();
        hammerGlow = GetComponentInChildren<MeshRenderer>();
        Physics.IgnoreLayerCollision(_ignoreCollisionLayer, _hammerLayer, false);
        leftSummonReference.action.performed += LeftRecall;
        rightSummonReference.action.performed += RightRecall;
        summonLightningReference.action.started += SummonLightning;
        summonLightningReference.action.canceled += StopSummonLightning;
    }


    public void LeftRecall(InputAction.CallbackContext context)
    {
        rB.useGravity = false;
        velocityCapOn = true;
        Physics.IgnoreLayerCollision(_ignoreCollisionLayer, _hammerLayer, true);
        rightSummon = false;
        leftSummon = true;
        Invoke("StuckCheck", 5);
    }

    public void RightRecall(InputAction.CallbackContext context)
    {
        rB.useGravity = false;
        velocityCapOn = true;
        Physics.IgnoreLayerCollision(3, 6, true);
        leftSummon = false;
        rightSummon = true;
        Invoke("StuckCheck", 5);
    }

    public void SummonLightning(InputAction.CallbackContext context)
    {
        lightning.SetActive(true);
        holdingTrigger = true;
        hammerGlow.material = materials[1];
    }

    void StopSummonLightning(InputAction.CallbackContext context)
    {
        lightning.SetActive(false);
        holdingTrigger = false;
        hammerGlow.material = materials[0];
    }

    private void Update()
    {

        //Audio Stuff
        if (rB.velocity.magnitude >= 5f && !source.isPlaying)
        {
            source.clip = clips[1];
            source.Play();
        }
        
        distanceToLeft = Vector3.Distance(transform.position, leftHand.transform.position);
        distanceToRight = Vector3.Distance(transform.position, rightHand.transform.position);

        //left commands
        if (leftSummon)
        {
            rB.AddForce((leftHand.transform.position - transform.position).normalized * returnSpeed, ForceMode.Impulse);
            rB.AddTorque(transform.forward * returnSpin);
            hammerGlow.material = materials[1];
        }

        //right commands
        if (rightSummon)
        {
            rB.AddForce((rightHand.transform.position - transform.position).normalized * returnSpeed, ForceMode.Impulse);
            rB.AddTorque(transform.forward * returnSpin);
            hammerGlow.material = materials[1];
        }

        if (distanceToLeft <= 0.5f || distanceToRight <= 0.5f)
        {
            leftSummon = false;
            rightSummon = false;
            velocityCapOn = false;
            Physics.IgnoreLayerCollision(_ignoreCollisionLayer, _hammerLayer, false);
            rB.useGravity = true;
        }

        if (distanceToLeft <= 1 && !holdingTrigger|| distanceToRight <= 1 && !holdingTrigger)
        {
            hammerGlow.material = materials[0];
        }

        if (velocityCapOn)
        {
            rB.velocity = Vector3.ClampMagnitude(rB.velocity, velocityCap);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (rB.velocity.magnitude >= 3 && !collision.gameObject.CompareTag("Hand"))
        {
            source.Stop();
            source.clip = clips[2];
            source.Play();

            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 pos = contact.point;
            Instantiate(impactEffect, pos, rot);
        }
    }

    void StuckCheck()
    {
        if (leftSummon || rightSummon)
        {
            GetComponent<MeshCollider>().isTrigger = true;
            Invoke("UnstuckCheck", 1f);
        }
    }

    void UnstuckCheck()
    {
        GetComponent<MeshCollider>().isTrigger = false;
    }

    public void OnGrab()
    {

        if (!isHolding)
        {
            source.Stop();
            source.clip = clips[0];
            source.Play();
            isHolding = true;
        }

    }

    public void OnRelease()
    {
        isHolding = false;

        Physics.IgnoreLayerCollision(_playerLayer, _hammerLayer, true);
        Invoke("ResetHammerCollision", 1);
    }

    void ResetHammerCollision()
    {
        Physics.IgnoreLayerCollision(_playerLayer, _hammerLayer, false);
    }
}
