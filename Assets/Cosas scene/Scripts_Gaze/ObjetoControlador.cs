using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjetoControlador : MonoBehaviour
{
    [SerializeField] private GameObject pointer;
    [SerializeField] private float maxDistancePointer = 4.5f;
    [Range(0, 1)]
    [SerializeField] private float disPointerObject = 0.95f;

    private const float _maxDistance = 10;
    private GameObject _gazedAtObject = null;

    private readonly string interactableTag = "Interactable";
    private float scaleSize = 0.025f;
    private float holdDuration = 10f; // Duración en segundos que el objeto se mantiene frente a la cámara

    private Vector3 originalPosition; // Almacena la posición original del objeto

    private void Start()
    {
        GazeManager.Instance.OnGazeSelection += GazeSelection;
        originalPosition = _gazedAtObject.transform.position; // Guarda la posición original al inicio
    }

    private void GazeSelection()
    {
        if (_gazedAtObject != null && _gazedAtObject.CompareTag(interactableTag))
        {
            StartCoroutine(MoveObjectWithCamera(_gazedAtObject));
        }
        else
        {
            _gazedAtObject?.SendMessage("OnPointerClick", null, SendMessageOptions.DontRequireReceiver);
        }
    }

    private IEnumerator MoveObjectWithCamera(GameObject obj)
    {
        float timer = 0f;
        // Después de 10 segundos, desactiva el objeto "pointer"
        pointer.SetActive(false);
        while (timer < holdDuration)
        {
            // Obtiene la posición central de la cámara y la utiliza como la nueva posición del objeto
            Vector3 cameraCenterPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, disPointerObject));
            obj.transform.position = cameraCenterPosition;

            // Obtiene la rotación de la cámara y aplica una rotación adicional en el eje X
            Quaternion targetRotation = Camera.main.transform.rotation * Quaternion.Euler(90f, 0f, 0f);

            // Aplica la rotación al objeto
            obj.transform.rotation = targetRotation;

            timer += Time.deltaTime;
            yield return null; // Espera al siguiente frame
        }

        if(timer >= 10f)
        {
            obj.transform.position = originalPosition;
        }      
        
        // Verifica si el objeto aún existe antes de realizar cualquier acción adicional
        if (obj != null)
        {
            // Realiza cualquier otra acción adicional que necesites
            obj.transform.position = originalPosition;
        }

        // Reactiva el objeto "pointer" después de haber pasado el tiempo de espera
        yield return new WaitForSeconds(2f); // Puedes ajustar el tiempo según sea necesario
        pointer.SetActive(true);
    }

    public void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, _maxDistance))
        {
            if (_gazedAtObject != hit.transform.gameObject)
            {
                _gazedAtObject?.SendMessage("OnPointerExit", null, SendMessageOptions.DontRequireReceiver);
                _gazedAtObject = hit.transform.gameObject;
                _gazedAtObject.SendMessage("OnPointerEnter", null, SendMessageOptions.DontRequireReceiver);
                GazeManager.Instance.StartGazeSelection();
            }
            if (hit.transform.CompareTag(interactableTag))
            {
                PointerOnGaze(hit.point);
            }
            else
            {
                PointerOutGaze();
            }
        }
        else
        {
            _gazedAtObject?.SendMessage("OnPointerExit", null, SendMessageOptions.DontRequireReceiver);
            _gazedAtObject = null;
        }

        if (Google.XR.Cardboard.Api.IsTriggerPressed)
        {
            _gazedAtObject?.SendMessage("OnPointerClick", null, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void PointerOnGaze(Vector3 hitPoint)
    {
        float ScaleFactor = scaleSize * Vector3.Distance(transform.position, hitPoint);
        pointer.transform.localScale = Vector3.one * ScaleFactor;
        pointer.transform.parent.position = CalculatePointerPosition(transform.position, hitPoint, disPointerObject);
    }

    private void PointerOutGaze()
    {
        pointer.transform.localScale = Vector3.one * 0.1f;
        pointer.transform.parent.transform.localPosition = new Vector3(0, 0, maxDistancePointer);
        pointer.transform.parent.parent.transform.rotation = transform.rotation;
        GazeManager.Instance.CancelGazeSelection();
    }

    private Vector3 CalculatePointerPosition(Vector3 p0, Vector3 p1, float t)
    {
        float x = p0.x + t * (p1.x - p0.x);
        float y = p0.y + t * (p1.y - p0.y);
        float z = p0.z + t * (p1.z - p0.z);
        return new Vector3(x, y, z);
    }
}
