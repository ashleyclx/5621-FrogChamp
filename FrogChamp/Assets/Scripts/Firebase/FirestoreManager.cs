using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;
using System.Linq;
using System.Threading.Tasks;

public class FirestoreManager : MonoBehaviour
{
    // Firestore variables
    [Header("Firestore")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    FirebaseFirestore DBreference;

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    // Completion Data Variables
    [Header("Statistics Data")]
    public TMP_Text bestTimingPlaceholder;
    public TMP_Text totalJumpsPlaceholder;
    public TMP_Text totalFallsPlaceholder;
    public TMP_Text numClearsPlaceholder;

    private float recordedBestTime = 0f;
    private int recordedTotalJumps = 0;
    private int recordedTotalFalls = 0;
    private int recordedNumClears = 0;

    // Leaderboard Data Variables
    [Header("Scoreboard Data")]
    public TMP_Text firstUsernamePlaceholder;
    public TMP_Text secondUsernamePlaceholder;
    public TMP_Text thirdUsernamePlaceholder;
    public TMP_Text firstTimePlaceholder;
    public TMP_Text secondTimePlaceholder;
    public TMP_Text thirdTimePlaceholder;

    // Achievement Data Variables
    [Header("Achievement Data")]
    public TMP_Text _1CT;
    public TMP_Text _2CT;
    public TMP_Text _3CT;
    public TMP_Text _4CT;

    void Awake()
    {
        // Checks if all of the necessary dependencies for Firebase are available
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // If avalible then Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        // Sets the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseFirestore.DefaultInstance;
    }

    public void ClearLoginFields()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }
    
    public void ClearRegisterFields()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }

    // Button for user login
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    // Button to allow new users to register
    public void RegisterButton()
    {
        // Calls the register function passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    // Button for users to sign out
    public void SignOutButton()
    {
        auth.SignOut();
        UIManager.instance.LoginScreen();
        ClearRegisterFields();
        ClearLoginFields();
    }

    // Button to open user stats screen
    public void StatisticsButton()
    {
        StartCoroutine(LoadStatsData());
    }

    // Button to open scoreboard screen
    public void ScoreboardButton()
    {
        StartCoroutine(LoadScoreboardData());
    }

    // Button to open achievement screen
    public void AchievementButton()
    {
        StartCoroutine(DisplayAchievement());
        UIManager.instance.Achievement1Screen();
    }

    // Button to start game
    public void StartGameButton()
    {
        StartCoroutine(LoadUserData());
    }
//here

    // Function to save progress (pause game)
    public void SaveProgress()
    {
        Vector2 currentPosition = StatsManager.instance.GetPosition();
        Vector3 currentCameraPosition = StatsManager.instance.GetCameraPosition();
        // Saves user stats to DB
        StartCoroutine(UpdateCurrJumps(StatsManager.instance.GetJumps()));
        StartCoroutine(UpdateCurrFalls(StatsManager.instance.GetFalls()));
        StartCoroutine(UpdateXPos(currentPosition.x));
        StartCoroutine(UpdateYPos(currentPosition.y));
        StartCoroutine(UpdateXCamPos(currentCameraPosition.x));
        StartCoroutine(UpdateYCamPos(currentCameraPosition.y));
        StartCoroutine(UpdateZCamPos(currentCameraPosition.z));
        StartCoroutine(UpdateCurrTime(TimeManager.instance.GetTime()));
    }

    // Function to update scoreboard (finished game)
    public void FinishProgress()
    {
        // Updates bestTime to database
        if (recordedBestTime == 0f)
            StartCoroutine(UpdateBestTime(TimeManager.instance.GetTime()));
        else
        {
            if (TimeManager.instance.GetTime() < recordedBestTime)
                StartCoroutine(UpdateBestTime(TimeManager.instance.GetTime()));
        }

        // Updates cummulative jumps to database
        if (recordedTotalJumps == 0)
            StartCoroutine(UpdateTotalJumps(StatsManager.instance.GetJumps()));
        else
            StartCoroutine(UpdateTotalJumps(recordedTotalJumps + StatsManager.instance.GetJumps()));

        // Updates cummulative falls to database
        if (recordedTotalFalls == 0)
            StartCoroutine(UpdateTotalFalls(StatsManager.instance.GetFalls()));
        else
            StartCoroutine(UpdateTotalFalls(recordedTotalFalls + StatsManager.instance.GetFalls()));

        // Updates number of clears
        StartCoroutine(UpdateNumClears(recordedNumClears + 1));
        
        // Resets all curr values to 0
        StartCoroutine(UpdateCurrJumps(0));
        StartCoroutine(UpdateCurrFalls(0));
        StartCoroutine(UpdateXPos(-6f));
        StartCoroutine(UpdateYPos(-3.5f));
        StartCoroutine(UpdateXCamPos(4.15f));
        StartCoroutine(UpdateYCamPos(3f));
        StartCoroutine(UpdateZCamPos(10f));
        StartCoroutine(UpdateCurrTime(0f));

        // Go to main menu screen
        UIManager.instance.MainMenuScreen();
    }

    // Function to save user's achievement status into DB
    public void SaveAchievement()
    {
        StartCoroutine(LoadAchievementData());
        StartCoroutine(UpdateAchievementStatus(AchievementManager.instance.GetAchievementStatus()));
    }

    // Function to display user's achievement status on achievement screen
    private IEnumerator DisplayAchievement()
    {
        List<string> achievementText = new List<string>();

        yield return StartCoroutine(LoadAchievementData());
        
        foreach (var achievementBool in AchievementManager.savedAchievement)
        {
            if (achievementBool == "true")
                achievementText.Add("completed");
            else
                achievementText.Add("not completed");
        }

        _1CT.text = achievementText[0];
        _2CT.text = achievementText[1];
        //_3CT.text = achievementText[2];
        //_4CT.text = achievementText[3];
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            // User is now logged in
            User = new FirebaseUser(LoginTask.Result.User);
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";

            // Opens the main menu screen after user is logged in
            UIManager.instance.MainMenuScreen(); 
            confirmLoginText.text = "";
            ClearLoginFields();
            ClearRegisterFields();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            // A warning is displayed if username input field is blank
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            // A warning is displayed if password and confirm password fields do not match
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            // Calls Firebase auth signin function passing the email and password
            Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);

            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                // User created
                User = new FirebaseUser(RegisterTask.Result.User);

                if (User != null)
                {
                    // Creates a user profile
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    // Call the Firebase auth update user profile function passing the profile with the username
                    Task ProfileTask = User.UpdateUserProfileAsync(profile);

                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        // Handles errors
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        // Username is now set

                        // Updates username to database
                        StartCoroutine(UpdateUsernameDatabase(_username));

                        // Returns to start screen
                        UIManager.instance.StartScreen();
                        warningRegisterText.text = "";
                        ClearRegisterFields();
                        ClearLoginFields();
                    }
                }
            }
        }
    }

    // Used to update username to database when account is created
    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        DBStats dbstats = new DBStats
        {
            achievementStatus =
        }
        //Set the currently logged in user username in the database
        Task DBTask = DBreference.Collection("users").Document(User.UserId).Document("username").SetAsync(_username);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    private IEnumerator UpdateCurrJumps(int _currJumps)
    {
        //Set the currently logged in user curr jumps
        Task DBTask = DBreference.Collection("users").Collection(User.UserId).Document("currJumps").SetAsync(_currJumps);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            // Updated number of curr jumps into database
        }
    }

    private IEnumerator UpdateCurrFalls(int _currFalls)
    {
        //Set the currently logged in user curr falls
        Task DBTask = DBreference.Collection("users").Collection(User.UserId).Document("currFalls").SetAsync(_currFalls);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            // Updated number of curr falls into database
        }
    }

    private IEnumerator UpdateXPos(float _xpos)
    {
        //Set the currently logged in user pos
        Task DBTask = DBreference.Collection("users").Collection(User.UserId).Document("xpos").SetAsync(_xpos);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            // Updated player's last x position into database
        }
    }

    private IEnumerator UpdateYPos(float _ypos)
    {
        //Set the currently logged in user pos
        Task DBTask = DBreference.Collection("users").Collection(User.UserId).Document("ypos").SetAsync(_ypos);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            // Updated player's last y position into database
        }
    }

    private IEnumerator UpdateXCamPos(float _xcampos)
    {
        //Set the currently logged in camera x pos
        Task DBTask = DBreference.Collection("users").Collection(User.UserId).Document("xcampos").SetAsync(_xcampos);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            // Updated camera's last x position into database
        }
    }

    private IEnumerator UpdateYCamPos(float _ycampos)
    {
        //Set the currently logged in camera y pos
        Task DBTask = DBreference.Collection("users").Collection(User.UserId).Document("ycampos").SetAsync(_ycampos);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            // Updated camera's last y position into database
        }
    }

    private IEnumerator UpdateZCamPos(float _zcampos)
    {
        //Set the currently logged in camera z pos
        Task DBTask = DBreference.Collection("users").Collection(User.UserId).Document("zcampos").SetAsync(_zcampos);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            // Updated camera's last z position into database
        }
    }

    private IEnumerator UpdateCurrTime(float _currtime)
    {
        // Sets the currently logged in user currtime (time before completion)
        Task DBTask = DBreference.Collection("users").Collection(User.UserId).Document("currtime").SetAsync(_currtime);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            // Updates currtime into database
        }
    }

    private IEnumerator UpdateBestTime(float _bestTime)
    {
        //Set the currently logged in user best time (time for completion)
        Task DBTask = DBreference.Collection("users").Collection(User.UserId).Document("bestTime").SetAsync(_bestTime);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            // Updated best clear time into database
        }
    }

    private IEnumerator UpdateTotalJumps(int _totalJumps)
    {
        //Set the currently logged in user total jumps
        Task DBTask = DBreference.Collection("users").Collection(User.UserId).Document("totalJumps").SetAsync(_totalJumps);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            // Updated number of total jumps into database
        }
    }

    private IEnumerator UpdateTotalFalls(int _totalFalls)
    {
        //Set the currently logged in user total falls
        Task DBTask = DBreference.Collection("users").Collection(User.UserId).Document("totalFalls").SetAsync(_totalFalls);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            // Updated number of total falls into database
        }
    }

    private IEnumerator UpdateNumClears(int _numClears)
    {
        //Set the currently logged in user number of clears
        Task DBTask = DBreference.Collection("users").Collection(User.UserId).Document("numClears").SetAsync(_numClears);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            // Updated number of clears into database
        }
    }

    private IEnumerator UpdateAchievementStatus(string _achievementStatus)
    {
        //Set the currently logged in user achievementStatus (boolean in string with delimiter " ")
        Task DBTask = DBreference.Collection("users").Collection(User.UserId).Document("achievementStatus").SetAsync(_achievementStatus);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            // Updated user achievementStatus into database
        }
    }

    private IEnumerator LoadUserData()
    {
        //Gets the currently logged in user data
        Task<DocumentSnapshot> DBTask = DBreference.Collection("users").Collection(User.UserId).GetSnapshotAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Data has been retrieved
            DocumentSnapshot snapshot = DBTask.Result;

            if (snapshot.Document("currtime").Value == null)
            {
                //No data exists yet (new game)
                TimeManager.instance.SetTime(0f);

                // Start game
                UIManager.instance.StartGame();
            }
            else
            {
                // Sets data for resume progress
                StatsManager.instance.SetPosition(float.Parse(snapshot.Document("xpos").Value.ToString()), float.Parse(snapshot.Document("ypos").Value.ToString()));
                StatsManager.instance.SetCameraPosition(float.Parse(snapshot.Document("xcampos").Value.ToString()), float.Parse(snapshot.Document("ycampos").Value.ToString()), float.Parse(snapshot.Document("zcampos").Value.ToString()));
                StatsManager.instance.SetJumps(int.Parse(snapshot.Document("currJumps").Value.ToString()));
                StatsManager.instance.SetFalls(int.Parse(snapshot.Document("currFalls").Value.ToString()));
                TimeManager.instance.SetTime(float.Parse(snapshot.Document("currtime").Value.ToString()));

                // Start game
                UIManager.instance.StartGame();
            }
        }
    }

    private IEnumerator LoadStatsData()
    {
        // Gets the currently logged in user data
        Task<DocumentSnapshot> DBTask = DBreference.Collection("users").Collection(User.UserId).GetSnapshotAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else 
        {
            //Data has been retrieved
            DocumentSnapshot snapshot = DBTask.Result;

            // Loads completion recorded data
            if (snapshot.Document("bestTime").Value != null)
            {
                recordedBestTime = float.Parse(snapshot.Document("bestTime").Value.ToString());
                recordedTotalJumps = int.Parse(snapshot.Document("totalJumps").Value.ToString());
                recordedTotalFalls = int.Parse(snapshot.Document("totalFalls").Value.ToString());
                recordedNumClears = int.Parse(snapshot.Document("numClears").Value.ToString());
            }
            
            // Sets text display for statistics screen
            bestTimingPlaceholder.text = recordedBestTime.ToString();
            totalJumpsPlaceholder.text = recordedTotalJumps.ToString();
            totalFallsPlaceholder.text = recordedTotalFalls.ToString();
            numClearsPlaceholder.text = recordedNumClears.ToString();

            // Go to statistics screen
            UIManager.instance.StatisticsScreen();
        }
    }

    private IEnumerator LoadAchievementData()
    {
        // Gets the currently logged in user data
        Task<DocumentSnapshot> DBTask = DBreference.Collection("users").Collection(User.UserId).GetSnapshotAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else 
        {
            string achievementStatus = "false false";
            //Data has been retrieved
            DocumentSnapshot snapshot = DBTask.Result;

            // Checks if user has achievement data stored (will not have data if new user)
            if (snapshot.Document("achievementStatus").Value != null)
            {
                achievementStatus = snapshot.Document("achievementStatus").Value.ToString();
            }

            string[] achievementBoolArray = achievementStatus.Split(' ');
            AchievementManager.savedAchievement = achievementBoolArray;
        }
    }

    private IEnumerator LoadScoreboardData()
    {
        //Get all the users data ordered by best clear time
        Task<DocumentSnapshot> DBTask = DBreference.Collection("users").OrderBy("bestTime").GetSnapshotAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Data has been retrieved
            DocumentSnapshot snapshot = DBTask.Result;

            int count = 0;

            //Loop through every users UID
            foreach (DocumentSnapshot childSnapshot in snapshot.Collection)
            {
                // Sets text display for first place player
                if (count == 0)
                {
                    // Checks if data is for new player with null bestTime
                    if (childSnapshot.Document("bestTime").Value == null)
                        continue;

                    string _displayUsername = childSnapshot.Document("username").Value.ToString();

                    if (_displayUsername.Length > 16)
                        firstUsernamePlaceholder.text = _displayUsername.Substring(0,16);
                    else
                        firstUsernamePlaceholder.text = _displayUsername;

                    firstTimePlaceholder.text = childSnapshot.Document("bestTime").Value.ToString().Substring(0,10);
                }

                // Sets text display for second place player
                if (count == 1)
                {
                    // Checks if data is for new player with null bestTime
                    if (childSnapshot.Document("bestTime").Value == null)
                        continue;

                    string _displayUsername = childSnapshot.Document("username").Value.ToString();

                    if (_displayUsername.Length > 16)
                        secondUsernamePlaceholder.text = _displayUsername.Substring(0,16);
                    else
                        secondUsernamePlaceholder.text = _displayUsername;

                    secondTimePlaceholder.text = childSnapshot.Document("bestTime").Value.ToString().Substring(0,10);
                }

                // Sets text display for third place player
                if (count == 2)
                {
                    // Checks if data is for new player with null bestTime
                    if (childSnapshot.Document("bestTime").Value == null)
                        continue;

                    string _displayUsername = childSnapshot.Document("username").Value.ToString();

                    if (_displayUsername.Length > 16)
                        thirdUsernamePlaceholder.text = _displayUsername.Substring(0,16);
                    else
                        thirdUsernamePlaceholder.text = _displayUsername;

                    thirdTimePlaceholder.text = childSnapshot.Document("bestTime").Value.ToString().Substring(0,10);
                }

                if (count == 3)
                {
                    break;
                }

                count++;
            }

            //Go to scoareboard screen
            UIManager.instance.ScoreboardScreen();
        }
    }
}