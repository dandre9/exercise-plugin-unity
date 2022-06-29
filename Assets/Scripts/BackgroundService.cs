using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
// using System.IO;

public class BackgroundService : MonoBehaviour
{
    [SerializeField] TMP_Text steps;
    [SerializeField] TMP_Text distance;
    [SerializeField] TMP_Text coords;
    [SerializeField] TMP_Text altitude;
    [SerializeField] TMP_Text serviceRunning;
    [SerializeField] LineRenderer route;
    [SerializeField] CanvasGroup canvasGroup;

    public int a = 43, b = 26;

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _ShowAlert(string title, string message);

    [DllImport("__Internal")]
    private static extern int _addTwoNumberInIOS(int a, int b);

#elif UNITY_ANDROID
    private AndroidJavaClass unityClass;
    private AndroidJavaObject unityActivity;
    private AndroidJavaClass customClass;
    private const string PlayerPrefsTotalSteps = "totalSteps";
    private const string PackageName = "com.radarstudio.toast.plugin.Bridge";
    private const string UnityDefaultJavaClassName = "com.unity3d.player.UnityPlayer";
    private const string CustomClassReceiveActivityInstanceMethod = "ReceiveActivityInstance";
    private const string CustomClassStartServiceMethod = "StartService";
    private const string CustomClassStopServiceMethod = "StopService";
    private const string CustomClassGetDataMethod = "GetData";
    private const string CustomClassGetServiceStateMethod = "GetServiceState";
    private const string CustomClassGetRouteCoordsMethod = "GetRouteCoords";
#endif

    int numOfDirections = 0;

    // static String[] fakeCoords = new String[]{
    //         "-19.9448475;-43.9534246;907.5",
    //         "-19.9448626;-43.9533769;907.5",
    //         "-19.9448694;-43.9533961;898",
    //         "-19.9448962;-43.953428;909.600036621094",
    //         "-19.9448777;-43.9534814;909.600036621094",
    //         "-19.9448598;-43.9535306;907.200012207031",
    //         "-19.9448522;-43.9535851;907.200012207031",
    //         "-19.9448045;-43.9535946;908.200012207031",
    //         "-19.9448185;-43.9536456;908.300048828125",
    //         "-19.9448322;-43.953711;914.700012207031",
    //         "-19.9448351;-43.9537601;914.700012207031",
    //         "-19.9448386;-43.9538127;914.5",
    //         "-19.9448415;-43.9538625;914.5",
    //         "-19.9448474;-43.9539143;915.800048828125",
    //         "-19.9448433;-43.9539706;914.700012207031",
    //         "-19.9448413;-43.9540263;914.700012207031",
    //         "-19.9448444;-43.9540797;914.700012207031",
    //         "-19.9448519;-43.9541593;914.5",
    //         "-19.9448905;-43.9541946;914.5",
    //         "-19.9449348;-43.9542161;914.900024414063",
    //         "-19.9449998;-43.9542077;914.900024414063",
    //         "-19.9450426;-43.9541914;914.900024414063",
    //         "-19.9450989;-43.9541681;915.600036621094",
    //         "-19.9451548;-43.9541638;915.600036621094",
    //         "-19.945171;-43.9541636;890.856323242188",
    //         "-19.9452272;-43.954166;893.31591796875",
    //         "-19.9452731;-43.9541822;920.600036621094",
    //         "-19.9453191;-43.9541898;920.600036621094",
    //         "-19.9453743;-43.9541831;920.600036621094",
    //         "-19.9454242;-43.9541798;918.200012207031",
    //         "-19.9454758;-43.9541739;918",
    //         "-19.9455317;-43.9541785;916.900024414063",
    //         "-19.9455897;-43.9541776;916.900024414063",
    //         "-19.9456342;-43.9541698;917.700012207031",
    //         "-19.9456801;-43.9541655;917.700012207031",
    //         "-19.9457369;-43.9541582;918.200012207031",
    //         "-19.9457718;-43.9541184;918.200012207031",
    //         "-19.9457953;-43.9540748;917.700012207031",
    //         "-19.9458391;-43.9540351;917.700012207031",
    //         "-19.9458717;-43.9540018;904.400024414063",
    //         "-19.9459176;-43.9539664;904.400024414063",
    //         "-19.9459621;-43.9539381;904.400024414063",
    //         "-19.9460053;-43.9539252;908.300048828125",
    //         "-19.9460538;-43.9538944;908.300048828125",
    //         "-19.9460984;-43.9538585;908.300048828125",
    //         "-19.9461441;-43.9538229;908.300048828125",
    //         "-19.9461836;-43.9537905;908.300048828125",
    //         "-19.9462207;-43.953742;908.300048828125",
    //         "-19.9462545;-43.9536975;908.300048828125",
    //         "-19.9462916;-43.9536611;908.300048828125",
    //         "-19.9463235;-43.9536194;908.300048828125",
    //         "-19.9463575;-43.9535855;908.300048828125",
    //         "-19.9463959;-43.9535526;908.300048828125",
    //         "-19.9464319;-43.953522;908.300048828125",
    //         "-19.9464721;-43.9534749;908.300048828125",
    //         "-19.9465033;-43.9534354;908.300048828125",
    //         "-19.9465037;-43.9533823;908.300048828125",
    //         "-19.9464759;-43.9533441;908.300048828125",
    //         "-19.9464112;-43.9533352;908.300048828125",
    //         "-19.9463668;-43.9533317;892.200012207031",
    //         "-19.9463215;-43.953316;892.200012207031",
    //         "-19.9462729;-43.9532893;892.100036621094",
    //         "-19.9462299;-43.9532619;892.100036621094",
    //         "-19.9461859;-43.9532225;892.200012207031",
    //         "-19.9461438;-43.9531919;892.200012207031",
    //         "-19.9461044;-43.9531657;891.900024414063",
    //         "-19.9460618;-43.9531382;891.900024414063",
    //         "-19.9460136;-43.9531164;892.100036621094",
    //         "-19.9459664;-43.9530921;892.100036621094",
    //         "-19.9459304;-43.9531274;892.100036621094",
    //         "-19.9458781;-43.953126;892.200012207031",
    //         "-19.9458184;-43.9531262;892.600036621094",
    //         "-19.9457656;-43.953123;892.600036621094",
    //         "-19.9457201;-43.9531185;892.600036621094",
    //         "-19.9456756;-43.9531369;892.600036621094",
    //         "-19.9456297;-43.9531435;892.600036621094",
    //         "-19.945578;-43.9531477;892.600036621094",
    //         "-19.9455446;-43.9531404;896.800048828125",
    //         "-19.9454915;-43.9531411;896.800048828125",
    //         "-19.9454457;-43.9531382;896.800048828125",
    //         "-19.9453883;-43.9531251;896.800048828125",
    //         "-19.9453391;-43.9531136;896.800048828125",
    //         "-19.9452864;-43.9531157;896.800048828125",
    //         "-19.9452281;-43.9531118;897.600036621094",
    //         "-19.9451827;-43.9531098;897.600036621094",
    //         "-19.9451307;-43.9531289;896.200012207031",
    //         "-19.9451055;-43.953181;896.200012207031",
    //         "-19.9450745;-43.9532268;903",
    //         "-19.9450178;-43.9532913;897.800048828125",
    //         "-19.9449756;-43.9533323;897.800048828125",
    //         "-19.9449343;-43.9533885;901.5",
    //         "-19.9449044;-43.9534271;901.5",
    //         "-19.9448777;-43.9534708;897.800048828125",
    //         "-19.9448522;-43.9535095;905.300048828125",
    //         "-19.9448402;-43.9535558;905.300048828125",
    //         "-19.9448672;-43.9535913;907.300048828125"
    // };

    private void Start()
    {
#if UNITY_IOS

#elif UNITY_ANDROID
        SendActivityReference(PackageName);
#endif
    }

    private void DrawRoute(List<Vector3> convertedCoords)
    {
        List<Vector3> filteredCoords = new List<Vector3>();

        filteredCoords = DouglasPeuckerReduction(convertedCoords, 5);
        route.positionCount = filteredCoords.Count;

        for (int i = 0; i < filteredCoords.Count; i++)
        {
            route.SetPosition(i, filteredCoords[i]);
        }
    }

    private void SendActivityReference(string packageName)
    {
#if UNITY_IOS

#elif UNITY_ANDROID
        unityClass = new AndroidJavaClass(UnityDefaultJavaClassName);
        unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
        customClass = new AndroidJavaClass(packageName);
        customClass.CallStatic(CustomClassReceiveActivityInstanceMethod, unityActivity);
#endif
    }

    public void StartService()
    {
#if UNITY_IOS
        int result = _addTwoNumberInIOS(a, b);
        _ShowAlert("O JOGO", result.ToString());
#elif UNITY_ANDROID
        serviceRunning.text = "Ligado";
        serviceRunning.color = Color.green;
        customClass.CallStatic(CustomClassStartServiceMethod);
        InvokeRepeating("SyncData", 1, 5);
#endif
    }

    public void StopService()
    {
#if UNITY_IOS

#elif UNITY_ANDROID
        serviceRunning.text = "Desligado";
        serviceRunning.color = Color.red;
        customClass.CallStatic(CustomClassStopServiceMethod);
        CancelInvoke();
#endif
    }

    public void SyncData()
    {
#if UNITY_IOS

#elif UNITY_ANDROID
        double[] data = customClass.CallStatic<double[]>(CustomClassGetDataMethod);

        steps.text = data[4].ToString("N0");
        distance.text = data[3] + " m";
        coords.text = data[0] + " , " + data[1];
        altitude.text = data[2].ToString("N2") + " (altitude)";
#endif
    }

    public void GetRouteCoords()
    {
#if UNITY_IOS

#elif UNITY_ANDROID
        canvasGroup.alpha = 0;
        string[] coordsString = customClass.CallStatic<string[]>(CustomClassGetRouteCoordsMethod);
        numOfDirections = coordsString.Length;

        float[,] coordsArray = new float[numOfDirections, 3];
        List<Vector3> convertedCoords = new List<Vector3>();

        for (int i = 0; i < numOfDirections; i++)
        {
            string[] temp = coordsString[i].Split(";");

            coordsArray[i, 0] = float.Parse(temp[0], CultureInfo.InvariantCulture);
            coordsArray[i, 1] = float.Parse(temp[1], CultureInfo.InvariantCulture);
            coordsArray[i, 2] = float.Parse(temp[2], CultureInfo.InvariantCulture);

            convertedCoords.Add(new Vector3((coordsArray[i, 0] - coordsArray[0, 0]) * 100000,
                                            (coordsArray[i, 1] - coordsArray[0, 1]) * 100000,
                                            (coordsArray[i, 2] - coordsArray[0, 2]) * 2));

            Debug.Log(convertedCoords[i]);
        }

        DrawRoute(convertedCoords);
#endif
    }

    public void ChangeRouteTransform(string symbol)
    {
        switch (symbol)
        {
            case "+":
                route.GetComponent<RectTransform>().localScale += new Vector3(0.1f, 0.1f, 0.1f);
                break;
            case "-":
                route.GetComponent<RectTransform>().localScale -= new Vector3(0.1f, 0.1f, 0.1f);
                break;
            case "l":
                route.GetComponent<RectTransform>().Rotate(new Vector3(0f, 5f, 0f), Space.World);
                break;
            case "r":
                route.GetComponent<RectTransform>().Rotate(new Vector3(0f, -5f, 0f), Space.World);
                break;
            case "u":
                route.GetComponent<RectTransform>().Rotate(new Vector3(5f, 0f, 0f), Space.World);
                break;
            case "d":
                route.GetComponent<RectTransform>().Rotate(new Vector3(-5f, 0f, 0f), Space.World);
                break;
            default:
                break;
        }
    }

    public static List<Vector3> DouglasPeuckerReduction
    (List<Vector3> Points, Double Tolerance)
    {
        if (Points == null || Points.Count < 3)
            return Points;

        Int32 firstPoint = 0;
        Int32 lastPoint = Points.Count - 1;
        List<Int32> pointIndexsToKeep = new List<Int32>();

        //Add the first and last index to the keepers
        pointIndexsToKeep.Add(firstPoint);
        pointIndexsToKeep.Add(lastPoint);

        //The first and the last point cannot be the same
        while (Points[firstPoint].Equals(Points[lastPoint]))
        {
            lastPoint--;
        }

        DouglasPeuckerReduction(Points, firstPoint, lastPoint,
        Tolerance, ref pointIndexsToKeep);

        List<Vector3> returnPoints = new List<Vector3>();
        pointIndexsToKeep.Sort();
        foreach (Int32 index in pointIndexsToKeep)
        {
            returnPoints.Add(Points[index]);
        }

        return returnPoints;
    }

    private static void DouglasPeuckerReduction(List<Vector3>
        points, Int32 firstPoint, Int32 lastPoint, Double tolerance,
        ref List<Int32> pointIndexsToKeep)
    {
        Double maxDistance = 0;
        Int32 indexFarthest = 0;

        for (Int32 index = firstPoint; index < lastPoint; index++)
        {
            Double distance = PerpendicularDistance
                (points[firstPoint], points[lastPoint], points[index]);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                indexFarthest = index;
            }
        }

        if (maxDistance > tolerance && indexFarthest != 0)
        {
            //Add the largest point that exceeds the tolerance
            pointIndexsToKeep.Add(indexFarthest);

            DouglasPeuckerReduction(points, firstPoint,
            indexFarthest, tolerance, ref pointIndexsToKeep);
            DouglasPeuckerReduction(points, indexFarthest,
            lastPoint, tolerance, ref pointIndexsToKeep);
        }
    }
    public static Double PerpendicularDistance(Vector3 Point1, Vector3 Point2, Vector3 Point)
    {
        Double area = Math.Abs(.5 * (Point1.x * Point2.y + Point2.x *
        Point.y + Point.x * Point1.y - Point2.x * Point1.y - Point.x *
        Point2.y - Point1.x * Point.y));
        Double bottom = Math.Sqrt(Math.Pow(Point1.x - Point2.x, 2) +
        Math.Pow(Point1.y - Point2.y, 2));
        Double height = area / bottom * 2;

        return height;
    }
}