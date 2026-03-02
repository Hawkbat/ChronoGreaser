using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EngineInjectorController : MonoBehaviour
{
    [SerializeField] List<CargoType> currentContents = new();
    [SerializeField] List<CargoType> correctCombination = new();
    [SerializeField] TextMeshProUGUI injectorText;
    [SerializeField] SoundController injectSound;
    [SerializeField] SoundController correctSound;
    [SerializeField] SoundController incorrectSound;

    bool isInjecting = false;
    Stack<CargoSnapshot> cargoHistory = new();

    public bool CanInject => TimeLoop.IsPlaying && !isInjecting && currentContents.Count == correctCombination.Count;

    private void Update()
    {
        while (cargoHistory.Count > 0 && cargoHistory.Peek().time > TimeLoop.CurrentTime)
        {
            var snapshot = cargoHistory.Pop();
            if (snapshot.added)
            {
                currentContents.Remove(snapshot.cargoType);
            }
            else
            {
                currentContents.Add(snapshot.cargoType);
            }
        }

        var statusText = isInjecting ? "Injecting..." : CanInject ? "Ready" : $"Insert Materials ({currentContents.Count}/{correctCombination.Count})";
        var contentsText = currentContents.Count > 0 ? string.Join("\n", currentContents.Select(c => c.GetDisplayName())) : "Empty";
        injectorText.text = $"Injector Status:\n{statusText}\n\nContents:\n{contentsText}";
    }

    public void TryInject()
    {
        if (!CanInject) return;
        {
            isInjecting = true;

            injectSound.Play();

            var correct = currentContents.Count == correctCombination.Count && currentContents.TrueForAll(c => correctCombination.Contains(c));

            foreach (var cargoType in currentContents)
            {
                cargoHistory.Push(new CargoSnapshot {
                    time = TimeLoop.CurrentTime,
                    cargoType = cargoType,
                    added = false,
                });
            }

            currentContents.Clear();

            if (correct)
            {
                StartCoroutine(DoCorrectSequence());
            }
            else
            {
                StartCoroutine(DoIncorrectSequence());
            }
        }
    }

    public bool AddCargo(CargoType cargoType)
    {
        if (cargoType == CargoType.None || currentContents.Count >= correctCombination.Count) return false;
        currentContents.Add(cargoType);
        cargoHistory.Push(new CargoSnapshot {
            time = TimeLoop.CurrentTime,
            cargoType = cargoType,
            added = true,
        });
        return true;
    }

    IEnumerator DoCorrectSequence()
    {
        yield return new WaitForSeconds(1f);
        TimeLoop.SetTimeScale(1f);
        correctSound.Play();
        FadeController.Instance.StartFade(Color.black, 2f, false);
        yield return new WaitForSeconds(2f);
        isInjecting = false;
        SceneManager.LoadScene("Startup");
    }

    IEnumerator DoIncorrectSequence()
    {
        yield return new WaitForSeconds(1f);
        TimeLoop.SetTimeScale(1f);
        incorrectSound.Play();
        FadeController.Instance.StartFade(Color.white, 0.2f, false);
        yield return new WaitForSeconds(2f);
        isInjecting = false;
        TimeLoop.EmergencyRewind();
    }

    struct CargoSnapshot
    {
        public float time;
        public CargoType cargoType;
        public bool added;
    }
}
