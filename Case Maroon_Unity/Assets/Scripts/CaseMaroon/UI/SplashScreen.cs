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

            BackendTester.Instance.OnPingResponse += OnPingResponse;
            BackendTester.Instance.OnLoginResponse += OnLoginResponse;
            BackendTester.Instance.OnQueueJoined += OnQueueJoined;
            BackendTester.Instance.OnQueueLeft += OnQueueLeft;

            BackendTester.Instance.OnQueueStatusChecked += OnQueueStatusChecked;

            BackendTester.Instance.PingServer();
        }

        private void OnQueueLeft(bool obj)
        {
            if (obj)
            {
                ChangeToFindGame();
                queueStatus = QueueStatus.InLobby;
                BackendTester.Instance.StopPollingQueueStatus();
                QueueCount.text = "NA";
            }
        }

        private void OnQueueStatusChecked(QueueStatusResponse response)
        {
            if(queueStatus == QueueStatus.InQueue)
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
                BackendTester.Instance.PollQueueStatus();
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
            AuthStatus st = (status) ? AuthStatus.Connected : AuthStatus.Disconnected;

            SetAuthStatus(st);
            LoginBtn.gameObject.SetActive(true);
        }


        MessageBox messageBox;
        private void FindGameClicked()
        {
            if(authStatus == AuthStatus.LoggedIn && queueStatus == QueueStatus.InLobby)
            {
                BackendTester.Instance.JoinQueue();
            }
            else if (queueStatus == QueueStatus.InQueue)
            {
                BackendTester.Instance.LeaveQueue();
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
            BackendTester.Instance.UploadMapConfig(Worldmap.Instance);
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

                BackendTester.Instance.Login(username);
            }
            else if (authStatus == AuthStatus.LoggedIn)
            {
                BackendTester.Instance.Logout(AuthManager.Username);
                BackendTester.Instance.OnLoggedOut += (obj) =>
                {
                    AuthManager.ClearAuth();
                    SetAuthStatus(AuthStatus.Connected);
                    ChangeToLogin();
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
