using UnityEngine;
using System.Collections;

public class Shop : MonoBehaviour
{
    public Color[] skinColors = new Color[4];

    Game Game;
    bool[] skinsBought = new bool[4];

    void Start()
    {
        Game = FindObjectOfType<Game>();
        for(int i = 0; i < 4; i++)
        {
            if (PlayerPrefs.GetInt("SKIN_" + i, 0) == 1)
                skinsBought[i] = true;
        }
    }

    public void OnBuyClick(int element)
    {
        if (!skinsBought[element])
        {
            Debug.Log("Bought " + element);
            var price = 5 * (element + 1);

            Game.ReduceDiamonds(price);

            skinsBought[element] = true;
            PlayerPrefs.SetInt("SKIN_" + element, 1);
        }

        Game.SetPlayerSkin(skinColors[element]);
    }
}
