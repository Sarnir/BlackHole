using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public Color[] skinColors = new Color[4];
    ShopButton[] buttons;

    Game Game;
    bool[] skinsBought = new bool[4];
    
    public void Init()
    {
        Game = FindObjectOfType<Game>();
        buttons = transform.GetComponentsInChildren<ShopButton>();
        for (int i = 0; i < buttons.Length; i++)
        {
            var price = 5 * (i + 1);
            buttons[i].Init(skinColors[i], price);

            if (PlayerPrefs.GetInt("SKIN_" + i, 0) == 1)
                skinsBought[i] = true;
        }

        RefreshUI();
    }

    public void Enter()
    {
        gameObject.SetActive(true);
        RefreshUI();
    }

    public void OnBuyClick(int element)
    {
        Game.PlayClickSound();
        if (!skinsBought[element])
        {
            Debug.Log("Bought " + element);

            Game.ReduceDiamonds(buttons[element].GetPrice());

            skinsBought[element] = true;
            PlayerPrefs.SetInt("SKIN_" + element, 1);
        }

        Game.SetPlayerSkin(skinColors[element]);
        RefreshUI();
    }

    public void RefreshUI()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (skinsBought[i])
            {
                buttons[i].SetOwned();
            }
            else if(Game.GetDiamonds() < buttons[i].GetPrice())
            {
                buttons[i].SetInactive();
            }
            else
            {
                buttons[i].SetDefault();
            }
        }
    }
}
