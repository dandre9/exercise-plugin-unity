#if UNITY_IOS && !UNITY_EDITOR
using Newtonsoft.Json.Linq;
#endif

public static class ExerciseService
{

    public struct ExerciseData
    {
        public readonly double lat;
        public readonly double lon;
        public readonly double alt;
        public readonly double distance;
        public readonly int steps;

        public ExerciseData(double lat, double lon, double alt, double distance, int steps)
        {
            this.lat = lat;
            this.lon = lon;
            this.alt = alt;
            this.distance = distance;
            this.steps = steps;
        }
    }

    public enum ExercisePermission
    {
        GRANTED = 0,
        DENIED = 1,
        NEVER_ASKED = 2,
        DENIED_STEPS = 3,
        DENIED_LOCATION = 4,
        RESTRICTED_LOCATION = 5
    }

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string _getData();

    [DllImport("__Internal")]
    private static extern void _startService();

    [DllImport("__Internal")]
    private static extern void _stopService();

    [DllImport("__Internal")]
    private static extern string _getRouteCoords();

    [DllImport("__Internal")]
    private static extern string _hasPermission();

    [DllImport("__Internal")]
    private static extern void _requestPermission();

#elif UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass unityClass = new AndroidJavaClass(UnityDefaultJavaClassName);
    private static AndroidJavaObject unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
    private static AndroidJavaClass customClass = new AndroidJavaClass("com.radarstudio.toast.plugin.Bridge");
    private const string UnityDefaultJavaClassName = "com.unity3d.player.UnityPlayer";
    private const string CustomClassStartServiceMethod = "StartService";
    private const string CustomClassStopServiceMethod = "StopService";
    private const string CustomClassGetDataMethod = "GetData";
    private const string CustomClassHasPermission = "HasPermission";
    private const string CustomClassRequestPermission = "RequestPermission";
#endif    

    public static void StartService()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _startService();
#elif UNITY_ANDROID && !UNITY_EDITOR   
        customClass.CallStatic(CustomClassStartServiceMethod);
#endif
    }

    public static ExercisePermission HasPermission()
    {
        string permissionState = "";
        ExercisePermission permission;

#if UNITY_IOS && !UNITY_EDITOR
        permissionState = _hasPermission();
#elif UNITY_ANDROID && !UNITY_EDITOR
        permissionState = customClass.CallStatic<string>(CustomClassHasPermission, unityActivity);
#endif

        switch (permissionState)
        {
            case "granted":
                permission = ExercisePermission.GRANTED;
                break;
            case "denied":
                permission = ExercisePermission.DENIED;
                break;
            case "steps":
                permission = ExercisePermission.DENIED_STEPS;
                break;
            case "location":
                permission = ExercisePermission.DENIED_LOCATION;
                break;
            case "restrictedLocation":
                permission = ExercisePermission.RESTRICTED_LOCATION;
                break;
            case "neverAsked":
                permission = ExercisePermission.NEVER_ASKED;
                break;
            default:
                permission = ExercisePermission.DENIED;
                break;
        }

        return permission;
    }

    public static void RequestPermission()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _requestPermission();
#elif UNITY_ANDROID && !UNITY_EDITOR
        customClass.CallStatic(CustomClassRequestPermission);
#endif
    }

    public static void StopService()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _stopService();
#elif UNITY_ANDROID && !UNITY_EDITOR
        customClass.CallStatic(CustomClassStopServiceMethod);
#endif
    }

    public static ExerciseData GetData()
    {
        double[] data = { 0, 0, 0, 0, 0 };

#if UNITY_IOS && !UNITY_EDITOR
        string strData = _getData();
        JArray json = JArray.Parse(strData);
        data = new double[json.Count];
        int i = 0;

        foreach (double item in json)
        {
            data[i++] = item;
        }

#elif UNITY_ANDROID && !UNITY_EDITOR
        data = customClass.CallStatic<double[]>(CustomClassGetDataMethod);
#endif

        return new ExerciseData(data[0], data[1], data[2], data[3], (int)data[4]);
    }
}