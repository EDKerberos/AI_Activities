using Unity.Netcode;
using Unity.Netcode.Transports.UTP;  
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;   
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Core;                
using Unity.Services.Authentication;      
using System.Threading.Tasks;             
using UnityEngine;
using TMPro;                              

public class MultiplayerMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuUI;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_Text statusText;

    //private const string WebGLConnectionType = "wss";
    private const string ConnectionType = "udp";

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            statusText.text = "Signed in locally. Ready to connect.";
        }
    }

    public async void StartHost()
    {
        try
        {
            statusText.text = "Creating Relay Session...";

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerData relayData = new RelayServerData(allocation, "udp");

            transport.SetRelayServerData(relayData);

            bool started = NetworkManager.Singleton.StartHost();

            if (started)
            {
                joinCodeText.text = "Join Code: " + joinCode;
                statusText.text = "Host started successfully!";

                //if (menuUI != null)
                    //menuUI.SetActive(false);
            }
            else
            {
                statusText.text = "Host failed to start.";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            statusText.text = "Failed to start Host.";
        }
    }

    public async void StartClient()
    {
        try
        {
            statusText.text = "Joining Session...";

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCodeInput.text);

            if (string.IsNullOrWhiteSpace(joinCodeInput.text))
            {
                statusText.text = "Input a join code!";
                return;
            }

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerData relayData = new RelayServerData(joinAllocation, ConnectionType);

            //transport.UseWebSockets = true;
            transport.SetRelayServerData(relayData);

            NetworkManager.Singleton.StartClient();

            if (menuUI != null) 
                menuUI.SetActive(false);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
            statusText.text = "Invalid Join Code.";
        }
    }
}

/*public class MultiplayerMenu : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }
}*/
