using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionPoint : MonoBehaviour
{
    public enum TransitionType
    {
        SameScene, DifferentScene
    }
    [Header("Transition Information")]
    public string sceneName;
    public TransitionType transitionType;

    public TransitionDestination.DestinationTag destinationTag;

    private bool canTrans;
    public GameObject PortalTutorial;

    void Update() {
        if (Input.GetKeyDown(KeyCode.F) && canTrans) {
            SceneController.Instance.TransitionToDestination(this);
        }
    }

    void Start() {
    }

    void OnTriggerStay(Collider other) {
        if(other.CompareTag("Player")) {
            canTrans = true;
            PortalTutorial.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other) {
        if(other.CompareTag("Player")) {
            canTrans = false;
            PortalTutorial.SetActive(false);
        }
    }
}
