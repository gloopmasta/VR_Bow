using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeColor : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Image img;
    private int c = 0;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //GetComponent<Image>().color = Color.HSVToRGB(c, 100, 100);

        spriteRenderer.color = Color.HSVToRGB(c, 100, 100);
        c++;
        if (c >= 359)
        {
        c = 1;
        }
    }
}
