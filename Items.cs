using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Items : MonoBehaviour {

    private EItem itemCurrent = EItem.k_EItem_None;
    private int itemUsesRemaining;

    private GameObject character;
    private Transform triangle;
    AudioSource sound;
    public Image itemDisplay;
    public Image shieldDisplay;
    private BlockMove playerScript;
    private GameController gc;
    private Shop shop;
    Color oldColor;

    //Rocket globals
    public GameObject rocket;
    private float m_lastShot;
    private float rocketFireRate = 0.15f;
    public float rocketSpeed = 10;    

    //Shrink globals
    private float shrinkTimeStamp;
    public float shrinkCoolDown = 2;
    public Color shrinkColor = Color.yellow;
    public AudioClip shrinkSound;

    //Invincibility globals
    public float invincibilityDuration = 5;
    public Color invincibilityColor = Color.magenta;
    bool invincibilityFadeOut;
    float invincibilityFade;
    public AudioClip invincibilitySound;

    //Slowmo globals
    public float slowDuration = 2;
    public float slowAmount = .6f;
    private int m_nShields;
    private bool slowEnabled;

    private List<EItem> m_lItemsUsed = new List<EItem>();
    //Other sounds
    public AudioClip triangleSound;
    SteamStatsAndAchievements m_StatsAndAchievements;
    //public AudioClip movespeedSound;


    void Awake()
    {
        character = GameObject.Find("Character");
        sound = GetComponent<AudioSource>();
        shop = GameObject.Find("Canvas").GetComponent<Shop>();
        gc = GameObject.Find("GameController").GetComponent<GameController>();
        playerScript = character.GetComponent<BlockMove>();
        //setItem(0);
        oldColor = character.GetComponent<SpriteRenderer>().color;
        m_StatsAndAchievements = GameObject.FindObjectOfType<SteamStatsAndAchievements>();
        triangle = character.transform.Find("triangle");

    }
    
    void Update()
    {
        if (invincibilityFadeOut)
            fadeColor();  
    }   

    public void useItem()
    {
        if (gc.gameEnding)
            return;
        //Debug.Log(shop.getItemByID(itemCurrent).Name);
        if (itemUsesRemaining > 0)
        {
            switch (itemCurrent)
            {
                case EItem.k_EItem_None: 
                        return;                    
                case EItem.k_EItem_Rocket:
                    if (!FireRocket())
                        return;
                    break;                
                case EItem.k_EItem_Shrink:
                    if (!Shrink())
                        return;
                    break;
                case EItem.k_EItem_Slowmo:
                    if (!SlowStart())
                        return;
                    break;
            }
            m_lItemsUsed.Add(itemCurrent);
            itemUsesRemaining--;
            itemDisplay.GetComponentInChildren<Text>().text = itemUsesRemaining.ToString();            
			if (itemUsesRemaining == 0) {
				setItem (EItem.k_EItem_None);
			}
        }
    }

    public void useShield()
    {
        if(m_nShields > 0)
        {
            if (!Invincible())
                return;
            m_nShields--;
            shieldDisplay.GetComponentInChildren<Text>().text = m_nShields.ToString();
            if(m_nShields == 0)
            {
                shieldDisplay.gameObject.SetActive(false);
            }
        }
    }
    public int getShields()
    {
        return m_nShields;
    }

    public void itemApplyBonuses(EItem ID) //Passive items
    {
        switch (ID)
        {
            case EItem.k_EItem_Speed: //Speed upgrade
                playerScript.speedBonus += (playerScript.moveSpeed * .15f); //add 15% of base speed
                break;
            case EItem.k_EItem_Rotation: //rotation
                playerScript.allowRotation();
                break;
            case EItem.k_EItem_Invincibility: //invinciblity
                m_nShields++;
                shieldDisplay.gameObject.SetActive(true);
                shieldDisplay.GetComponentInChildren<Text>().text = m_nShields.ToString();
                break;
            case EItem.k_EItem_Triangle: //triangle
                playerScript.makeTriangle();
                sound.PlayOneShot(triangleSound);
                break;
        }
        m_lItemsUsed.Add(ID);
    }

    public List<EItem> getUsedItems()
    {
        return m_lItemsUsed;
    }

    bool FireRocket()
    {
        if (Time.time >= m_lastShot + rocketFireRate)
        {
            GameObject rocketClone = (GameObject)Instantiate(rocket, new Vector2(character.transform.position.x + 1, character.transform.position.y), Quaternion.Euler(0, 0, -90));
            if (!playerScript.isTriangle)
                Physics2D.IgnoreCollision(rocketClone.GetComponent<Collider2D>(), character.GetComponent<Collider2D>(), true);           
            else
                Physics2D.IgnoreCollision(rocketClone.GetComponent<Collider2D>(), character.transform.Find("triangle").GetComponent<Collider2D>(), true);            

            rocketClone.GetComponent<Rigidbody2D>().velocity = Vector3.right * rocketSpeed;

            m_lastShot = Time.time;
            m_StatsAndAchievements.IncreaseRocketsFired();
            return true;    
        }
        return false;
    }

    bool Shrink()
    {
        if (shrinkTimeStamp <= Time.time)
        {
            sound.PlayOneShot(shrinkSound);
            if (!playerScript.isTriangle)            
                character.GetComponent<SpriteRenderer>().color = shrinkColor;            
            else
                triangle.GetComponent<SpriteRenderer>().color = shrinkColor;
                        
            character.GetComponent<Animator>().SetTrigger("shrinkSquare");
            shrinkTimeStamp = Time.time + shrinkCoolDown;
            StartCoroutine(RevertStatus(shrinkCoolDown));
            return true;
        }
        return false;
    }

    bool Invincible()
    {
        if (!playerScript.godmode)
        { 
        invincibilityFade = 0;
        sound.PlayOneShot(invincibilitySound);
        if (!playerScript.isTriangle)
            character.GetComponent<SpriteRenderer>().color = invincibilityColor;
        else
            triangle.GetComponent<SpriteRenderer>().color = invincibilityColor;

        playerScript.godmode = true;
        invincibilityFadeOut = true;
            //StartCoroutine(RevertStatus(invincibilityDuration));
            return true;
        }
        return false;
    }

    bool SlowStart()
    {        
        if (!slowEnabled)
        {
            Time.timeScale = slowAmount;
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
            foreach (AudioSource AS in FindObjectsOfType<AudioSource>())
                AS.pitch = Time.timeScale;
            StartCoroutine(SlowEnd());
            slowEnabled = true;
            playerScript.speedBonus += 3;
            return true;
        }
        return false;
    }

    IEnumerator SlowEnd()
    {
        yield return new WaitForSeconds(slowDuration);
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
        foreach (AudioSource AS in FindObjectsOfType<AudioSource>())
            AS.pitch = Time.timeScale;

        playerScript.speedBonus -= 3;
        slowEnabled = false;
    }

    void fadeColor()
    {
        if (!playerScript.isTriangle)
            character.GetComponent<SpriteRenderer>().color = Color.Lerp(invincibilityColor, oldColor, invincibilityFade);
        else
            triangle.GetComponent<SpriteRenderer>().color = Color.Lerp(invincibilityColor, oldColor, invincibilityFade);

        if (invincibilityFade < 1)
            invincibilityFade += Time.deltaTime / invincibilityDuration;
        else
        {
            invincibilityFadeOut = false;
            playerScript.godmode = false;
        }
    }

    IEnumerator RevertStatus(float cd)
    {
        yield return new WaitForSeconds(cd);
        if (!playerScript.isTriangle)
            GetComponent<SpriteRenderer>().color = oldColor;
        else
            character.transform.Find("triangle").GetComponent<SpriteRenderer>().color = oldColor;        
    }

    public void setItem(EItem itemID, int itemUses = 0)
    {
        bool enabled = itemID > 0 ? true : false;
        itemDisplay.gameObject.SetActive(enabled);
        if (itemCurrent == itemID)
            itemUsesRemaining += itemUses;
        else
        {
            itemCurrent = itemID;
            itemUsesRemaining = itemUses;
        }
        itemDisplay.GetComponentInChildren<Text> ().text = (itemUsesRemaining > 0) ? itemUsesRemaining.ToString () : itemDisplay.GetComponentInChildren<Text>().text = "";
		
		if (enabled) {
			itemDisplay.sprite = shop.getItemByID(itemID).Sprite;
		}
    }

    public EItem getItem(out int usesRemaining)
    {
        usesRemaining = itemUsesRemaining;
        return itemCurrent;
    }
}
