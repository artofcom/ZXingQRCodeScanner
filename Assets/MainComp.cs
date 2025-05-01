using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainComp : MonoBehaviour
{
    [SerializeField] QRCodeScanner QRCodeScanner;
    [SerializeField] TMP_InputField InputField;
    [SerializeField] UnityTransportClient transportClient;

    string IP = "192.168.1.67";

    // Start is called before the first frame update
    void Start()
    {
        InputField.text = IP;
    }

    public void OnBtnConnect()
    {
        transportClient.Connect(InputField.text);
    }

    public void OnBtnSendQRCodeData()
    {
        if(string.IsNullOrEmpty(QRCodeScanner.QRCodeData))
            return;

        transportClient.Send(QRCodeScanner.QRCodeData);
    }
}
