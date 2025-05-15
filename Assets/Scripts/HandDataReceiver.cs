using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class HandDataReceiver : MonoBehaviour
{
    public static HandDataReceiver instance;
    private TcpListener listener;
    private TcpClient client;
    private StreamReader reader;

    public bool IsGameStarted;
    
    private CharacterInputController characterInputController;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        
        Debug.Log("Bắt đầu lắng nghe kết nối từ Python...");
        listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5005);
        listener.Start();
        listener.BeginAcceptTcpClient(AcceptCallback, null);
        IsGameStarted = false;
        
        StartPythonScript();
    }

    void AcceptCallback(IAsyncResult ar)
    {
        client = listener.EndAcceptTcpClient(ar);
        reader = new StreamReader(client.GetStream());
        Debug.Log("Đã kết nối từ Input.");
    }

    void Update()
    {
        if (IsGameStarted && characterInputController == null)
        {
            characterInputController = FindFirstObjectByType<CharacterInputController>();
        }
        if (client != null && client.Available > 0)
        {
            string json = reader.ReadLine();
            if (!string.IsNullOrEmpty(json) && characterInputController != null)
            {
                int input = int.Parse(json);
                Debug.Log(input);
                if ( input == 2 && characterInputController.TutorialMoveCheck(0))
                {
                    characterInputController.ChangeLane(-1);
                }
                else if(input == 1 && characterInputController.TutorialMoveCheck(0))
                {
                    characterInputController.ChangeLane(1);
                }
                // else if(input == 4 && characterInputController.TutorialMoveCheck(1))
                // {
                //     characterInputController.Jump();
                // }
                // else if (input == 3 && characterInputController.TutorialMoveCheck(2))
                // {
                //     if(!characterInputController.m_Sliding)
                //         characterInputController.Slide();
                // }
            }
        }
    }

    void OnApplicationQuit()
    {
        reader?.Close();
        client?.Close();
        listener?.Stop();
    }
    
    void StartPythonScript()
    {
        string exePath;
#if UNITY_EDITOR
        // Khi chạy trong Editor: dùng đường dẫn trực tiếp trong Assets
        exePath = Path.Combine(Application.dataPath, "PyInput/dist/input.exe");
#else
        // Khi chạy bản build: dùng thư mục StreamingAssets
        exePath = Path.Combine(Application.streamingAssetsPath, "input.exe");
#endif
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = exePath; 
        start.UseShellExecute = false;
        start.CreateNoWindow = false; 
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;

        try
        {
            Process process = new Process();
            process.StartInfo = start;
            process.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log("PYTHON OUT: " + args.Data);
            process.ErrorDataReceived += (sender, args) => UnityEngine.Debug.LogError("PYTHON ERR: " + args.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("Không thể chạy input.exe: " + ex.Message);
        }
    }


}
