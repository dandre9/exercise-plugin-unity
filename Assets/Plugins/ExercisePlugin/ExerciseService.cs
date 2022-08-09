#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
#endif
using System;
using System.Globalization;
using UnityEngine;

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
    private static extern void _resetData();

    [DllImport("__Internal")]
    private static extern string _getRouteCoords();

    [DllImport("__Internal")]
    private static extern string _hasPermission();

    [DllImport("__Internal")]
    private static extern void _requestPermission();

    [DllImport("__Internal")]
    public static extern void _openSettings();

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
    private const string CustomClassOpenAppSettings = "OpenAppSettings";
    private const string CustomClassIsServiceRunning = "IsServiceRunning";
    private const string CustomClassResetData = "ResetData";
    private const string CustomClassGetRouteCoordsMethod = "GetRouteCoords";
#endif

    static string[] fakeCoords = new string[]{
            "-19.9448475;-43.9534246;907.5",
            "-19.9448626;-43.9533769;907.5",
            "-19.9448694;-43.9533961;898",
            "-19.9448962;-43.953428;909.600036621094",
            "-19.9448777;-43.9534814;909.600036621094",
            "-19.9448598;-43.9535306;907.200012207031",
            "-19.9448522;-43.9535851;907.200012207031",
            "-19.9448045;-43.9535946;908.200012207031",
            "-19.9448185;-43.9536456;908.300048828125",
            "-19.9448322;-43.953711;914.700012207031",
            "-19.9448351;-43.9537601;914.700012207031",
            "-19.9448386;-43.9538127;914.5",
            "-19.9448415;-43.9538625;914.5",
            "-19.9448474;-43.9539143;915.800048828125",
            "-19.9448433;-43.9539706;914.700012207031",
            "-19.9448413;-43.9540263;914.700012207031",
            "-19.9448444;-43.9540797;914.700012207031",
            "-19.9448519;-43.9541593;914.5",
            "-19.9448905;-43.9541946;914.5",
            "-19.9449348;-43.9542161;914.900024414063",
            "-19.9449998;-43.9542077;914.900024414063",
            "-19.9450426;-43.9541914;914.900024414063",
            "-19.9450989;-43.9541681;915.600036621094",
            "-19.9451548;-43.9541638;915.600036621094",
            "-19.945171;-43.9541636;890.856323242188",
            "-19.9452272;-43.954166;893.31591796875",
            "-19.9452731;-43.9541822;920.600036621094",
            "-19.9453191;-43.9541898;920.600036621094",
            "-19.9453743;-43.9541831;920.600036621094",
            "-19.9454242;-43.9541798;918.200012207031",
            "-19.9454758;-43.9541739;918",
            "-19.9455317;-43.9541785;916.900024414063",
            "-19.9455897;-43.9541776;916.900024414063",
            "-19.9456342;-43.9541698;917.700012207031",
            "-19.9456801;-43.9541655;917.700012207031",
            "-19.9457369;-43.9541582;918.200012207031",
            "-19.9457718;-43.9541184;918.200012207031",
            "-19.9457953;-43.9540748;917.700012207031",
            "-19.9458391;-43.9540351;917.700012207031",
            "-19.9458717;-43.9540018;904.400024414063",
            "-19.9459176;-43.9539664;904.400024414063",
            "-19.9459621;-43.9539381;904.400024414063",
            "-19.9460053;-43.9539252;908.300048828125",
            "-19.9460538;-43.9538944;908.300048828125",
            "-19.9460984;-43.9538585;908.300048828125",
            "-19.9461441;-43.9538229;908.300048828125",
            "-19.9461836;-43.9537905;908.300048828125",
            "-19.9462207;-43.953742;908.300048828125",
            "-19.9462545;-43.9536975;908.300048828125",
            "-19.9462916;-43.9536611;908.300048828125",
            "-19.9463235;-43.9536194;908.300048828125",
            "-19.9463575;-43.9535855;908.300048828125",
            "-19.9463959;-43.9535526;908.300048828125",
            "-19.9464319;-43.953522;908.300048828125",
            "-19.9464721;-43.9534749;908.300048828125",
            "-19.9465033;-43.9534354;908.300048828125",
            "-19.9465037;-43.9533823;908.300048828125",
            "-19.9464759;-43.9533441;908.300048828125",
            "-19.9464112;-43.9533352;908.300048828125",
            "-19.9463668;-43.9533317;892.200012207031",
            "-19.9463215;-43.953316;892.200012207031",
            "-19.9462729;-43.9532893;892.100036621094",
            "-19.9462299;-43.9532619;892.100036621094",
            "-19.9461859;-43.9532225;892.200012207031",
            "-19.9461438;-43.9531919;892.200012207031",
            "-19.9461044;-43.9531657;891.900024414063",
            "-19.9460618;-43.9531382;891.900024414063",
            "-19.9460136;-43.9531164;892.100036621094",
            "-19.9459664;-43.9530921;892.100036621094",
            "-19.9459304;-43.9531274;892.100036621094",
            "-19.9458781;-43.953126;892.200012207031",
            "-19.9458184;-43.9531262;892.600036621094",
            "-19.9457656;-43.953123;892.600036621094",
            "-19.9457201;-43.9531185;892.600036621094",
            "-19.9456756;-43.9531369;892.600036621094",
            "-19.9456297;-43.9531435;892.600036621094",
            "-19.945578;-43.9531477;892.600036621094",
            "-19.9455446;-43.9531404;896.800048828125",
            "-19.9454915;-43.9531411;896.800048828125",
            "-19.9454457;-43.9531382;896.800048828125",
            "-19.9453883;-43.9531251;896.800048828125",
            "-19.9453391;-43.9531136;896.800048828125",
            "-19.9452864;-43.9531157;896.800048828125",
            "-19.9452281;-43.9531118;897.600036621094",
            "-19.9451827;-43.9531098;897.600036621094",
            "-19.9451307;-43.9531289;896.200012207031",
            "-19.9451055;-43.953181;896.200012207031",
            "-19.9450745;-43.9532268;903",
            "-19.9450178;-43.9532913;897.800048828125",
            "-19.9449756;-43.9533323;897.800048828125",
            "-19.9449343;-43.9533885;901.5",
            "-19.9449044;-43.9534271;901.5",
            "-19.9448777;-43.9534708;897.800048828125",
            "-19.9448522;-43.9535095;905.300048828125",
            "-19.9448402;-43.9535558;905.300048828125",
            "-19.9448672;-43.9535913;907.300048828125"
    };

    const int MAX_COORDS_NUMBER = 25;

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

    public static void PauseService()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _stopService();
#elif UNITY_ANDROID && !UNITY_EDITOR
        if(customClass.CallStatic<bool>(CustomClassIsServiceRunning))        
            customClass.CallStatic(CustomClassStopServiceMethod);            
#endif
    }

    public static void StopService()
    {
        PauseService();
#if UNITY_IOS && !UNITY_EDITOR
        _resetData();
#elif UNITY_ANDROID && !UNITY_EDITOR        
        customClass.CallStatic(CustomClassResetData);   
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

    public static double[,] GetRouteCoords()
    {
        string[] coordsString = new string[0];
        int numOfDirections = 0;
        int indexOffset = 1;

#if UNITY_IOS && !UNITY_EDITOR
        string strData = _getRouteCoords();
        JArray json = JArray.Parse(strData);
        coordsString = new string[json.Count];
        int index = 0;

        foreach (string item in json)
        {
            coordsString[index++] = item;
        }

        numOfDirections = coordsString.Length;

#elif UNITY_ANDROID && !UNITY_EDITOR
        coordsString = customClass.CallStatic<string[]>(CustomClassGetRouteCoordsMethod);
        numOfDirections = coordsString.Length;
#endif        

        Debug.Log("QUANTIDADE DE COORDENADAS: " + numOfDirections);

        if (numOfDirections > MAX_COORDS_NUMBER)
        {
            indexOffset = (int)Math.Ceiling((float)numOfDirections / MAX_COORDS_NUMBER);
        }

        double[,] coordsArray = new double[numOfDirections / indexOffset, 2];

        for (int i = 0; i < coordsArray.Length / 2; i++)
        {
            string[] temp = coordsString[i * indexOffset].Split(";");

            if (i == coordsArray.Length / 2 - 1)
                temp = coordsString[coordsString.Length - 1].Split(";");

            coordsArray[i, 0] = float.Parse(temp[0], CultureInfo.InvariantCulture);
            coordsArray[i, 1] = float.Parse(temp[1], CultureInfo.InvariantCulture);
            // coordsArray[i, 2] = float.Parse(temp[2], CultureInfo.InvariantCulture);
        }

        return coordsArray;
    }

    public static void OpenAppSettings()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _openSettings();
#elif UNITY_ANDROID && !UNITY_EDITOR
        customClass.CallStatic(CustomClassOpenAppSettings);
#endif
    }
}