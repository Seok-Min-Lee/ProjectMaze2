using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMissile : MonoBehaviour
{
    public GameObject mesh, boosterEffect, explosionEffect;
    public int speed;
    public int damage;

    private Rigidbody rigid;
    private float rotateSpeed;

    private void Awake()
    {
        this.rigid = GetComponent<Rigidbody>();
        this.rotateSpeed = 15 * Time.deltaTime;
    }

    private void Update()
    {
        this.transform.Rotate(axis: Vector3.forward, angle: rotateSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == NameManager.TAG_WALL)
        {
            Destroy(this.gameObject);
        }
    }

    public void ExplosionDestroy()
    {
        this.rigid.angularVelocity = Vector3.zero;
        this.rigid.velocity = Vector3.zero;

        this.mesh.SetActive(false);
        this.boosterEffect.SetActive(false);
        this.explosionEffect.SetActive(true);

        Destroy(obj: this.gameObject, t: 1f);
    }
}
