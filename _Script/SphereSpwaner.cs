using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
//*****************************************
//创建人： SamLee 
//功能说明：
//***************************************** 
public class SphereSpwaner : NetworkSingleton<SphereSpwaner>
{
    [SerializeField] private GameObject prefabToInstantiate;
    [SerializeField] private int instanciateAmount;

    void Start()
    {

    }

    void Update()
    {

    } 

    public void InstantiateSpheres()
    {
        if (!IsServer) return; //only the server instantiate

        for(int i = 0;i < instanciateAmount; i++)
        {
            GameObject go = Instantiate(prefabToInstantiate,
                new Vector3(Random.Range(-10, 10), 10, Random.Range(-10, 10)), Quaternion.identity);
            go.GetComponent<NetworkObject>().Spawn(); //synchronize to client
        }



    }
}
