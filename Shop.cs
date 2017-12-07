using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum EItem
{
    k_EItem_None,
    k_EItem_Rocket,
    k_EItem_Speed,
    k_EItem_Invincibility,
    k_EItem_Rotation,
    k_EItem_Shrink,
    k_EItem_Triangle,
    k_EItem_Slowmo,
};

public class Shop : MonoBehaviour {    
    public List<ShopItem> shopItems = new List<ShopItem>();
    public bool shopDisplayed;    
    public Button[] shopButtons;
    public GameObject DeathZone;
    private ShopItem[] shopStockedItems = { new ShopItem(), new ShopItem(), new ShopItem() };

    private GameController gc;
    private Items items;
    public float m_flShopClosed;

    void Awake()
    {
        gc = GameObject.Find("GameController").GetComponent<GameController>();
        items = GameObject.Find("Character").GetComponent<Items>();
        
        

        //Add shopitems here
        //Unique ID, Item Name, Sprite name (in resource folder), Price, Uses, Unique                
        shopItems.Add(new ShopItem(EItem.k_EItem_None, "None", Resources.Load<Sprite>("none"), 0, 0));
        shopItems.Add(new ShopItem(EItem.k_EItem_Rocket, "Rockets", Resources.Load<Sprite>("rocket"), 150, 3));               
        shopItems.Add(new ShopItem(EItem.k_EItem_Speed, "Speed Upgrade", Resources.Load<Sprite>("plusMove"), 100, 0));
        shopItems.Add(new ShopItem(EItem.k_EItem_Invincibility, "Invincibility", Resources.Load<Sprite>("potion"), 500, 0));
        shopItems.Add(new ShopItem(EItem.k_EItem_Rotation, "Rotation", Resources.Load<Sprite>("rotation"), 400, 0, true));
        shopItems.Add(new ShopItem(EItem.k_EItem_Shrink, "Shrink", Resources.Load<Sprite>("shrink"), 100, 2));
        shopItems.Add(new ShopItem(EItem.k_EItem_Triangle, "Triangle", Resources.Load<Sprite>("triangle"), 750, 0, true));
        shopItems.Add(new ShopItem(EItem.k_EItem_Slowmo, "Slow-Mo", Resources.Load<Sprite>("SlowMo"), 200, 2));

        
        foreach (Button button in shopButtons)
        {
            button.gameObject.SetActive(false);
        }        
        //items.itemApplyBonuses(EItem.k_EItem_Invincibility);
    }
    void Start()
    {
        items.setItem(EItem.k_EItem_None);
    }
    void Update()
    {
        if(m_flShopClosed > 0)
        {
            Debug.Log("shopclosed: " + m_flShopClosed);
            if (Time.time >= m_flShopClosed + 0.5f)
                m_flShopClosed = 0;
        }
    }

    void randomizeItems(bool firstshop)
    {
        int item1 = Random.Range(1, shopItems.Count);
        int item2 = Random.Range(1, shopItems.Count);
        int item3 = Random.Range(1, shopItems.Count);
        //int item1 = UnityEngine.Random.Range(0, shopItems.Count);
        if (!firstshop)
        {
            while (shopItems[item1].Price > gc.getScore())
                item1 = Random.Range(1, shopItems.Count);
            //prevent duplicates
            while (item2 == item1)
                item2 = Random.Range(1, shopItems.Count);
            while (item3 == item1 || item3 == item2 || (shopItems[item1].Uses == 0 && shopItems[item2].Uses == 0 && shopItems[item3].Uses == 0))
                item3 = Random.Range(1, shopItems.Count);
        }
        else
        {
            item1 = (int)EItem.k_EItem_Rocket;
            item2 = (int)EItem.k_EItem_Slowmo;
            item3 = (int)EItem.k_EItem_Shrink;
        }


        for (int i = 0; i < 3; i++)
        {
            int item = 0;
            switch (i)
            {
                case 0:
                    item = item1;
                    break;
                case 1:
                    item = item2;
                    break;
                case 2:
                    item = item3;
                    break;
            }
            
            shopStockedItems[i] = shopItems[item];
            
            shopButtons[i].GetComponent<Image>().overrideSprite = shopStockedItems[i].Sprite;            
            shopButtons[i].GetComponentInChildren<Text>().text = shopStockedItems[i].Price.ToString();

            if (shopStockedItems[i].Price > gc.getScore() && !firstshop)
            {
                shopButtons[i].interactable = false;                
                shopButtons[i].GetComponentInChildren<Text>().color = Color.red;
                shopButtons[i].GetComponent<Image>().color = new Color(0, 0, 0, .5f);
            }
            else
            {
                shopButtons[i].interactable = true;
                shopButtons[i].GetComponentInChildren<Text>().color = Color.yellow;
                if (firstshop)
                {
                    shopButtons[i].GetComponentInChildren<Text>().color = Color.green;
                    shopButtons[i].GetComponentInChildren<Text>().text = "";
                }
                shopButtons[i].GetComponent<Image>().color = Color.white;
            }
        }
    }

    public void displayShop(bool firstShop)
    {        
        randomizeItems(firstShop);
        shopDisplayed = true;
        foreach (Button button in shopButtons)
        {
            button.gameObject.SetActive(true);
        }
        Cursor.visible = true;
        shopButtons[0].Select();        
        gc.deathText.text = "SELECT ITEM TO CONTINUE";
        DeathZone.SetActive(false);        
    }

    public void hideShop()
    {
        shopDisplayed = false;
        foreach (Button button in shopButtons)
        {
            button.gameObject.SetActive(false);
        }
        gc.deathText.text = "";
        DeathZone.SetActive(true);
        m_flShopClosed = Time.time;
        Cursor.visible = false;
    }

    public void shopButtonPressed(int button)
    {
        if(!shopButtons[button].interactable)
            return;
        //Debug.Log("button: " + button + " was pressed.");
        if (shopStockedItems[button].Price <= gc.getScore() || shopButtons[button].GetComponentInChildren<Text>().color == Color.green)
        {
            if (shopStockedItems[button].ID > 0)
            {
                gc.createTextPopup(shopStockedItems[button].Name, Color.green);
                if (shopStockedItems[button].Unique)
                    removeItemFromList(shopStockedItems[button].ID);
                if (shopStockedItems[button].Uses == 0) //Passive item, apply bonuses
                    items.itemApplyBonuses(shopStockedItems[button].ID);
                else
                {
                    items.setItem(shopStockedItems[button].ID, shopStockedItems[button].Uses);                    
                    //Debug.Log("Item purchased: " + shopStockedItems[button].Name);                    
                }
            }
            if(shopButtons[button].GetComponentInChildren<Text>().color != Color.green)
                gc.setScore(gc.getScore() - shopStockedItems[button].Price);
        }
        hideShop();
        gc.scoreKeeper();
        if (gc.stageTier == 1 && shopButtons[button].GetComponentInChildren<Text>().color != Color.green)        
            gc.increaseDifficulty();        
    }

    public void removeItemFromList(EItem ID)
    {
        foreach (ShopItem item in shopItems)
        {
            if (item.ID == ID)
            {
                shopItems.Remove(item);
                return;
            }
        }
    }

    public ShopItem getItemByID(EItem ID)
    {
        foreach (ShopItem item in shopItems)
        {
            if (item.ID == ID)
                return item;
        }
        return null;
    }
}

public class ShopItem
{
    public EItem ID;
    public string Name;
    public Sprite Sprite;
    public int Price;
    public int Uses;
    public bool Unique;

    public ShopItem()
    { }
    public ShopItem(EItem itemID, string itemName, Sprite ItemSprite, int itemPrice, int itemUses, bool isUnique = false)
    {
        ID = itemID;
        Name = itemName;
        Sprite = ItemSprite;
        Price = itemPrice;
        Uses = itemUses;
        Unique = isUnique;
    }
}
