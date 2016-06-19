using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour
{
    Button button;
    Image colorImage;
    GameObject priceObject;
    Text priceLabel;
    int price;
    
    public void Init(Color _color, int _price)
    {
        button = GetComponent<Button>();
        colorImage = GetComponent<Image>();
        priceObject = transform.GetChild(0).gameObject;
        priceLabel = priceObject.GetComponentInChildren<Text>();

        colorImage.color = _color;
        price = _price;
    }

    public void SetOwned()
    {
        button.interactable = true;
        priceObject.SetActive(false);
        priceLabel.color = Color.white;
    }

    public void SetInactive()
    {
        button.interactable = false;
        priceObject.SetActive(true);
        priceLabel.color = Color.red;
    }

    public void SetDefault()
    {
        button.interactable = true;
        priceObject.SetActive(true);
        priceLabel.color = Color.white;
    }

    public int GetPrice()
    {
        return price;
    }
}
