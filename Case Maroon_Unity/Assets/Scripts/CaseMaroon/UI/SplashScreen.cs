using CaseMaroon.Backend;
using CaseMaroon.GameSystem;
using CaseMaroon.WorldMap;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CaseMaroon.Backend.BackendResponses;

namespace CaseMaroon.WorldMapUI
{
    public class SplashScreen : MonoBehaviour
    {
        public static SplashScreen Instance { get; private set; }
        public int OnQueueLeave { get; private set; }

        public Button LoginBtn;
        public Button FindGameBtn;
        public Button JoinGameBtn;

        public TMP_Text ErrorTxt;
        public TMP_InputField UsernameInput;

        public TMP_Text QueueCount;
        public TMP_Text StatusTxt;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Prevent duplicates
                return;
            }

            Instance = this;

            FindGameBtn.onClick.AddListener(FindGameClicked);
            JoinGameBtn.onClick.AddListener(JoinGameClicked);
            LoginBtn.onClick.AddListener(LoginBtnClicked);

        }

        private void Start()
        {
            BackendMessenger.Instance.OnPingResponse += OnPingResponse;
            BackendMessenger.Instance.OnLoginResponse += OnLoginResponse;
            BackendMessenger.Instance.OnQueueJoined += OnQueueJoined;
            BackendMessenger.Instance.OnQueueLeft += OnQueueLeft;

            BackendMessenger.Instance.OnQueueStatusChecked += OnQueueStatusChecked;

            BackendMessenger.Instance.OnMapConfigReceived += OnMapConfigReceived;

            BackendMessenger.Instance.OnNoiseHashValidated += OnNoiseHashValidated;

            BackendMessenger.Instance.StartPingPolling();

        }

        private void OnNoiseHashValidated(bool success, HashValidResponse response)
        {
            if(success)
            {
                BackendMessenger.Instance.StopPollingQueueStatus();
                BackendMessenger.Instance.StopPingPolling();

                GameManager.Instance.StartGameSequence();
            }
            else
            {
                MessageBox.Show("Invalid Map", "The map configuration is invalid or does not match the server's configuration. ");
            }
        }

        private void OnMapConfigReceived(bool success, BackendPayloads.MapConfig config)
        {
            if(success)
            {
                Worldmap.Instance.SetMapConfig(config);
                Worldmap.Instance.GenerateGrid();

                float clientHash = Worldmap.Instance.noiseGenerator.NoiseHash;

                BackendMessenger.Instance.ValidateNoiseHash(clientHash);

            }
        }

        private void OnQueueLeft(bool obj)
        {
            if (obj)
            {
                ChangeToFindGame();
                queueStatus = QueueStatus.InLobby;
            }
        }
        private void OnQueueStatusChecked(QueueStatusResponse response)
        {
            if(queueStatus == QueueStatus.InQueue)
            {
                if(response.gameFound == true)
                {
                    AuthManager.GameId = response.gameId;

                    BackendMessenger.Instance.GetMapConfig();
                }
            }

            if(response.success)
            {
                QueueCount.text = response.playersInQueue.ToString();
            }
        }
        private void OnQueueJoined(bool arg1, QueueJoinResponse response)
        {
            // CIRCLE that shows your in queue
            // show number in queue
            // begin poll

            if(arg1 == true)
            {
                ChangeToLeaveQueue();

                queueStatus = QueueStatus.InQueue;
            }
        }
        private void OnLoginResponse(bool status, LoginResponse loginResponse)
        {
            if(status)
            {
                ErrorTxt.gameObject.SetActive(false);
                AuthManager.SetToken(loginResponse);
                SetAuthStatus(AuthStatus.LoggedIn);
                ChangeToLogout();
            }
            else
            {
                ErrorTxt.text = loginResponse.message;
                ErrorTxt.gameObject.SetActive(true);
                SetAuthStatus(AuthStatus.Connected);
            }
        }

        private void OnPingResponse(bool status)
        {
            if(status)
            {
                if (authStatus == AuthStatus.Disconnected)
                {
                    SetAuthStatus(AuthStatus.Connected);
                    LoginBtn.gameObject.SetActive(true);
                }

                if(!BackendMessenger.Instance.IsPollingQueue)
                {
                    BackendMessenger.Instance.PollQueueStatus();
                }
            }
            else
            {
                SetAuthStatus(AuthStatus.Disconnected);
                LoginBtn.gameObject.SetActive(false);
            }
        }

        MessageBox messageBox;
        private void FindGameClicked()
        {
            if(authStatus == AuthStatus.LoggedIn && queueStatus == QueueStatus.InLobby)
            {
                BackendMessenger.Instance.JoinQueue();
            }
            else if (queueStatus == QueueStatus.InQueue)
            {
                BackendMessenger.Instance.LeaveQueue();
            }
            else
            {
                if(messageBox == null)
                {
                    messageBox = MessageBox.Show("Login First", "You must be logged in to join a game.");
                }
            }
        } 

        private void ChangeToLeaveQueue()
        {
            FindGameBtn.GetComponentInChildren<TMP_Text>().text = "Leave Queue";

            FindGameBtn.image.color = Color.red;
        }
        private void ChangeToFindGame()
        {
            FindGameBtn.GetComponentInChildren<TMP_Text>().text = "Find Game";
            FindGameBtn.image.color = Color.green;
        }
        private void JoinGameClicked()
        {
            BackendMessenger.Instance.UploadMapConfig(Worldmap.Instance);
        }
        private void LoginBtnClicked()
        {
            if(authStatus == AuthStatus.Connected)
            {
                string username = UsernameInput.text.Trim();

                if (!VerifyUsername(username))
                {
                    return; // Invalid username, do not proceed
                }

                BackendMessenger.Instance.Login(username);
            }
            else if (authStatus == AuthStatus.LoggedIn)
            {
                if(queueStatus == QueueStatus.InQueue)
                {
                    // this will leave the queue
                    FindGameClicked();
                }

                BackendMessenger.Instance.Logout(AuthManager.Username);
                BackendMessenger.Instance.OnLoggedOut += (obj) =>
                {
                    AuthManager.ClearAuth();
                    SetAuthStatus(AuthStatus.Connected);
                    ChangeToLogin();
                    BackendMessenger.Instance.StartPingPolling();
                };
            }
            else
            {
                ErrorTxt.text = "You must be connected to login.";
                ErrorTxt.gameObject.SetActive(true);
            }
        }

        private void ChangeToLogout()
        {
            LoginBtn.GetComponentInChildren<TMP_Text>().text = "Logout";
            LoginBtn.image.color = Color.red;
        }
        private void ChangeToLogin()
        {
            LoginBtn.GetComponentInChildren<TMP_Text>().text = "Login";
            LoginBtn.image.color = Color.white;
        }
        private bool VerifyUsername(string username)
        {
            // Regex: Starts with a letter, followed by 2-15 alphanumeric characters
            string pattern = @"^[A-Za-z][A-Za-z0-9]{2,15}$";

            if (!Regex.IsMatch(username, pattern))
            {
                ErrorTxt.text = "Username must start with a letter and contain only letters and numbers (3–16 chars).";
                ErrorTxt.gameObject.SetActive(true);
                return false;
            }

            ErrorTxt.gameObject.SetActive(false);

            ErrorTxt.text = ""; // Clear error if valid
            return true;
        }
        public void UpdateQueueCount(int count)
        {
            QueueCount.text = $"{count}";
        }

        private void PrepareForGameStart()
        {
            // reset ui back to defaults
            // stop polling queue status

            BackendMessenger.Instance.StopPollingQueueStatus();
            //ChangeToFindGame();

        }

        private enum AuthStatus { Disconnected, Connected, LoggedIn }
        private enum QueueStatus { InLobby, InQueue }


        private AuthStatus authStatus;
        private QueueStatus queueStatus;
        private void SetAuthStatus(AuthStatus status)
        {
            authStatus = status;

            switch (status)
            {
                case AuthStatus.Disconnected:
                    StatusTxt.text = "Disconnected";
                    StatusTxt.color = Color.red;
                    break;

                case AuthStatus.Connected:
                    StatusTxt.text = "Connected";
                    StatusTxt.color = Color.green;
                    break;

                case AuthStatus.LoggedIn:
                    StatusTxt.text = "Logged In as:\n" + AuthManager.Username;
                    StatusTxt.color = Color.green;
                    break;
            }
        }
    }
}
