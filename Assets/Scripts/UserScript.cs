using UnityEngine;
using TMPro;
using Mapbox.Unity.Map;

public class UserScript : MonoBehaviour
{
    [SerializeField] TMP_Text steps;
    [SerializeField] TMP_Text distance;
    [SerializeField] TMP_Text coords;
    [SerializeField] TMP_Text altitude;
    [SerializeField] TMP_Text serviceRunning;
    [SerializeField] AbstractMap map;

    ExerciseService.ExerciseData exerciseData;

    public void StartTraining()
    {
        ExerciseService.ExercisePermission permission = ExerciseService.HasPermission();

        if (true)
        {
            serviceRunning.text = "Ligado";
            serviceRunning.color = Color.green;

            ExerciseService.StartService();
            InvokeRepeating("UpdateExerciseData", 1, 5);
        }
        else if (permission == ExerciseService.ExercisePermission.NEVER_ASKED)
        {
            ExerciseService.RequestPermission();
        }
        else
        {
            if (permission == ExerciseService.ExercisePermission.DENIED)
                Debug.Log("É NECESSÁRIO TER AS DUAS PERMISSOES");
            else if (permission == ExerciseService.ExercisePermission.DENIED_LOCATION)
                Debug.Log("É NECESSÁRIO TER LOCALIZAÇÃO");
            else if (permission == ExerciseService.ExercisePermission.RESTRICTED_LOCATION)
                Debug.Log("É NECESSÁRIO HABILITAR LOCALIZAÇÃO SEMPRE");
            else if (permission == ExerciseService.ExercisePermission.DENIED_STEPS)
                Debug.Log("É NECESSÁRIO TER ACELEROMETRO");
        }
    }

    void UpdateExerciseData()
    {
        exerciseData = ExerciseService.GetData();

        steps.text = exerciseData.steps.ToString();
        distance.text = exerciseData.distance.ToString("N2") + " m";
        coords.text = exerciseData.lat.ToString("N7") + " , " + exerciseData.lon.ToString("N7");
        altitude.text = exerciseData.alt.ToString("N2") + " (altitude)";

        // map.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(exerciseData.lat, exerciseData.lon));
        // map.UpdateMap(14f);
    }

    public void StopTraning()
    {
        serviceRunning.text = "Desligado";
        serviceRunning.color = Color.red;

        ExerciseService.StopService();
    }

    public void PauseTraning()
    {
        serviceRunning.text = "Pausado";
        serviceRunning.color = Color.yellow;

        ExerciseService.PauseService();
    }

    public void OpenSettings()
    {
        ExerciseService.OpenAppSettings();
    }
}
