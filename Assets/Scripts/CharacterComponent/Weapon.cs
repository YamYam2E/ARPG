using System;
using UnityEngine;

namespace CharacterComponent
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private int damage;

        public int Damage => damage;

        private void Awake()
        {
            GetComponent<BoxCollider>().enabled = false;
        }

        //Animation Event
        public void SetAbleEvent(bool ableAttackingEvent)
        {
            GetComponent<BoxCollider>().enabled = ableAttackingEvent;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag($"Enemy"))
            {
                Debug.Log("Hit");
                //col.GetComponent<Enemy>().TakeDamage(10);
            }
        }
    }
}