using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Linq;
using System.Threading.Tasks;

public class FirebaseManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference DBreference;

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
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
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

    public void StartGameButton()
    {
        StartCoroutine(LoadUserData());
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        //var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
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

    // This function is not in use (do not intend to allow username changes)
    private IEnumerator UpdateUsernameAuth(string _username)
    {
        // Creates a user profile and set the username
        UserProfile profile = new UserProfile { DisplayName = _username };

        // Calls the Firebase auth update user profile function passing the profile with the username
        Task ProfileTask = User.UpdateUserProfileAsync(profile);

        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

        if (ProfileTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
        }
        else
        {
            // Auth username is now updated
        }
    }

    // Used to update username to database when account is created
    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        //Set the currently logged in user username in the database
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);

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
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("currJumps").SetValueAsync(_currJumps);

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
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("currFalls").SetValueAsync(_currFalls);

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
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("xpos").SetValueAsync(_xpos);

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
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("ypos").SetValueAsync(_ypos);

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
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("xcampos").SetValueAsync(_xcampos);

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
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("ycampos").SetValueAsync(_ycampos);

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
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("zcampos").SetValueAsync(_zcampos);

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
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("currtime").SetValueAsync(_currtime);

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
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("bestTime").SetValueAsync(_bestTime);

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
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("totalJumps").SetValueAsync(_totalJumps);

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
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("totalFalls").SetValueAsync(_totalFalls);

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
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("numClears").SetValueAsync(_numClears);

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

    private IEnumerator LoadUserData()
    {
        //Gets the currently logged in user data
        Task<DataSnapshot> DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            if (snapshot.Child("currtime").Value == null)
            {
                //No data exists yet (new game)
                TimeManager.instance.SetTime(0f);

                // Start game
                UIManager.instance.StartGame();
            }
            else
            {
                // Sets data for resume progress
                StatsManager.instance.SetPosition(float.Parse(snapshot.Child("xpos").Value.ToString()), float.Parse(snapshot.Child("ypos").Value.ToString()));
                StatsManager.instance.SetCameraPosition(float.Parse(snapshot.Child("xcampos").Value.ToString()), float.Parse(snapshot.Child("ycampos").Value.ToString()), float.Parse(snapshot.Child("zcampos").Value.ToString()));
                StatsManager.instance.SetJumps(int.Parse(snapshot.Child("currJumps").Value.ToString()));
                StatsManager.instance.SetFalls(int.Parse(snapshot.Child("currFalls").Value.ToString()));
                TimeManager.instance.SetTime(float.Parse(snapshot.Child("currtime").Value.ToString()));

                // Start game
                UIManager.instance.StartGame();
            }
        }
    }

    private IEnumerator LoadStatsData()
    {
        // Gets the currently logged in user data
        Task<DataSnapshot> DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else 
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            // Loads completion recorded data
            if (snapshot.Child("bestTime").Value != null)
            {
                recordedBestTime = float.Parse(snapshot.Child("bestTime").Value.ToString());
                recordedTotalJumps = int.Parse(snapshot.Child("totalJumps").Value.ToString());
                recordedTotalFalls = int.Parse(snapshot.Child("totalFalls").Value.ToString());
                recordedNumClears = int.Parse(snapshot.Child("numClears").Value.ToString());
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

    private IEnumerator LoadScoreboardData()
    {
        //Get all the users data ordered by best clear time
        Task<DataSnapshot> DBTask = DBreference.Child("users").OrderByChild("bestTime").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            int count = 0;

            //Loop through every users UID
            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                // Sets text display for first place player
                if (count == 0)
                {
                    firstUsernamePlaceholder.text = childSnapshot.Child("username").Value.ToString();
                    firstTimePlaceholder.text = childSnapshot.Child("bestTime").Value.ToString().Substring(0,10);
                }

                // Sets text display for second place player
                if (count == 1)
                {
                    secondUsernamePlaceholder.text = childSnapshot.Child("username").Value.ToString();
                    secondTimePlaceholder.text = childSnapshot.Child("bestTime").Value.ToString().Substring(0,10);
                }

                // Sets text display for third place player
                if (count == 2)
                {
                    thirdUsernamePlaceholder.text = childSnapshot.Child("username").Value.ToString();
                    thirdTimePlaceholder.text = childSnapshot.Child("bestTime").Value.ToString().Substring(0,10);
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