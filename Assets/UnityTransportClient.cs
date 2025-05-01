using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine;
using System.Collections;

public class UnityTransportClient : MonoBehaviour
{
    private NetworkDriver driver = default;
    private NetworkConnection connection = default;

    [SerializeField] float _updateInterval = 1.0f;

    bool isConnected = false;

    // Start is called before the first frame update
    private void Awake()
    {
        driver = NetworkDriver.Create(); 
        StartCoroutine(coUpdate());
    }

    // Update is called once per frame
    IEnumerator coUpdate()
    {
        while(true)
        {
            driver.ScheduleUpdate().Complete();
            if (connection.IsCreated)
            {
                NetworkEvent.Type cmd;
                while ((cmd = connection.PopEvent(driver, out var reader)) != NetworkEvent.Type.Empty)
                {
                    switch(cmd)
                    {
                    case NetworkEvent.Type.Connect:
                        Debug.Log("Connected to server.");
                        break;
                    case NetworkEvent.Type.Disconnect:
                        Debug.Log("Disconnected from server");
                        connection = default;
                        break;
                    case NetworkEvent.Type.Data:
                        FixedString128Bytes msg = reader.ReadFixedString128();
                        Debug.Log($"Received: {msg}");
                        break;
                    default:
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(_updateInterval);
        }
    }

    public void Connect(string IP, int port = 9000)
    {
        if(isConnected)
            Disconnect();

        var endpoint = NetworkEndpoint.Parse(IP, (ushort)port);
        connection = driver.Connect(endpoint);
        isConnected = true;
        Debug.Log("Try connecting....");
    }

    public void Send(string strData)
    {
        if(connection == default)
        {
            Debug.LogError("Connect required...");
            return;
        }

        Debug.Log("Try Sending Data....");
        var status = driver.BeginSend(connection, out var writer);
        if (status == 0)
        {
            writer.WriteFixedString128(strData);
            driver.EndSend(writer);
        }
        else
        {
            Debug.LogError($"BeginSend failed with status code {status}");
        }
    }

    public void Disconnect()
    {
        if(connection == default)
            return;

        driver.Disconnect(connection);
        connection = default;
        isConnected = false;

        Debug.Log("Disconnecting....");
    }

    void OnDestroy()
    {   
        Disconnect();

        driver.Dispose();
        Debug.Log("Destroying...");
    }
}
