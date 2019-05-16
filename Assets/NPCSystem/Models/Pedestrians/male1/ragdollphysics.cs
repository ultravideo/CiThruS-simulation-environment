using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ragdollphysics : MonoBehaviour
{

    public Rigidbody rb;

    float ctime = 0;

    bool apply = true;

    void Start()
    {
        // rb.isKinematic = false;
        // rb.AddForce(Vector3.up * 150f, ForceMode.Impulse);
    }

    void Update()
    {
        ctime += Time.deltaTime;

        if (ctime > 5f && apply)
        {
            rb.isKinematic = false;
            rb.AddForce(transform.forward * 600f, ForceMode.Impulse);

            apply = false;
        }


        //return;

        //RaycastHit hit;

        //if (Physics.SphereCast(transform.position, 1f, transform.forward, out hit))
        //{
        //    if (hit.collider.gameObject.CompareTag("Replayable"))
        //    {
        //        rb.isKinematic = false;
        //        Vector3 cpos = hit.normal;
        //        rb.AddForce(cpos * 100f + Vector3.up * 25f, ForceMode.Impulse);                
        //    }
        //}
    }
}
