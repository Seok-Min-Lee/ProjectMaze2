using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMissile : MonoBehaviour
{
    public GameObject mesh, boosterEffect, explosionEffect;
    public int speed;
    public int damage;

    Rigidbody rigid;
    float rotateSpeed;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rotateSpeed = 15 * Time.deltaTime;
    }

    private void Update()
    {
        transform.Rotate(axis: Vector3.forward, angle: rotateSpeed);
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
        rigid.angularVelocity = Vector3.zero;
        rigid.velocity = Vector3.zero;

        mesh.SetActive(false);
        boosterEffect.SetActive(false);
        explosionEffect.SetActive(true);

        Destroy(obj: this.gameObject, t: 1f);
    }
}
