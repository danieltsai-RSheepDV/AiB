using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IntroStar : MonoBehaviour
{
    public int counter;
    public bool collected = false;
    public Transform endPos;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (collected)
        {
            transform.position = Vector3.Lerp(transform.position, endPos.position, Time.deltaTime);
        }
    }

    public void UpdateCounter()
    {
        counter++;
        if (counter >= 6)
        {
            GetComponent<MeshRenderer>().enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (counter >= 6 && other.gameObject.CompareTag("Player"))
        {
            collected = true;
            GetComponent<Collider>().enabled = false;
        }
    }
}
