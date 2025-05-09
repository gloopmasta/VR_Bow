using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasTronShader : MonoBehaviour
{
    // A GameObject with the main material attached on
    public GameObject Main;
    
    public Slider _WidthOutline;
    private float widthOutline;
    
    public Slider _WidthOutlineSharpness;
    private float widthOutlineSharpness;


    public Slider _WidthGrid;
    private float widthGrid;

    public Slider _WidthGridSharpness;
    private float widthGridSharpness;

    public Slider _NumberHorizzontal;
    private float numberHorizzontal;

     public Slider _NumberVertical;
    private float numberVertical;

    public Slider _Frequency;
    private float frequency;
    

    void Start()
    {
        _WidthOutline.value = Main.GetComponent<Renderer>().material.GetFloat("_WidthOutline");
        widthOutline = _WidthOutline.value;

        _WidthOutlineSharpness.value = Main.GetComponent<Renderer>().material.GetFloat("_WidthOutlineSharpness");
        widthOutlineSharpness = _WidthOutlineSharpness.value;

        _WidthGrid.value = Main.GetComponent<Renderer>().material.GetFloat("_WidthGrid");
        widthGrid = _WidthGrid.value;

        _WidthGridSharpness.value = Main.GetComponent<Renderer>().material.GetFloat("_WidthGridSharpness");
        widthGridSharpness = _WidthGridSharpness.value;

        _NumberHorizzontal.value = Main.GetComponent<Renderer>().material.GetFloat("_NumberHorizzontal");
        numberHorizzontal = _NumberHorizzontal.value;

        _NumberVertical.value = Main.GetComponent<Renderer>().material.GetFloat("_NumberVertical");
        numberVertical = _NumberVertical.value;

        _Frequency.value = Main.GetComponent<Renderer>().material.GetFloat("_Frequency");
        frequency = _Frequency.value;
    }

    public void ResetStats()
    {
        _WidthOutline.value = widthOutline;
        _WidthOutlineSharpness.value =  widthOutlineSharpness;
        _WidthGrid.value = widthGrid;
        _WidthGridSharpness.value = widthGridSharpness;
        _NumberHorizzontal.value = numberHorizzontal;
        _NumberVertical.value = numberVertical;
        _Frequency.value = frequency;


    }

    public void ChangeWidthOutline()
    {
        Main.GetComponent<Renderer>().material.SetFloat("_WidthOutline", _WidthOutline.value);
    }

    public void ChangewidthOutlineSharpness()
    {
        Main.GetComponent<Renderer>().material.SetFloat("_WidthOutlineSharpness", _WidthOutlineSharpness.value);
    }


    public void ChangeWidthGrid()
    {
        Main.GetComponent<Renderer>().material.SetFloat("_WidthGrid", _WidthGrid.value);
    }

    public void ChangewidthGridSharpness()
    {
        Main.GetComponent<Renderer>().material.SetFloat("_WidthGridSharpness", _WidthGridSharpness.value);
    }

    public void ChangeNumberHorizzontal()
    {
        Main.GetComponent<Renderer>().material.SetFloat("_NumberHorizzontal", _NumberHorizzontal.value);
    }

    public void ChangeNumberVertical()
    {
        Main.GetComponent<Renderer>().material.SetFloat("_NumberVertical", _NumberVertical.value );
    }

    public void ChangeFrequency()
    {
        Main.GetComponent<Renderer>().material.SetFloat("_Frequency", _Frequency.value * 100);
    }


}
