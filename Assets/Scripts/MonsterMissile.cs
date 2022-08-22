using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMissile : MonoBehaviour
{
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
        //rigid.AddForce(force: Vector3.right * speed, mode: ForceMode.Impulse);
        transform.Rotate(axis: Vector3.forward, angle: rotateSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == NameManager.TAG_WALL)
        {
            Destroy(this.gameObject);
        }
    }
}
