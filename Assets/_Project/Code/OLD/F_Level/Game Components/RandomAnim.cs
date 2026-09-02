using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Galactic1
{
    public class RandomAnim : MonoBehaviour
    {
        private Animator anim;


        private void OnEnable()
        {
            anim = GetComponent<Animator>();
            StartCoroutine(e());
        }


        IEnumerator e()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(2, 4));
                anim.SetTrigger("attack");
            }
        }
    }
}