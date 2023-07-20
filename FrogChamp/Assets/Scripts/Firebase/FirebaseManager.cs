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

    //User Data variables
    [Header("UserData")]
    public TMP_InputField usernameField;
    public TMP_InputField xpField;
    public TMP_InputField killsField;
    public TMP_InputField deathsField;
    public GameObject scoreElement;
    public Transform scoreboardContent;


    // Completion Data Variables
    [Header("Statistics Data")]
    public TMP_Text bestTimingPlaceholder;
    public TMP_Text totalJumpsPlaceholder;
    public TMP_Text totalFallsPlaceholder;

    private float recordedBestTime = 0f;
    private int recordedTotalJumps = 0;
    private int recordedTotalFalls = 0;

    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
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
        //Set the authentication instance object
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

    //Function for the login button
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    //Function for the register button
    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }
    //Function for the sign out button
    public void SignOutButton()
    {
        auth.SignOut();
        UIManager.instance.LoginScreen();
        ClearRegisterFields();
        ClearLoginFields();
    }
    //Function for the save button
    public void SaveDataButton()
    {
        //StartCoroutine(UpdateUsernameAuth(usernameField.text));
        //StartCoroutine(UpdateUsernameDatabase(usernameField.text));

        StartCoroutine(UpdateXp(int.Parse(xpField.text)));
        StartCoroutine(UpdateKills(int.Parse(killsField.text)));
        StartCoroutine(UpdateDeaths(int.Parse(deathsField.text)));
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

        // Resets all curr values to 0
        StartCoroutine(UpdateCurrJumps(0));
        StartCoroutine(UpdateCurrFalls(0));
        StartCoroutine(UpdateXPos(-6f));
        StartCoroutine(UpdateYPos(-3.5f));
        StartCoroutine(UpdateXCamPos(4.15f));
        StartCoroutine(UpdateYCamPos(3f));
        StartCoroutine(UpdateZCamPos(10f));
        StartCoroutine(UpdateCurrTime(0f));
    }

    //Function for the scoreboard button
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
            //User is now logged in
            //Now get the result
            // User = LoginTask.Result.User;
            User = new FirebaseUser(LoginTask.Result.User);
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";
            // StartCoroutine(LoadUserData());
            // yield return new WaitForSeconds(2);

            // usernameField.text = User.DisplayName;
            UIManager.instance.MainMenuScreen(); // Change to user data UI
            confirmLoginText.text = "";
            ClearLoginFields();
            ClearRegisterFields();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            // var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
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
                //User has now been created
                //Now get the result
                // User = RegisterTask.Result.User;
                User = new FirebaseUser(RegisterTask.Result.User);

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    // var ProfileTask = User.UpdateUserProfileAsync(profile);
                    Task ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to start screen
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
        //Create a user profile and set the username
        UserProfile profile = new UserProfile { DisplayName = _username };

        //Call the Firebase auth update user profile function passing the profile with the username
        // var ProfileTask = User.UpdateUserProfileAsync(profile);
        Task ProfileTask = User.UpdateUserProfileAsync(profile);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

        if (ProfileTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
        }
        else
        {
            //Auth username is now updated
        }
    }

    // This function is not in use (do not intend to allow username changes)
    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        //Set the currently logged in user username in the database
        // var DBTask = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);
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

    // Not in use
    private IEnumerator UpdateXp(int _xp)
    {
        //Set the currently logged in user xp
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("xp").SetValueAsync(_xp);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Xp is now updated
        }
    }

    // Not in use
    private IEnumerator UpdateKills(int _kills)
    {
        //Set the currently logged in user kills
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("kills").SetValueAsync(_kills);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Kills are now updated
        }
    }

    // Not in use
    private IEnumerator UpdateDeaths(int _deaths)
    {
        //Set the currently logged in user deaths
        // var DBTask = DBreference.Child("users").Child(User.UserId).Child("deaths").SetValueAsync(_deaths);
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("deaths").SetValueAsync(_deaths);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Deaths are now updated
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
        //Set the currently logged in user currtime (time before completion)
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

    private IEnumerator LoadUserData()
    {
        //Get the currently logged in user data
        // var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();
        Task<DataSnapshot> DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet

            bestTimingPlaceholder.text = recordedBestTime.ToString();
            totalJumpsPlaceholder.text = recordedTotalJumps.ToString();
            totalFallsPlaceholder.text = recordedTotalFalls.ToString();

            //xpField.text = "0";
            //killsField.text = "0";
            //deathsField.text = "0";
            TimeManager.instance.SetTime(0f);

            // Start game
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            //xpField.text = snapshot.Child("xp").Value.ToString();
            //killsField.text = snapshot.Child("kills").Value.ToString();
            //deathsField.text = snapshot.Child("deaths").Value.ToString();

            // Sets data for resume progress
            StatsManager.instance.SetPosition(float.Parse(snapshot.Child("xpos").Value.ToString()), float.Parse(snapshot.Child("ypos").Value.ToString()));
            StatsManager.instance.SetCameraPosition(float.Parse(snapshot.Child("xcampos").Value.ToString()), float.Parse(snapshot.Child("ycampos").Value.ToString()), float.Parse(snapshot.Child("zcampos").Value.ToString()));
            StatsManager.instance.SetJumps(int.Parse(snapshot.Child("currJumps").Value.ToString()));
            StatsManager.instance.SetFalls(int.Parse(snapshot.Child("currFalls").Value.ToString()));
            TimeManager.instance.SetTime(float.Parse(snapshot.Child("currtime").Value.ToString()));

            // Loads completion recorded data
            if (snapshot.Child("bestTime").Value != null)
            {
                recordedBestTime = float.Parse(snapshot.Child("bestTime").Value.ToString());
                recordedTotalJumps = int.Parse(snapshot.Child("totalJumps").Value.ToString());
                recordedTotalFalls = int.Parse(snapshot.Child("totalFalls").Value.ToString());
            }
            
            bestTimingPlaceholder.text = recordedBestTime.ToString();
            totalJumpsPlaceholder.text = recordedTotalJumps.ToString();
            totalFallsPlaceholder.text = recordedTotalFalls.ToString();

            // Start game
        }
    }

    private IEnumerator LoadScoreboardData()
    {
        //Get all the users data ordered by kills amount
        // var DBTask = DBreference.Child("users").OrderByChild("kills").GetValueAsync();
        Task<DataSnapshot> DBTask = DBreference.Child("users").OrderByChild("kills").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            //Destroy any existing scoreboard elements
            foreach (Transform child in scoreboardContent.transform)
            {
                Destroy(child.gameObject);
            }

            //Loop through every users UID
            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                string username = childSnapshot.Child("username").Value.ToString();
                int kills = int.Parse(childSnapshot.Child("kills").Value.ToString());
                int deaths = int.Parse(childSnapshot.Child("deaths").Value.ToString());
                int xp = int.Parse(childSnapshot.Child("xp").Value.ToString());

                //Instantiate new scoreboard elements
                GameObject scoreboardElement = Instantiate(scoreElement, scoreboardContent);
                // scoreboardElement.GetComponent<ScoreElement>().NewScoreElement(username, kills, deaths, xp); commented out to prevent compilation errors
            }

            //Go to scoareboard screen
            UIManager.instance.LeaderboardScreen();
        }
    }
}