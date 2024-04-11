using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectSelectorManager : MonoBehaviour
{
    public GameObject objectSelectorPrefab; // Referência ao prefab do seletor de objeto
    public GameObject addButton; // Referência ao botão de adicionar objeto
    public Transform objectSelectorsContainer; // Container onde os seletores de objeto serão instanciados
    private int objectCounter = 0; // Counter to keep track of the number of objects added
    private int areaCounter = 0; // Counter to keep track of the number of objects added


    private List<GameObject> objectSelectors = new List<GameObject>(); // Lista para manter um registro dos seletores adicionados

        public void AddObjectSelector()
        {
            GameObject newSelector = Instantiate(objectSelectorPrefab, objectSelectorsContainer);
            objectSelectors.Add(newSelector); // Add the new selector to the list 

            // Find the "ObjectName" TextMeshPro component within the "ObjectSelectorPrefab(Clone)" GameObject
            TMP_Text objectNameTextMeshPro = newSelector.transform.Find("ObjectName").GetComponent<TMP_Text>();

            // Check if the "ObjectName" TextMeshPro component is found
            if (objectNameTextMeshPro != null)
            {
                // Update the object name property based on the counter value
                objectCounter++;
                objectNameTextMeshPro.text = "Object " + objectCounter.ToString() + ":";
            }
            else
            {
                Debug.LogError("TextMeshPro component 'ObjectName' not found in the instantiated prefab.");
            }
        }
        public void DeleteLastObjectSelector()
        {
            // Check if there are any objects to delete
            if (objectSelectors.Count > 0)
            {
                // Get the last added object
                GameObject lastObject = objectSelectors[objectSelectors.Count - 1];

                // Remove it from the list
                objectSelectors.RemoveAt(objectSelectors.Count - 1);

                // Destroy the GameObject associated with it
                Destroy(lastObject);
                
                // Decrement the object counter
                objectCounter--;
            }
            else
            {
                Debug.LogWarning("No object to delete.");
            }
        }

    // Método para inicializar as opções dos Dropdowns de cada seletor, se necessário
    private void InitializeSelectorOptions(GameObject selector)
    {
        // Aqui você pode inicializar as opções dos Dropdowns do prefab, se necessário
    }
}