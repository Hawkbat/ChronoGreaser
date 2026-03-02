using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EngineInjectorController : MonoBehaviour
{
    [SerializeField] InjectorMode mode;
    [SerializeField] List<CargoType> currentContents = new();
    [SerializeField] List<CargoType> correctCombination = new();
    [SerializeField] CargoType shieldType;
    [SerializeField] TextMeshProUGUI injectorText;
    [SerializeField] SoundController injectSound;
    [SerializeField] SoundController correctSound;
    [SerializeField] SoundController incorrectSound;

    bool isInjecting = false;
    Stack<CargoSnapshot> cargoHistory = new();
    Stack<ShieldSnapshot> shieldHistory = new();
    Stack<ModeSnapshot> modeHistory = new();

    public bool CanInject => TimeLoop.IsPlaying && !isInjecting && (mode switch
    {
        InjectorMode.Shield => currentContents.Count > 0,
        InjectorMode.Engine => currentContents.Count == correctCombination.Count,
        _ => false,
    });
    public InjectorMode Mode
    {
        get => mode;
        set
        {
            if (mode == value) return;
            mode = value;
            modeHistory.Push(new ModeSnapshot
            {
                time = TimeLoop.CurrentTime,
                prev = mode,
                next = value,
            });
        }
    }
    public CargoType ShieldType => shieldType;

    private void Awake()
    {
        isInjecting = false;
        cargoHistory = new();
        shieldHistory = new();
        modeHistory = new();
    }

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
        while (shieldHistory.Count > 0 && shieldHistory.Peek().time > TimeLoop.CurrentTime)
        {
            var snapshot = shieldHistory.Pop();
            shieldType = snapshot.prev;
        }
        while (modeHistory.Count > 0 && modeHistory.Peek().time > TimeLoop.CurrentTime)
        {
            var snapshot = modeHistory.Pop();
            mode = snapshot.prev;
        }

        var modeText = mode switch
        {
            InjectorMode.Shield => ">Shield<  Engine ",
            InjectorMode.Engine => " Shield  >Engine<",
            _ => " Shield   Engine ",
        };

        var statusText = "";
        if (isInjecting)
        {
            statusText = "Injecting...";
        }
        else if (CanInject)
        {
            statusText = "Ready";
        }
        else if (mode == InjectorMode.Engine)
        {
            statusText = $"Insert Materials ({currentContents.Count}/{correctCombination.Count})";
        }
        else if (mode == InjectorMode.Shield)
        {
            statusText = $"Insert Material ({currentContents.Count}/1)";
        }

        var shieldText = shieldType != CargoType.None ? shieldType.GetDisplayName() : "Awaiting Injection";

        var contentsText = currentContents.Count > 0 ? string.Join("\n", currentContents.Select(c => c.GetDisplayName())) : "Empty";

        injectorText.text = $"Current Shield:\n{shieldText}\n\nInjector Mode:\n{modeText}\n\n{statusText}\n\nInjector Contents:\n{contentsText}";
    }

    public bool TryInject()
    {
        if (!CanInject) return false;
        {
            isInjecting = true;

            injectSound.Play();

            if (mode == InjectorMode.Shield)
            {
                var cargoType = currentContents[0];
                cargoHistory.Push(new CargoSnapshot
                {
                    time = TimeLoop.CurrentTime,
                    cargoType = cargoType,
                    added = false,
                });
                currentContents.RemoveAt(0);
                shieldHistory.Push(new ShieldSnapshot
                {
                    time = TimeLoop.CurrentTime,
                    prev = shieldType,
                    next = cargoType,
                });
                shieldType = cargoType;
                isInjecting = false;
            }
            else if (mode == InjectorMode.Engine)
            {
                var correct = currentContents.Count == correctCombination.Count && currentContents.TrueForAll(c => correctCombination.Contains(c));

                foreach (var cargoType in currentContents)
                {
                    cargoHistory.Push(new CargoSnapshot
                    {
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
        return true;
    }

    public bool AddCargo(CargoType cargoType)
    {
        if (cargoType == CargoType.None || currentContents.Count >= correctCombination.Count) return false;
        currentContents.Add(cargoType);
        cargoHistory.Push(new CargoSnapshot
        {
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

    struct ShieldSnapshot
    {
        public float time;
        public CargoType prev;
        public CargoType next;
    }

    struct ModeSnapshot
    {
        public float time;
        public InjectorMode prev;
        public InjectorMode next;
    }
}
