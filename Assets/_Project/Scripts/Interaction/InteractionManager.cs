using UnityEngine;

namespace MetaEdu.Interaction
{
    public class InteractionManager : MonoBehaviour
    {
        public static InteractionManager Instance { get; private set; }

        public float interactionDistance = 3f;
        public LayerMask interactableLayer;

        private Transform mainCameraTransform;
        private IInteractable currentInteractable;

        public System.Action<string> OnInteractionPromptChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            var camera = Camera.main;
            if (camera != null)
            {
                mainCameraTransform = camera.transform;
            }
        }

        private void Update()
        {
            if (mainCameraTransform == null)
            {
                var camera = Camera.main;
                if (camera != null)
                {
                    mainCameraTransform = camera.transform;
                }
                return;
            }

            CheckForInteractable();

            if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
            {
                currentInteractable.Interact();
            }
        }

        private void CheckForInteractable()
        {
            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    if (currentInteractable != interactable)
                    {
                        currentInteractable = interactable;
                        OnInteractionPromptChanged?.Invoke(currentInteractable.GetInteractionPrompt());
                    }
                    return;
                }
            }

            if (currentInteractable != null)
            {
                currentInteractable = null;
                OnInteractionPromptChanged?.Invoke(string.Empty);
            }
        }
    }
}
