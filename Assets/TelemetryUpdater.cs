using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required when Using UI elements.

public class TelemetryUpdater : MonoBehaviour
{

    public float adc_val0;
    public float adc_val1;
    public float adc_val2;
    public float adc_val3;
    public float adc_val4;
    public float adc_val5;
    public float adc_val6;
    public float adc_val7;
    public float adc_val8;
    public float adc_val9;
    public float batteryVoltage;
    public float posLeft;
    public float posRight;

    public int triggers1;
    public int triggers2;

    public Slider ADC0;
    public Slider ADC1;
    public Slider ADC2;
    public Slider ADC3;
    public Slider ADC4;
    public Slider ADC5;
    public Slider ADC6;
    public Slider ADC7;
    public Slider ADC8;
    public Slider ADC9;

    public Text telemetryText;

    Color triggered = new Color(22f / 255f, 233f / 255f, 11f / 255f);
    Color notTriggered = new Color(233f / 255f, 22f / 255f, 11f / 255f);


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ADC0.value = adc_val0;
        ADC1.value = adc_val1;
        ADC2.value = adc_val2;
        ADC3.value = adc_val3;
        ADC4.value = adc_val4;
        ADC5.value = adc_val5;
        ADC6.value = adc_val6;
        ADC7.value = adc_val7;
        ADC8.value = adc_val8;
        ADC9.value = adc_val9;

        telemetryText.text = $"Telemetry:\nBattery voltage: {batteryVoltage}V\nPos left: {posLeft}m\nPos right: {posRight}m\nADC0: {adc_val0}\nADC1: {adc_val1}\nADC2: {adc_val2}\nADC3: {adc_val3}\nADC4: {adc_val4}\nADC5: {adc_val5}\nADC6: {adc_val6}\nADC7: {adc_val7}\nADC8: {adc_val8}\nADC9: {adc_val9}" ;

        if ((triggers1 & 1) == 1)
        {
            ADC0.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = triggered;
        }
        else
        {
            ADC0.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = notTriggered;
        }

        if ((triggers1 & 2) == 2)
        {
            ADC1.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = triggered;
        }
        else
        {
            ADC1.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = notTriggered;
        }

        if ((triggers1 & 4) == 4)
        {
            ADC2.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = triggered;
        }
        else
        {
            ADC2.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = notTriggered;
        }

        if ((triggers1 & 8) == 8)
        {
            ADC3.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = triggered;
        }
        else
        {
            ADC3.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = notTriggered;
        }

        if ((triggers1 & 16) == 16)
        {
            ADC4.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = triggered;
        }
        else
        {
            ADC4.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = notTriggered;
        }

        if ((triggers1 & 32) == 32)
        {
            ADC5.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = triggered;
        }
        else
        {
            ADC5.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = notTriggered;
        }

        if ((triggers1 & 64) == 64)
        {
            ADC6.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = triggered;
        }
        else
        {
            ADC6.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = notTriggered;
        }

        if ((triggers1 & 128) == 128)
        {
            ADC7.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = triggered;
        }
        else
        {
            ADC7.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = notTriggered;
        }

        if ((triggers2 & 1) == 1)
        {
            ADC8.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = triggered;
        }
        else
        {
            ADC8.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = notTriggered;
        }

        if ((triggers2 & 2) == 2)
        {
            ADC9.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = triggered;
        }
        else
        {
            ADC9.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = notTriggered;
        }

    }
}
