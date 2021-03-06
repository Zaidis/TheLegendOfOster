using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPointer : MonoBehaviour
{
    [SerializeField] string targetTag;
    public int range = 10;
    public bool point;
    GameObject[] objs;
    GameObject closestObj;
    private void Start()
    {
        objs = GameObject.FindGameObjectsWithTag(targetTag);
    }
    // Update is called once per frame
    void Update()
    {
        if (point)
        {
            FindClosestObjectWithTag();

            if (closestObj != null)
            {
                this.transform.LookAt(closestObj.transform);
            }
            else
            {
                closestObj = gameObject;
            }
        }
    }

    void FindClosestObjectWithTag()
    {
        //If there is no closest object and the array isnt empty, then default to the first object. Otherwise, if the 
        //Array is empty, just return.
        if (closestObj == null && objs.Length > 0)
            closestObj = objs[0];
        else if (objs.Length <= 0)
            return;

        for (int i = 0; i < objs.Length; i++)
        {
            float distToCheck = Vector3.Distance(this.transform.position, objs[i].transform.position);
            float distToCurrent = Vector3.Distance(this.transform.position, closestObj.transform.position);
            if ((distToCheck < distToCurrent))
            {
                closestObj = objs[i];
            }
        }
    }
}
