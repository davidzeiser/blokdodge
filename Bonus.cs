using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonus : MonoBehaviour {
    private GameController m_GameController;
    private Shop m_Shop;
    private float m_flTimeBetweenDrops;
    private float m_flLastDrop;
    public float MaxDropWait = 15;
    public float MinDropWait = 5;
    
    public GameObject m_Pickup;
    // Use this for initialization
    void Awake () {
        m_Shop = GameObject.Find("Canvas").GetComponent<Shop>();
        m_GameController = GameObject.Find("GameController").GetComponent<GameController>();
        m_flLastDrop = Time.time;
        m_flTimeBetweenDrops = Random.Range(MinDropWait, MaxDropWait);
        //Debug.Log("Spawning bonus in: " + m_flTimeBetweenDrops);
    }
	
	// Update is called once per frame
	void Update () {
        if (m_GameController.gameEnding)
            return;
        if (m_Shop.shopDisplayed)
            m_flLastDrop = Time.time;

        //hacky fix to stop spawning right before shop opens
        if (m_GameController.stageTier == 6 || m_GameController.stageTier == 1) 
            return;


		if(m_flLastDrop + m_flTimeBetweenDrops <= Time.time)
        {            
            spawnItem();
           
        }
	}

    void spawnItem()
    {
        GameObject spawn;
        if (Random.value > 0.5f)
        {
            spawn = Instantiate(m_Pickup, new Vector3(Random.Range(-3, 7), -6), Quaternion.identity);
            spawn.GetComponent<Rigidbody2D>().gravityScale = -0.6f;
        }
        else
            spawn = Instantiate(m_Pickup, new Vector3(Random.Range(-3, 7), 6), Quaternion.identity);

        Destroy(spawn, 5);
        ;
        m_flLastDrop = Time.time;// + Random.Range(-5,5);
        m_flTimeBetweenDrops = Random.Range(MaxDropWait, MinDropWait);
        //Debug.Log("Spawning bonus in: " + m_flTimeBetweenDrops);        
    }    
}
