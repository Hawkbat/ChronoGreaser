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

    public bool CanInject => TimeLoop.IsPlaying && !isInjecting && currentContents.Count > 0;

    private void Update()
    {
        var statusText = isInjecting ? "Injecting..." : CanInject ? "Ready" : "Insert Materials";
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

    public void AddCargo(CargoType cargoType)
    {
        if (cargoType == CargoType.None) return;
        currentContents.Add(cargoType);
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
        TimeLoop.SetTargetTime(0f);
    }
}
