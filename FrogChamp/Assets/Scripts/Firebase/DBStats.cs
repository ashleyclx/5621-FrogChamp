using Firebase.Firestore;

[FirestoreData]

public struct DBStats
{
    [FirestoreProperty]
    public string achievementStatus { get; set; }

    [FirestoreProperty]
    public float bestTime { get; set; }

    [FirestoreProperty]
    public int currFalls { get; set; }

    [FirestoreProperty]
    public int currJumps { get; set; }

    [FirestoreProperty]
    public float currtime { get; set; }

    [FirestoreProperty]
    public int numClears { get; set; }

    [FirestoreProperty]
    public int totalFalls { get; set; }

    [FirestoreProperty]
    public int totalJumps { get; set; }

    [FirestoreProperty]
    public string username { get; set; }

    [FirestoreProperty]
    public float xcampos { get; set; }

    [FirestoreProperty]
    public float ycampos { get; set; }

    [FirestoreProperty]
    public float zcampos { get; set; }

    [FirestoreProperty]
    public float xpos { get; set; }

    [FirestoreProperty]
    public float ypos { get; set; }
}
